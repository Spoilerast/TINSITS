using System;
using Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using CC = UnityEngine.InputSystem.InputAction.CallbackContext;

namespace Inputs
{
	internal interface ICameraPushable //todo monobeh-free classes design
	{
		abstract bool IsPushable();

		abstract void PushCameraPivotTo(in Vector3 position);//TODO: make summary

		abstract void PushCameraPivotTo(in Vector3 position, in float stickInSeconds);
	}

	[DisallowMultipleComponent,
	SelectionBase,
	RequireComponent(typeof(Rigidbody),
					typeof(SphereCollider))]
	internal sealed class CameraInput_Async : Monos.SceneSystem, ICameraPushable //todo: camera behavior is little bit weird
	{
		[SerializeField, Delayed,
			Range(3, 100)]
		private float _moveSpeed = 15f;

		[SerializeField, Delayed,
					Range(20, 500)]
		private float _rotateSpeed = 100f;

		[SerializeField, Delayed,
			Range(0, 200)]
		[Tooltip("Indent from screen borders for camera scrolling when cursor at edge of screen")]
		private int _screenIndentSize = 10;

		[SerializeField, Delayed,
			Range(10, 100)]
		private float _zoomSpeed = 40f;

		private const float //camera x axis rotation clamp from 0 to -70 degrees
		XRotationMin = 290,
		XRotationMax = 0, //
		MinBuffer = XRotationMin - 100,
		MaxBuffer = XRotationMax + 100;

		private const int
			ZoomMin = 2,
			ZoomMax = 42;

		//todo to much responsibilities
		private Camera _camera;

		private InputActions.CameraActions _actions;
		private PlayerInput _playerInput;

		private Indents _indents;
		private ControlDevice _currentDevice = 0;
		private bool _isInitialized = false;
		private bool _scrollingAllowed = false;
		private bool _moveAllowed = true;

		private enum ControlDevice
		{
			None, Keyboard, Gamepad//, Touch
		}

		private void Start()
			=> StartProcedures();

		private void OnEnable()
			=> StartProcedures();

		private void OnDisable()
		{
			_actions.Exit.performed -= Exit_performed; //remove
			_actions.Restart.performed -= CameraRestart; //consider

			_actions.Zoom.started -= Zoom_started;
			_actions.Movement.started -= Movement_started;
			_actions.MouseMovement.started -= MouseMovement_started;
			_actions.RotationX.started -= RotationX_started;
			_actions.RotationY.started -= RotationY_started;

			_actions.Disable();

			_playerInput.onControlsChanged -= OnControlsChanged;
		}

		private float MoveSpeed
			=> _moveSpeed * Time.deltaTime;

		private float RotateSpeed
			=> _rotateSpeed * Time.deltaTime;

		private float ZoomSpeed
			=> _zoomSpeed * Time.deltaTime;

		bool ICameraPushable.IsPushable()
			=> _currentDevice is ControlDevice.Gamepad;

		void ICameraPushable.PushCameraPivotTo(in Vector3 position)
		{
			transform.position = position;
		}

		void ICameraPushable.PushCameraPivotTo(in Vector3 position, in float stickInSeconds)
		{
			transform.position = position;
			TemporalDisallowMove();
		}

		internal void Initialize(Camera camera, PlayerInput playerInput)
		{
			(_camera, _playerInput) = (camera, playerInput);
			_actions = InputsWrapper.Actions.Camera;
			_isInitialized = true;
		}

		internal void SetCameraPosition(Vector3 pos)
			=> transform.position = pos;

		private static void Exit_performed(CC obj)
			=> EditorApplication.ExitPlaymode();

		private void CameraRestart(CC obj)
		{
			transform.SetPositionAndRotation(Vector3.zero, default);
			_camera.fieldOfView = 10;
		}

		private async void EdgeScrolling()
		{
			Vector2 moveVector;
			Vector3 newPosition;
			InputAction action = _actions.MouseMovement; //todo maybe more cashing?
														 //(_moveAction, _zoomAction, etc) not pretty but less calls
			while (_scrollingAllowed)
			{
				if (!action.inProgress
					&& _moveAllowed
					&& IsMouseAtScreenEdge(out moveVector))
				{
					newPosition = new Vector3(moveVector.x, 0f, moveVector.y);
					transform.Translate(newPosition, Space.Self);
				}

				await Awaitable.NextFrameAsync();
			}
		}

		private void EdgeScrollingSetup()
		{
			if (_screenIndentSize == 0)
				return;

			PeekLogger.LogName();
			_indents = new Indents(
				right: Screen.width - _screenIndentSize,
				top: Screen.height - _screenIndentSize,
				left: _screenIndentSize,
				bottom: _screenIndentSize
				);
			_scrollingAllowed = true;
			EdgeScrolling();
		}

		private bool IsMouseAtScreenEdge(out Vector2 moveVector)
		{
			short partX, partY;
			partX = partY = 0;
			Vector3 mPos = Input.mousePosition;

			if (mPos.x < _indents.leftEdge)
				partX = -1;
			else if (mPos.x > _indents.rightEdge)
				partX = 1;

			if (mPos.y < _indents.bottomEdge)
				partY = -1;
			else if (mPos.y > _indents.topEdge)
				partY = 1;

			if (partX == 0 && partY == 0)
			{
				moveVector = default;
				return false;
			}

			moveVector = MoveSpeed * new Vector2(partX, partY);
			return true;
		}

