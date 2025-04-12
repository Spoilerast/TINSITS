using System;
using Extensions;
using Monos;
using Monos.Scene;
using NotMonos;
using NotMonos.Backstage;
using NotMonos.Previews;
using UnityEngine;
using UnityEngine.InputSystem;
using CC = UnityEngine.InputSystem.InputAction.CallbackContext;

namespace Inputs
{
[DisallowMultipleComponent]
internal sealed class InteractionInput : SceneSystem
{
	private InputActions.InteractionActions _actions;
	private Camera _camera;
	private InteractableObject _object;
	private PreviewsController _previewsController;

	private Ray ScreenPointToRay => _camera.ScreenPointToRay(Pointer.current.position.ReadValue());

	private void Awake()
	{
		_camera = Camera.main;
		_actions = InputsWrapper.Actions.Interaction;
		_actions.Enable();
		_actions.FirstButton.performed += FirstButton_performed;
		_actions.LeftClick.performed += LeftMouseClick;
		_actions.ScrollPreviews.performed += ScrollPreviews_performed;

		_actions.Load.performed += OnLoad; //todo move to another class
		_actions.Save.performed += OnSave;
	}

	internal void Initialize(PreviewsController previewsController)
		=> _previewsController = previewsController;

	private void FirstButton_performed(CC obj)
	{
		PeekLogger.LogName(SceneGlobals.CurrentState);
		switch (SceneGlobals.CurrentState){
			case SceneState.SpawnMode:
			case SceneState.Default:
			case SceneState.PreviewMode:
			case SceneState.SubPreviewMode:
				FirstButtonInteraction();
				return;

			case SceneState.Error: throw new InvalidOperationException();
			case SceneState.NotInteractable:
			default:
				return;
		}
	}

	private void FirstButtonInteraction()
	{
		PeekLogger.LogName(_object);
		_object.Interact();
	}

	private void LeftMouseClick(CC obj)
	{
		if (TryGetComponentFromRaycast(out _object))
			FirstButton_performed(obj);
	}

	private static void OnLoad(CC obj)
		=> SceneGlobals.LoadCurrentScene();

	private static void OnSave(CC obj)
		=> SceneGlobals.SaveCurrentScene();

	private void ScrollPreviews_performed(CC obj) { _previewsController.ScrollAllPreviews(); }

	private bool TryGetComponentFromRaycast<T>(out T component, float maxDistance = 500f)
		where T : MonoBehaviour
	{
		component = null;
		bool isCollided = Physics.Raycast(ScreenPointToRay,
										  out RaycastHit hit,
										  maxDistance);
		if (!isCollided)
			return false;

		GameObject obj = hit.transform
			? hit.transform.gameObject
			: null;

		return obj && obj.TryGetComponent(out component);
	}
}
}