using UnityEngine;
using UnityEngine.UIElements;

namespace Monos
{
	internal class TestMenu : SceneUI //todo remove it
	{
		private Label _label;

		private void Start()
		{
			var root = GetComponent<UIDocument>();
			var mainGroup = root.rootVisualElement.Q<GroupBox>();
			_label = mainGroup.Q<Label>();
			var buttonsGroup = mainGroup.Q<GroupBox>();

			var groupA = buttonsGroup.Q<GroupBox>();
			var groupB = buttonsGroup.Q<GroupBox>("GroupB");
			var groupC = buttonsGroup.Q<GroupBox>("GroupC");

			var button1 = groupA.Q<Button>();
			var button2 = groupA.Q<Button>("But2");
			var button3 = groupA.Q<Button>("But3");
			var button4 = groupB.Q<Button>();
			var button5 = groupB.Q<Button>("But5");
			var button6 = groupB.Q<Button>("But6");
			var button7 = groupC.Q<Button>();
			var button8 = groupC.Q<Button>("But8");
			var button9 = groupC.Q<Button>("But9");

			button1.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);
			button2.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);
			button3.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);
			button4.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);
			button5.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);
			button6.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);
			button7.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);
			button8.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);
			button9.RegisterCallback<NavigationSubmitEvent>(ButtonClicked);

			button1.RegisterCallback<ClickEvent>(ButtonClicked);
			button2.RegisterCallback<ClickEvent>(ButtonClicked);
			button3.RegisterCallback<ClickEvent>(ButtonClicked);
			button4.RegisterCallback<ClickEvent>(ButtonClicked);
			button5.RegisterCallback<ClickEvent>(ButtonClicked);
			button6.RegisterCallback<ClickEvent>(ButtonClicked);
			button7.RegisterCallback<ClickEvent>(ButtonClicked);
			button8.RegisterCallback<ClickEvent>(ButtonClicked);
			button9.RegisterCallback<ClickEvent>(ButtonClicked);

			button1.RegisterCallback<FocusEvent>(ButtonFocused);
			button2.RegisterCallback<FocusEvent>(ButtonFocused);
			button3.RegisterCallback<FocusEvent>(ButtonFocused);
			button4.RegisterCallback<FocusEvent>(ButtonFocused);
			button5.RegisterCallback<FocusEvent>(ButtonFocused);
			button6.RegisterCallback<FocusEvent>(ButtonFocused);
			button7.RegisterCallback<FocusEvent>(ButtonFocused);
			button8.RegisterCallback<FocusEvent>(ButtonFocused);
			button9.RegisterCallback<FocusEvent>(ButtonFocused);
			//EditorApplication.ExitPlaymode();
		}

		private void ButtonFocused(FocusEvent evt)
		{
			_label.text = ((Button)evt.currentTarget).name + " focused";
		}

		private void ButtonClicked(EventBase evt)
		{
			_label.text = $"{((Button)evt.currentTarget).name} clicked by gamepad";
		}

		private void ButtonClicked(ClickEvent evt)
		{
			_label.text = $"{((Button)evt.currentTarget).name} clicked by mouse";
		}
	}
}