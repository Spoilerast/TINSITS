using System;
using System.Collections;
using Extensions;
using UnityEditor;
using UnityEngine;
using CC = UnityEngine.InputSystem.InputAction.CallbackContext;

namespace Inputs
{
	[Obsolete(
		"Async version is better. Code here is created for example to compare between two versions." +
		"Not maintained. Contains some temporal code and comments"
		), DisallowMultipleComponent]
	internal sealed class CameraInput_Coroutines : Monos.SceneSystem
	{
		private const float //camera x axis rotation clamp from 0 to -70 degrees
			X_ROTATION_MIN = 290,
			X_ROTATION_MAX = 0,
			MIN_BUFFER = X_ROTATION_MIN - 100,
			MAX_BUFFER = X_ROTATION_MAX + 100
			;

		private const int
			ZOOM_MIN = 2,
			ZOOM_MAX = 42
			;

		private Camera _camera;
		private InputActions _inputs;

		[SerializeField][Range(3, 100)] private float _moveSpeed = 15f;
		[SerializeField][Range(20, 500)] private float _rotateSpeed = 100f;
		[SerializeField][Range(10, 100)] private float _zoomSpeed = 40f;

		private float MoveSpeed => _moveSpeed * Time.deltaTime;

		private float RotateSpeed => _rotateSpeed * Time.deltaTime;

		private float ZoomSpeed => _zoomSpeed * Time.deltaTime;

		private void Awake()
		{
			//TODO: make something for branch
			_ = this.TryGetComponent_InAttachedParentalChild(out _camera);

			_inputs = InputsWrapper.Actions;

			InputActions.CameraActions actions = _inputs.Camera;
			actions.Enable();

			actions.Exit.performed += Exit_performed; //remove
			actions.Restart.performed += CameraRestart; //consider

			actions.Zoom.started += Zoom_started;
			actions.Movement.started += Movement_started;
			actions.MouseMovement.started += MouseMovement_started;
			actions.RotationX.started += RotationX_started;
			actions.RotationY.started += RotationY_started;
		}

		private void OnDisable()
		{
			InputActions.CameraActions actions = _inputs.Camera;

			actions.Exit.performed -= Exit_performed; //remove
			actions.Restart.performed -= CameraRestart; //consider

			actions.Zoom.started -= Zoom_started;
			actions.Movement.started -= Movement_started;
			actions.MouseMovement.started -= MouseMovement_started;
			actions.RotationX.started -= RotationX_started;
			actions.RotationY.started -= RotationY_started;
			actions.Disable();
		}

		private void CameraRestart(CC obj)
		{
			transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			_camera.fieldOfView = 10;
		}

		private void Exit_performed(CC obj) => EditorApplication.ExitPlaymode();

		private void MouseMovement_started(CC obj) => StartCoroutine(MouseMovementRoutine());

		private IEnumerator MouseMovementRoutine()
		{
			Vector3 x, y;
			UnityEngine.InputSystem.InputAction action = _inputs.Camera.MouseMovement;
			while (action.inProgress)
			{
				x = Input.GetAxisRaw("Mouse X") * Vector3.left; //InputSystem here is unsmooth
				y = Input.GetAxisRaw("Mouse Y") * Vector3.back;
				transform.Translate(x + y);

				yield return null;
			}
		}

		private void Movement_started(CC obj) => StartCoroutine(MovementRoutine());

		private IEnumerator MovementRoutine()
		{
			Vector3 moveVector, newPosition;
			UnityEngine.InputSystem.InputAction action = _inputs.Camera.Movement;
			while (action.inProgress)
			{
				moveVector = MoveSpeed * action.ReadValue<Vector2>();
				newPosition = new(moveVector.x, 0f, moveVector.y);
				transform.Translate(newPosition, Space.Self);

				yield return null;
			}
		}

		private void RotationX_started(CC obj) => StartCoroutine(RotationXRoutine());

		private IEnumerator RotationXRoutine()
		{
			float rotationValue;
			Vector3 euler;
			UnityEngine.InputSystem.InputAction action = _inputs.Camera.RotationX;
			while (action.inProgress)
			{
				rotationValue = RotateSpeed * _inputs.Camera.RotationX.ReadValue<float>();
				transform.Rotate(rotationValue, 0, 0);
				euler = transform.rotation.eulerAngles;

				if (euler.x is >= X_ROTATION_MAX and < MAX_BUFFER)
				{
					transform.rotation = Quaternion.Euler(X_ROTATION_MAX, euler.y, euler.z);
					yield break;
				}

				if (euler.x is <= X_ROTATION_MIN and > MIN_BUFFER)
				{
					transform.rotation = Quaternion.Euler(X_ROTATION_MIN, euler.y, euler.z);
					yield break;
				}

				yield return null;
			}
		}

		private void RotationY_started(CC obj) => StartCoroutine(RotationYRoutine());

		private IEnumerator RotationYRoutine()
		{
			float rotationValue;
			UnityEngine.InputSystem.InputAction action = _inputs.Camera.RotationY;
			while (action.inProgress)
			{
				rotationValue = RotateSpeed * action.ReadValue<float>();
				transform.Rotate(0, rotationValue, 0, Space.World);

				yield return null;
			}
		}

		private void Zoom_started(CC obj) => StartCoroutine(ZoomRoutine());

		private IEnumerator ZoomRoutine()
		{
			UnityEngine.InputSystem.InputAction action = _inputs.Camera.Zoom;
			while (action.inProgress)
			{
				_camera.fieldOfView += ZoomSpeed * action.ReadValue<float>();
				if (_camera.fieldOfView <= ZOOM_MIN)
				{
					_camera.fieldOfView = ZOOM_MIN;
					yield break;
				}

				if (_camera.fieldOfView >= ZOOM_MAX)
				{
					_camera.fieldOfView = ZOOM_MAX;
					yield break;
				}
				yield return null;
			}
		}
	}
}