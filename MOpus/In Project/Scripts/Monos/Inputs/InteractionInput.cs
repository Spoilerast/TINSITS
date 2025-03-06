using System;
using Extensions;
using Monos;
using Monos.Backstage.Previews;
using Monos.Scene;
using NotMonos;
using UnityEngine;
using CC = UnityEngine.InputSystem.InputAction.CallbackContext;

namespace Inputs
{
	[DisallowMultipleComponent]
	internal sealed class InteractionInput : SceneSystem
	{
		private InputActions.InteractionActions _actions;
		private InteractableObject _object;
		private PreviewsLayout _previewsLayout; //todo interface?

		private void Start()
		{
			_actions = InputsWrapper.Actions.Interaction;
			_actions.Enable();
			_actions.FirstButton.performed += FirstButton_performed;
			_actions.LeftClick.performed += LeftMouseClick;
			_actions.ScrollPreviews.performed += ScrollPreviews_performed;
		}

		private static bool TryGetComponentFromRaycast<T>(out T component, float maxDistance = 500f)
			where T : MonoBehaviour
		{
			component = null;
			bool isCollided = Physics.Raycast( //todo consider RaycastNonAlloc
					Camera.main.ScreenPointToRay(Input.mousePosition),
					out RaycastHit hit,
					maxDistance);
			if (!isCollided)
				return false;

			GameObject obj = hit.transform
				? hit.transform.gameObject
				: null;

			return obj && obj.TryGetComponent(out component);
		}

		internal void Initialize(PreviewsLayout previewsLayout)
			=> _previewsLayout = previewsLayout;

		private void FirstButton_performed(CC obj)
		{
			PeekLogger.LogName(SceneGlobals.CurrentState);
			switch (SceneGlobals.CurrentState)
			{
				case SceneState.SpawnMode:
				case SceneState.Default:
				case SceneState.PreviewMode:
				case SceneState.SubPreviewMode:
					FirstButtonInteraction();
					return;

				case SceneState.Error:
					throw new InvalidOperationException();
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

		private void ScrollPreviews_performed(CC obj)
			=> _previewsLayout.ScrollAllPreviews();
	}
}