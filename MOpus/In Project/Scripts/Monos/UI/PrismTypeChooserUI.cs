using Extensions;
using Monos.Backstage.Previews;
using NotMonos;
using NotMonos.Processors;
using UnityEngine;
using UnityEngine.UIElements;
using Elements = NotMonos.UI.Enums.Chooser_P;

namespace Monos.Systems
{
	public sealed class PrismTypeChooserUI : SceneUI
	{
		[SerializeField] private ConnectionsLayout _links;

		private UnitId _current;
		private GroupBox _groupAll;
		private Label _label;
		private PreviewsLayout _layout;
		private Button _setC_Button;
		private Button _setL_Button;
		private Button _setR_Button;

		internal void Initialize(PreviewsLayout layout)
		{
			_layout = layout;

			UIDocument uiDocument = GetComponent<UIDocument>();
			uiDocument.FindVisualElement(Elements.All, out _groupAll);
			_groupAll.FindVisualElement(Elements.Buttons, out GroupBox groupButtons);
			_groupAll.FindFirstVisualElement(out _label);

			groupButtons.FindVisualElement(Elements.SetL_Button, out _setL_Button);
			groupButtons.FindVisualElement(Elements.SetR_Button, out _setR_Button);
			groupButtons.FindVisualElement(Elements.SetC_Button, out _setC_Button);
		}

		internal void ToogleVisiblity(bool isVisible = false)
		{
			_groupAll.visible = isVisible;
		}

		private void ChangeLabel(PrismType type) =>
			_label.text = type switch
			{
				PrismType._L => "L-type",
				PrismType._R => "R-type",
				PrismType._C => "C-type",
				_ => "error"
			};

		private void HideWindow() => ToogleVisiblity();

		private void OnDisable()
		{
			_layout.UnitSelected -= PrismSelected;
			_layout.UnitUnselected -= HideWindow;
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
		}

		private void OnEnable()
		{
			_current = null;
			_layout.UnitSelected += PrismSelected;
			_layout.UnitUnselected += HideWindow;
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

			ToogleVisiblity();
		}

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
			if (ChooseTypeProcessor.IsNotClustered(_current, out _))
			{
				ChooseTypeProcessor.SetPrismType(_current, type);
				HideWindow();
				return;
			}
			HideWindow();
			//ChooseTypeProcessor.SetPrismTypeForCluster(clusterId,type); todo unfinished
			//_links.MakeLinks();
		}

		private void SetTypeButtonClicked(ClickEvent evt, PrismType type)
			=> SetType(type);

		private void SetTypeButtonClicked(EventBase evt, PrismType type)
			=> SetType(type);
	}
}