		private async void MouseMovement_started(CC context)
		{
			Vector3 x, y;
			_currentDevice = ControlDevice.Keyboard;
			InputAction action = _actions.MouseMovement;
			while (action.inProgress)
			{
				x = Input.GetAxisRaw("Mouse X") * Vector3.left; //somehow old input here "feels better"
				y = Input.GetAxisRaw("Mouse Y") * Vector3.back; //alternate method at EOF
				transform.Translate(x + y);

				await Awaitable.NextFrameAsync();
			}

			if (_scrollingAllowed)
				TemporalDisallowMove(); //when cursor moved at edge of screen it cause countermovement
		}

		private async void Movement_started(CC context)
		{
			Vector3 moveVector, newPosition;
			InputAction action = _actions.Movement;
			while (action.inProgress)
			{
				if (_moveAllowed)
				{
					moveVector = MoveSpeed * action.ReadValue<Vector2>();
					newPosition = new(moveVector.x, 0f, moveVector.y);
					transform.Translate(newPosition, Space.Self);
				}

				await Awaitable.NextFrameAsync();
			}
		}

		private void OnControlsChanged(PlayerInput obj)
		{
			_currentDevice = obj.currentControlScheme switch
			{
				nameof(ControlDevice.Keyboard) => ControlDevice.Keyboard,
				nameof(ControlDevice.Gamepad) => ControlDevice.Gamepad,
				//nameof(ControlDevice.Touch) => ControlDevice.Touch,
				_ => ControlDevice.None
			};
		}

		private async void RotationX_started(CC context)
		{
			float rotationValue;
			Vector3 euler;
			InputAction action = _actions.RotationX;
			while (action.inProgress)
			{
				rotationValue = RotateSpeed * _actions.RotationX.ReadValue<float>();
				transform.Rotate(rotationValue, 0, 0);
				euler = transform.rotation.eulerAngles;

				if (euler.x is >= XRotationMax and < MaxBuffer)
				{
					transform.rotation = Quaternion.Euler(XRotationMax, euler.y, euler.z);
					break;
				}

				if (euler.x is <= XRotationMin and > MinBuffer)
				{
					transform.rotation = Quaternion.Euler(XRotationMin, euler.y, euler.z);
					break;
				}
				await Awaitable.NextFrameAsync();
			}
		}

		private async void RotationY_started(CC context)
		{
			float rotationValue;
			InputAction action = _actions.RotationY;
			while (action.inProgress)
			{
				rotationValue = RotateSpeed * action.ReadValue<float>();
				transform.Rotate(0, rotationValue, 0, Space.World);

				await Awaitable.NextFrameAsync();
			}
		}

		private void StartProcedures()
		{
			if (!_isInitialized)
				return;

			EdgeScrollingSetup();

			_actions.Enable();

			_actions.Exit.performed += Exit_performed; //remove
			_actions.Restart.performed += CameraRestart; //consider

			_actions.Zoom.started += Zoom_started;
			_actions.Movement.started += Movement_started;
			_actions.MouseMovement.started += MouseMovement_started;
			_actions.RotationX.started += RotationX_started;
			_actions.RotationY.started += RotationY_started;

			_playerInput.onControlsChanged += OnControlsChanged;

			OnControlsChanged(_playerInput);
		}

		private async void TemporalDisallowMove(float seconds = 0.3f)
		{
			_moveAllowed = false;
			await Awaitable.WaitForSecondsAsync(seconds);
			_moveAllowed = true;
		}

		private async void Zoom_started(CC context)
		{
			InputAction action = _actions.Zoom;
			while (action.inProgress)
			{
				_camera.fieldOfView += ZoomSpeed * action.ReadValue<float>();
				if (_camera.fieldOfView <= ZoomMin)
				{
					_camera.fieldOfView = ZoomMin;
					break;
				}

				if (_camera.fieldOfView >= ZoomMax)
				{
					_camera.fieldOfView = ZoomMax;
					break;
				}
				await Awaitable.NextFrameAsync();
			}
		}

		private readonly struct Indents
		{
			public readonly float
				rightEdge,
				topEdge,
				leftEdge,
				bottomEdge;

			public Indents(float right, float top, float left, float bottom)
				=> (rightEdge, topEdge, leftEdge, bottomEdge) = (right, top, left, bottom);
		}

		/*
			Mouse movement based on InputSystem.
			Action type: PassThrough,
			Control type: Delta,
			Processor: Normalize		*/
		/*
	   private async void MouseMovement_started(CC context)
			   {
				   Vector3 x, y;
				   Vector2 d;
				   var action = _inputs.Camera.MouseMovement;
				   var delta = _inputs.Camera.MouseDelta;
				   while (action.inProgress)
				   {
					   d = delta.ReadValue<Vector2>();
					   x = Time.deltaTime * d.x * Vector3.left;
					   y = Time.deltaTime * d.y * Vector3.back;
					   transform.Translate(x + y);

					   await Awaitable.NextFrameAsync();
				   }
			   }*/
	}
}