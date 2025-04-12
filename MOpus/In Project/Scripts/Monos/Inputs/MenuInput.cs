using Extensions;
using Monos;
using Monos.Systems;
using NotMonos.Previews;
using UnityEngine;
using CC = UnityEngine.InputSystem.InputAction.CallbackContext;

namespace Inputs
{
[DisallowMultipleComponent]
internal sealed class MenuInput : SceneSystem
{
	private CornerMenu _cornerMenu;
	private InputActions _inputs;
	private bool _menuOpenedYet;
	private PreviewsController _previewsController; //todo interface?

	internal void Initialize(CornerMenu cornerMenu, PreviewsController previewsController)
	{
		_previewsController = previewsController;
		_cornerMenu = cornerMenu;
		_cornerMenu.OnMenuOpened += OnMenuOpened;
		_cornerMenu.OnMenuClosed += OnMenuClosed; //todo unsubscribe

		_inputs = InputsWrapper.Actions;
		InputActions.MenuActions actions = _inputs.Menu;
		actions.Enable();
		actions.ToggleCornerMenu.performed += ToggleCornerMenu_performed;
		actions.CancelByMouse.performed += InvokeCancel;
		actions.CancelByKey.performed += InvokeCancel;
	}

	private void InvokeCancel(CC obj)
	{
		PeekLogger.LogName();
		_previewsController.InvokeCancel();
	}

	private void OnMenuClosed()
	{
		PeekLogger.LogName();
		_inputs.UI.Disable();
		_inputs.Camera.Enable();
		_inputs.Interaction.Enable();
	}

	private void OnMenuOpened()
	{
		PeekLogger.LogName();
		_inputs.Camera.Disable();
		_inputs.Interaction.Disable();
		_inputs.UI.Enable();
	}

	private void ToggleCornerMenu_performed(CC obj)
	{
		PeekLogger.LogName();
		CornerMenuState state = !_menuOpenedYet
			? CornerMenuState.Opened
			: CornerMenuState.Closed;
		_cornerMenu.ToggleMenu(state);
		_menuOpenedYet = !_menuOpenedYet;
	}
}
}