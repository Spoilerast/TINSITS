using System;
using Extensions;
using Inputs;
using UnityEngine;

namespace Monos.Scene
{
	[RequireComponent(typeof(SphereCollider))]
	internal sealed class StickObject : SceneObject //todo unfinished. suspended at current time 
	{
		[SerializeField] private Transform _stickPosition;
		private bool _isNotStickable;

		internal event Action CollisionEnter;

		internal event Action CollisionExit;

		private async void PreventCollisionForTime(float seconds = 1f)
		{
			_isNotStickable = true;
			await Awaitable.WaitForSecondsAsync(seconds);
			_isNotStickable = false;
		}

		//IDissalowCameraMove _camera;
		//private void Start()
		//{
		//	_ = this.TryFindSingleInterface(out _camera);
		//      }
		private void OnTriggerEnter(Collider other)
		{
			/*	if (UnityEngine.InputSystem.Gamepad.current.rightStick.magnitude is 0) //TODO
					return;*/
			if (_isNotStickable)
				return;

			if (!other.gameObject.TryGetComponent<ICameraPushable>(out var camera)
				&& camera.IsPushable())
				return;

			camera.PushCameraPivotTo(_stickPosition.position, 0.3f); //TODO: make stick duration customizable
			PreventCollisionForTime();
			CollisionEnter.SafeInvoke();			
		}

		private void OnTriggerExit(Collider other)
		{
			CollisionExit.SafeInvoke();
		}

		private void OnMouseEnter()
		{
			CollisionEnter.SafeInvoke();
		}

		private void OnMouseExit()
		{
			CollisionExit.SafeInvoke();
		}
	}

	internal interface IStickObject
	{
		//abstract Vector3 Position { get; }
	}
}