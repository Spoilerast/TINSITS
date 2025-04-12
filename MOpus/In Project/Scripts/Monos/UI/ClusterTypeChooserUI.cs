using Extensions;
using NotMonos;
using NotMonos.Previews;
using UnityEngine;
using UnityEngine.UIElements;
using Elements = NotMonos.UI.Enums.Chooser_C;

namespace Monos.Systems
{
[RequireComponent(typeof(UIDocument))]
public sealed class ClusterTypeChooserUI : SceneUI
{
	private Button _cancelButton;
	private GroupBox _groupAll;
	private PreviewsController _previewsController;
	private Button _setButtonC;
	private Button _setButtonL;
	private Button _setButtonR;

	private void OnEnable()
	{
		_previewsController.ChooseClusterType += ShowWindow;
		_previewsController.ClearAllEvent += HideWindow; //todo
		_previewsController.UnitUnselected += HideWindow;

		_setButtonC
			.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._C)
			.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._C);
		_setButtonR
			.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._R)
			.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._R);
		_setButtonL
			.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._L)
			.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._L);
		_cancelButton
			.RegisterCallbackElement<ClickEvent>(CancelButtonClicked)
			.RegisterCallback<NavigationSubmitEvent>(CancelButtonClicked);

		HideWindow();
	}

	private void OnDisable()
	{
		_previewsController.ChooseClusterType -= ShowWindow;
		_previewsController.ClearAllEvent -= HideWindow;

		_setButtonC
			.UnregisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked)
			.UnregisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked);
		_setButtonR
			.UnregisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked)
			.UnregisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked);
		_setButtonL
			.UnregisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked)
			.UnregisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked);
		_cancelButton
			.UnregisterCallbackElement<ClickEvent>(CancelButtonClicked)
			.UnregisterCallback<NavigationSubmitEvent>(CancelButtonClicked);
	}

	internal void HideWindow() { ToogleVisiblity(); }

	internal void Initialize(PreviewsController previewsController)
	{
		_previewsController = previewsController;

		var uiDocument = GetComponent<UIDocument>();

		uiDocument.FindVisualElement(Elements.All, out _groupAll);
		_groupAll.FindVisualElement(Elements.Buttons, out GroupBox groupButtons);

		groupButtons.FindVisualElement(Elements.SetL_Button, out _setButtonL);
		groupButtons.FindVisualElement(Elements.SetR_Button, out _setButtonR);
		groupButtons.FindVisualElement(Elements.SetC_Button, out _setButtonC);
		_groupAll.FindVisualElement(Elements.Cancel_Button, out _cancelButton);
	}

	internal void ToogleVisiblity(bool isVisible = false) { _groupAll.visible = isVisible; }

	private void Cancel()
	{
		HideWindow();
		_previewsController.InvokeCancel();
	}

	private void CancelButtonClicked(EventBase evt) { Cancel(); }

	private void CancelButtonClicked(ClickEvent evt) { Cancel(); }

	private void SetType(PrismType type)
	{
		_previewsController.ConfirmPickedPreview(type);
		HideWindow();
	}

	private void SetTypeButtonClicked(ClickEvent evt, PrismType type) { SetType(type); }

	private void SetTypeButtonClicked(EventBase evt, PrismType type) { SetType(type); }

	private void ShowWindow() { ToogleVisiblity(true); }
}
}