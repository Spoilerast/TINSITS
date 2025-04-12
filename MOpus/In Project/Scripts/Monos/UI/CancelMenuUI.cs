using Extensions;
using NotMonos;
using NotMonos.Databases;
using NotMonos.Previews;
using UnityEngine;
using UnityEngine.UIElements;

namespace Monos.Systems
{
[RequireComponent(typeof(UIDocument))]
public sealed class CancelMenuUI : SceneUI
{
	private const string SkipName = "ButtonSkip";

	private Button _buttonCancel;
	private Button _buttonSkip;
	private GroupBox _groupAll;
	private PreviewsController _previewsController;

	private void OnEnable()
	{
		_buttonCancel
			.RegisterCallbackElement<ClickEvent>(CancelButtonClicked)
			.RegisterCallback<NavigationSubmitEvent>(CancelButtonClicked);
		_buttonSkip
			.RegisterCallbackElement<ClickEvent>(SkipButtonClicked)
			.RegisterCallback<NavigationSubmitEvent>(SkipButtonClicked);

		HideWindow();
	}

	private void OnDisable()
	{
		_buttonCancel
			.UnregisterCallbackElement<ClickEvent>(CancelButtonClicked)
			.UnregisterCallback<NavigationSubmitEvent>(CancelButtonClicked);
		_buttonSkip
			.UnregisterCallbackElement<ClickEvent>(SkipButtonClicked)
			.UnregisterCallback<NavigationSubmitEvent>(SkipButtonClicked);
	}

	internal void HideSkipButton() //todo
		=> _buttonSkip.HideElement();

	internal void HideWindow()
		=> ToogleVisiblity();

	internal void Initialize(PreviewsController previewsController)
	{
		_previewsController = previewsController;

		var uiDocument = GetComponent<UIDocument>();
		uiDocument.FindFirstVisualElement(out _groupAll);
		_groupAll.FindFirstVisualElement(out _buttonCancel);
		_groupAll.FindVisualElement(SkipName, out _buttonSkip);

		DataCenter.Selection.UnitSelectedId += UnitSelected;
		_previewsController.UnitUnselected += HideWindow;
		_previewsController.ChooseClusterType += ShowWindow;
		_previewsController.ShowCancelWindow += ShowWindow;
		_previewsController.ClearAllEvent += HideWindow;
		_previewsController.OnSubSequence += ShowSkipButton;
	}

	internal void ShowSkipButton()
		=> _buttonSkip.ShowElement();

	internal void ShowWindow()
		=> ToogleVisiblity(true);

	internal void ToogleVisiblity(bool isVisible = false)
		=> _groupAll.visible = isVisible;

	private void Cancel()
	{
		PeekLogger.Log("cancel click");
		HideWindow();
		_previewsController.InvokeCancel();
	}

	private void CancelButtonClicked(EventBase evt)
		=> Cancel();

	private void CancelButtonClicked(ClickEvent evt)
		=> Cancel();

	private void Skip()
	{
		PeekLogger.Log("skip click");
		_previewsController.InvokeSkip();
	}

	private void SkipButtonClicked(EventBase evt)
		=> Skip();

	private void SkipButtonClicked(ClickEvent evt)
		=> Skip();

	private void UnitSelected(UnitId _) { ShowWindow(); }
}
}