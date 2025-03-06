using Extensions;
using Monos.Backstage.Previews;
using NotMonos;
using UnityEngine.UIElements;
using Elements = NotMonos.UI.Enums.Chooser_C;

namespace Monos.Systems
{
	public sealed class ClusterTypeChooserUI : SceneUI
	{
		private Button _cancel_Button;
		private GroupBox _groupAll;
		private PreviewsLayout _layout;
		private Button _setC_Button;
		private Button _setL_Button;
		private Button _setR_Button;

		internal void HideWindow()
			=> ToogleVisiblity();

		internal void Initialize(PreviewsLayout layout)
		{
			_layout = layout;

			UIDocument uiDocument = GetComponent<UIDocument>();

			uiDocument.FindVisualElement(Elements.All, out _groupAll);
			_groupAll.FindVisualElement(Elements.Buttons, out GroupBox groupButtons);

			groupButtons.FindVisualElement(Elements.SetL_Button, out _setL_Button);
			groupButtons.FindVisualElement(Elements.SetR_Button, out _setR_Button);
			groupButtons.FindVisualElement(Elements.SetC_Button, out _setC_Button);
			_groupAll.FindVisualElement(Elements.Cancel_Button, out _cancel_Button);
		}

		internal void ToogleVisiblity(bool isVisible = false)
		{
			_groupAll.visible = isVisible;
		}

		private void ShowWindow()
			=> ToogleVisiblity(true);

		private void Cancel()
		{
			HideWindow();
			_layout.InvokeCancel();
		}

		private void CancelButtonClicked(EventBase evt)
			=> Cancel();

		private void CancelButtonClicked(ClickEvent evt)
			=> Cancel();

		private void OnDisable()
		{
			_layout.ChooseClusterType -= ShowWindow;
			_layout.ClearAllEvent -= HideWindow;

			_setC_Button
				.UnregisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked)
				.UnregisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked);
			_setR_Button
				.UnregisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked)
				.UnregisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked);
			_setL_Button
				.UnregisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked)
				.UnregisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked);
			_cancel_Button
				.UnregisterCallbackElement<ClickEvent>(CancelButtonClicked)
				.UnregisterCallback<NavigationSubmitEvent>(CancelButtonClicked);
		}

		private void OnEnable()
		{
			_layout.ChooseClusterType += ShowWindow;
			_layout.ClearAllEvent += HideWindow;

			_setC_Button
				.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._C)
				.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._C);
			_setR_Button
				.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._R)
				.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._R);
			_setL_Button
				.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._L)
				.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._L);
			_cancel_Button
				.RegisterCallbackElement<ClickEvent>(CancelButtonClicked)
				.RegisterCallback<NavigationSubmitEvent>(CancelButtonClicked);

			HideWindow();
		}

		private void SetType(PrismType type)
		{
			_layout.ConfirmPickedPreview(type);
			HideWindow();
		}

		private void SetTypeButtonClicked(ClickEvent evt, PrismType type)
			=> SetType(type);

		private void SetTypeButtonClicked(EventBase evt, PrismType type)
			=> SetType(type);
	}
}