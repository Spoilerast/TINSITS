using Extensions;
using Monos.Backstage.Previews;
using NotMonos;
using UnityEngine.UIElements;

namespace Monos.Systems
{
	public sealed class CancelMenuUI : SceneUI
	{
		private Button _cancel_Button;
		private GroupBox _groupAll;
		private PreviewsLayout _layout;

		private void OnDisable()
		{
			_cancel_Button
				.UnregisterCallbackElement<ClickEvent>(CancelButtonClicked)
				.UnregisterCallback<NavigationSubmitEvent>(CancelButtonClicked);
		}

		private void OnEnable()
		{
			_cancel_Button
				.RegisterCallbackElement<ClickEvent>(CancelButtonClicked)
				.RegisterCallback<NavigationSubmitEvent>(CancelButtonClicked);

			HideWindow();
		}

		internal void Initialize(PreviewsLayout layout)
		{
			_layout = layout;

			UIDocument uiDocument = GetComponent<UIDocument>();
			uiDocument.FindFirstVisualElement(out _groupAll);
			_groupAll.FindFirstVisualElement(out _cancel_Button);

			_layout.UnitSelected += UnitSelected;
			//_layout.UnitUnselected += HideWindow;
			_layout.ChooseClusterType += ShowWindow;
			_layout.ClearAllEvent += HideWindow;
		}			

		internal void HideWindow()
			=> ToogleVisiblity();

		internal void ShowWindow()
			=> ToogleVisiblity(true);

		internal void ToogleVisiblity(bool isVisible = false)
			=> _groupAll.visible = isVisible;

		private void Cancel()
		{
			PeekLogger.Log($"cancel click");
			HideWindow();
			_layout.InvokeCancel();
		}

		private void CancelButtonClicked(EventBase evt)
			=> Cancel();

		private void CancelButtonClicked(ClickEvent evt)
			=> Cancel();

		private void UnitSelected(UnitId _)
			=> ShowWindow();
	}
}