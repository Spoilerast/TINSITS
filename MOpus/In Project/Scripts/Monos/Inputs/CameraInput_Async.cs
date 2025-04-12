using Extensions;
using Monos;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using CC = UnityEngine.InputSystem.InputAction.CallbackContext;

namespace Inputs
{
/*internal interface ICameraPushable //todo monobeh-free classes design
{
	bool IsPushable();

	void PushCameraPivotTo(in Vector3 position); //TODO: make summary

	void PushCameraPivotTo(in Vector3 position, in float stickInSeconds);
}*/

[DisallowMultipleComponent][SelectionBase]
internal sealed class CameraInput_Async : SceneSystem //, ICameraPushable
{
	private const float //camera x axis rotation clamp from 0 to 70 degrees
		XRotationMin = 0,
		XRotationMax = 70,
		//MinBuffer = XRotationMin - 100,
		MaxBuffer = XRotationMax + 100;

	private const int
		ZoomMin = 2,
		ZoomMax = 42;

	private const string MouseAxisX = "Mouse X";
	private const string MouseAxisY = "Mouse Y";

	[SerializeField][Range(3, 100)]
	private float _moveSpeed = 15f;

	[SerializeField][Range(20, 500)]
	private float _rotateSpeed = 100f;

	[SerializeField][Range(10, 100)]
	private float _zoomSpeed = 40f;

	[Space]
	[SerializeField][Range(0, 200)]
	[Tooltip("Indent from screen borders for camera scrolling when cursor at edge of screen")]
	private int _screenIndentSize = 10;

	[SerializeField] private Transform _yawTransform,
									   _pitchTransform;

	private InputActions.CameraActions _actions;

	//todo too much responsibilities?
	private Camera _camera;
	private ControlDevice _currentDevice = 0;
	private DefaultCamera _defaultCameraSettings;

	private Indents _indents;
	private bool _isInitialized;
	private bool _moveAllowed = true;
	private PlayerInput _playerInput;
	private bool _scrollingAllowed;

	private float MoveSpeed => _moveSpeed * Time.deltaTime;
	private float RotateSpeed => _rotateSpeed * Time.deltaTime;
	private float ZoomSpeed => _zoomSpeed * Time.deltaTime;
	public Vector3 Position => _yawTransform.position;

	private void Start()
		=> StartProcedures();

	private void OnEnable()
		=> StartProcedures();

	private void OnDisable()
	{
		_actions.Exit.performed -= Exit_performed;   //remove
		_actions.Restart.performed -= CameraRestart; //consider

		_actions.Zoom.started -= Zoom_started;
		_actions.Movement.started -= Movement_started;
		_actions.MouseMovement.started -= MouseMovement_started;
		_actions.RotationX.started -= RotationX_started;
		_actions.RotationY.started -= RotationY_started;

		_actions.Disable();

		_playerInput.onControlsChanged -= OnControlsChanged;
	}

	/*bool ICameraPushable.IsPushable()
		=> _currentDevice is ControlDevice.Gamepad;

	void ICameraPushable.PushCameraPivotTo(in Vector3 position) { transform.position = position; }

	void ICameraPushable.PushCameraPivotTo(in Vector3 position, in float stickInSeconds)
	{
		transform.position = position;
		TemporalDisallowMove();
	}
	*/

	internal void Initialize(Camera camera, PlayerInput playerInput)
	{
		(_camera, _playerInput) = (camera, playerInput);
		_actions = InputsWrapper.Actions.Camera;
		_isInitialized = true;
	}

	internal void SetCameraPosition(Vector3 pos)
	{
		//PeekLogger.LogName(pos);
		_yawTransform.position = pos;
	}

	private void CameraRestart(CC obj)
	{
		_yawTransform.position = _defaultCameraSettings.cameraPosition;
		_yawTransform.rotation = _defaultCameraSettings.yawRotation;
		_pitchTransform.rotation = _defaultCameraSettings.pitchRotation;
		_camera.fieldOfView = 10;
	}

	private async void EdgeScrolling() //todo needs logic to turn off (like on scene changing)
	{
		Vector2 moveVector;
		Vector3 newPosition;
		InputAction action = _actions.MouseMovement; //todo maybe more cashing?
		//(_moveAction, _zoomAction, etc) not pretty but fewer calls
		while (_scrollingAllowed){
			if (!action.inProgress
				&& _moveAllowed
				&& IsMouseAtScreenEdge(out moveVector)) //todo maybe some independent procedure to check cursor position and call scroll
			{
				newPosition = new(moveVector.x, 0f, moveVector.y);
				_yawTransform.Translate(newPosition);
			}

			await Awaitable.NextFrameAsync();
		}
	}

	private void EdgeScrollingSetup()
	{
		if (_screenIndentSize == 0)
			return;

		PeekLogger.LogName();
		_indents = new(Screen.width - _screenIndentSize,
					   Screen.height - _screenIndentSize,
					   _screenIndentSize,
					   _screenIndentSize);
		_scrollingAllowed = true;
		EdgeScrolling();
	}

	private static void Exit_performed(CC obj) { EditorApplication.ExitPlaymode(); }

