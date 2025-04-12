using UnityEngine;

namespace Monos.Scene
{
[RequireComponent(typeof(SphereCollider))]
internal sealed class StickObject : SceneObject //todo unfinished. suspended at current time
{
	/*[SerializeField] private Transform _stickPosition;
	private bool _isNotStickable;

	private void OnMouseEnter() { CollisionEnter.SafeInvoke(); }

	private void OnMouseExit() { CollisionExit.SafeInvoke(); }

	//IDissalowCameraMove _camera;
	//private void Start()
	//{
	//	_ = this.TryFindSingleInterface(out _camera);
	//      }
	/*private void OnTriggerEnter(Collider other)
	{
		/*	if (UnityEngine.InputSystem.Gamepad.current.rightStick.magnitude is 0) //TODO
				return;#2#
		if (_isNotStickable)
			return;

		if (!other.gameObject.TryGetComponent(out ICameraPushable camera)
			&& camera.IsPushable())
			return;

		camera.PushCameraPivotTo(_stickPosition.position, 0.3f); //TODO: make stick duration customizable
		PreventCollisionForTime();
		CollisionEnter.SafeInvoke();
	}#1#

	private void OnTriggerExit(Collider other) { CollisionExit.SafeInvoke(); }

	internal event Action CollisionEnter;

	internal event Action CollisionExit;

	private async void PreventCollisionForTime(float seconds = 1f)
	{
		_isNotStickable = true;
		await Awaitable.WaitForSecondsAsync(seconds);
		_isNotStickable = false;
	}*/
}

internal interface IStickObject
{
	//abstract Vector3 Position { get; }
}
}