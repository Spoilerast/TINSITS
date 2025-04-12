using Extensions;
using NotMonos;
using NotMonos.Databases;
using NotMonos.Previews;
using NotMonos.Processors;
using UnityEngine;
using UnityEngine.UIElements;
using Elements = NotMonos.UI.Enums.Chooser_P;

namespace Monos.Systems
{
[RequireComponent(typeof(UIDocument))]
public sealed class PrismTypeChooserUI : SceneUI
{
	[SerializeField] private ConnectionsLayout _links;

	private UnitId _current;
	private GroupBox _groupAll;
	private Label _label;
	private PreviewsController _previewsController;
	private Button _setButtonC;
	private Button _setButtonL;
	private Button _setButtonR;

	private void OnEnable()
	{
		_current = null;
		DataCenter.Selection.UnitSelectedId += PrismSelected;
		_previewsController.UnitUnselected += HideWindow;
		_previewsController.ClearAllEvent += HideWindow;

		_setButtonC
			.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._C)
			.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._C);
		_setButtonR
			.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._R)
			.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._R);
		_setButtonL
			.RegisterCallbackElement<ClickEvent, PrismType>(SetTypeButtonClicked, PrismType._L)
			.RegisterCallback<NavigationSubmitEvent, PrismType>(SetTypeButtonClicked, PrismType._L);

		ToogleVisiblity();
	}

	private void OnDisable()
	{
		DataCenter.Selection.UnitSelectedId -= PrismSelected;
		_previewsController.UnitUnselected -= HideWindow;
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
	}

	internal void Initialize(PreviewsController previewsController)
	{
		_previewsController = previewsController;

		var uiDocument = GetComponent<UIDocument>();
		uiDocument.FindVisualElement(Elements.All, out _groupAll);
		_groupAll.FindVisualElement(Elements.Buttons, out GroupBox groupButtons);
		_groupAll.FindFirstVisualElement(out _label);

		groupButtons.FindVisualElement(Elements.SetL_Button, out _setButtonL);
		groupButtons.FindVisualElement(Elements.SetR_Button, out _setButtonR);
		groupButtons.FindVisualElement(Elements.SetC_Button, out _setButtonC);
	}

	internal void ToogleVisiblity(bool isVisible = false) { _groupAll.visible = isVisible; }

	private void ChangeLabel(PrismType type)
	{
		_label.text = type switch {
			PrismType._L => "L-type",
			PrismType._R => "R-type",
			PrismType._C => "C-type",
			_            => "error"
		};
	}

	private void HideWindow() { ToogleVisiblity(); }

	private void PrismSelected(UnitId id)
	{
		PrismType type = ChooseTypeProcessor.GetPrismType(id);
		ToogleVisiblity(true);
		_current = id;
		ChangeLabel(type);
	}

	private void SetType(PrismType type)
	{
		ChangeLabel(type);
		if (ChooseTypeProcessor.IsNotClustered(_current)){
			ChooseTypeProcessor.SetPrismType(_current, type);
			HideWindow();
			return;
		}

		HideWindow();
		//ChooseTypeProcessor.SetPrismTypeForCluster(clusterId,type); todo unfinished
		//_links.MakeLinks();
	}

	private void SetTypeButtonClicked(ClickEvent evt, PrismType type) { SetType(type); }

	private void SetTypeButtonClicked(EventBase evt, PrismType type) { SetType(type); }
}
}