	private bool IsMouseAtScreenEdge(out Vector2 moveVector)
	{ //maybe rethink
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

		if (partX == 0 && partY == 0){
			moveVector = default;
			return false;
		}

		moveVector = MoveSpeed * new Vector2(partX, partY);
		return true;
	}

	private async void MouseMovement_started(CC context)
	{ //caution! async void method unhandled exceptions can crash all app!
		Vector3 x, y, vector;
		_currentDevice = ControlDevice.Keyboard;
		InputAction action = _actions.MouseMovement;
		while (action.inProgress){
			x = Input.GetAxisRaw(MouseAxisX) * Vector3.left; //somehow old input here "feels better"
			y = Input.GetAxisRaw(MouseAxisY) * Vector3.back; //alternate method at EOF
			vector = x + y;                                  //.normalized;
			_yawTransform.Translate(vector);

			await Awaitable.NextFrameAsync();
		}

		if (_scrollingAllowed)
			await TemporalDisallowMove(); //when cursor moved at edge of screen it cause countermovement
	}

	private async void Movement_started(CC context)
	{
		InputAction action = _actions.Movement;
		while (action.inProgress){
			if (!_moveAllowed)
				await Awaitable.NextFrameAsync();

			Vector2 moveVector = MoveSpeed * action.ReadValue<Vector2>();
			_yawTransform.Translate(moveVector.ToGridVector());
			await Awaitable.NextFrameAsync();
		}
	}

	private void OnControlsChanged(PlayerInput obj)
	{
		_currentDevice = obj.currentControlScheme switch {
			nameof(ControlDevice.Keyboard) => ControlDevice.Keyboard,
			nameof(ControlDevice.Gamepad)  => ControlDevice.Gamepad,
			//nameof(ControlDevice.Touch) => ControlDevice.Touch,
			_ => ControlDevice.None
		};
	}

	private async void RotationX_started(CC context)
	{
		float rotationValue;
		Vector3 euler;
		InputAction action = _actions.RotationX;
		while (action.inProgress){
			rotationValue = RotateSpeed * _actions.RotationX.ReadValue<float>();
			_pitchTransform.Rotate(Vector3.right, rotationValue, Space.Self);

			euler = _pitchTransform.rotation.eulerAngles;

			if (euler.x is >= XRotationMax and < MaxBuffer){
				_pitchTransform.rotation = Quaternion.Euler(XRotationMax, euler.y, euler.z);
				break;
			}

			//euler.x.LogThis(); //0->360
			if (euler.x > MaxBuffer){
				_pitchTransform.rotation = Quaternion.Euler(XRotationMin, euler.y, euler.z);
				break;
			}

			await Awaitable.NextFrameAsync();
		}
	}

	private async void RotationY_started(CC context)
	{
		float rotationValue;
		InputAction action = _actions.RotationY;
		while (action.inProgress){
			rotationValue = RotateSpeed * action.ReadValue<float>();
			_yawTransform.Rotate(Vector3.up, rotationValue, Space.World);

			await Awaitable.NextFrameAsync();
		}
	}

	private void StartProcedures()
	{
		if (!_isInitialized)
			return;

		EdgeScrollingSetup();
		_defaultCameraSettings = new(_yawTransform, _pitchTransform);
		_actions.Enable();

		_actions.Exit.performed += Exit_performed;   //remove
		_actions.Restart.performed += CameraRestart; //consider

		_actions.Zoom.started += Zoom_started;
		_actions.Movement.started += Movement_started;
		_actions.MouseMovement.started += MouseMovement_started;
		_actions.RotationX.started += RotationX_started;
		_actions.RotationY.started += RotationY_started;

		_playerInput.onControlsChanged += OnControlsChanged;

		OnControlsChanged(_playerInput);
	}

	private async Awaitable TemporalDisallowMove(float seconds = 0.3f)
	{
		_moveAllowed = false;
		await Awaitable.WaitForSecondsAsync(seconds);
		_moveAllowed = true;
	}

	private async void Zoom_started(CC context)
	{
		InputAction action = _actions.Zoom;
		while (action.inProgress){
			_camera.fieldOfView += ZoomSpeed * action.ReadValue<float>();
			if (_camera.fieldOfView <= ZoomMin){
				_camera.fieldOfView = ZoomMin;
				break;
			}

			if (_camera.fieldOfView >= ZoomMax){
				_camera.fieldOfView = ZoomMax;
				break;
			}

			await Awaitable.NextFrameAsync();
		}
	}

	private enum ControlDevice
	{
		None, Keyboard, Gamepad //, Touch
	}

	private readonly struct Indents
	{
		public readonly float
			rightEdge,
			topEdge,
			leftEdge,
			bottomEdge;

		public Indents(float right, float top, float left, float bottom)
		{
			(rightEdge, topEdge, leftEdge, bottomEdge) = (right, top, left, bottom);
		}
	}

	private readonly struct DefaultCamera
	{
		public readonly Vector3 cameraPosition;
		public readonly Quaternion yawRotation;
		public readonly Quaternion pitchRotation;

		public DefaultCamera(Transform yaw, Transform pitch)
		{
			(cameraPosition, yawRotation, pitchRotation) = (yaw.position, yaw.rotation, pitch.rotation);
		}
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
				   _yawTransform.Translate(x + y);

				   await Awaitable.NextFrameAsync();
			   }
		   }*/
}
}