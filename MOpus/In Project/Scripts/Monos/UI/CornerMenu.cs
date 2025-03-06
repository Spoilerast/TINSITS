using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos;
using UnityEngine;
using UnityEngine.UIElements;
using Elements = NotMonos.UI.Enums.CornerMenu;

namespace Monos.Systems
{
	internal enum CornerMenuState : byte
	{
		Opened, Closed
	}

	[RequireComponent(typeof(UIDocument))]
	internal sealed class CornerMenu : SceneUI
	{
		private Button _buttonLoad;
		private Button _buttonSave;
		private Button _buttonSpawnMode;
		private Button _buttonChangeTeam;
		private UIDocument _document;
		private Foldout _foldout;
		private SceneObjectsSpawner _spawner;
		private Label _currentTeam;

		internal event Action OnMenuClosed;

		internal event Action OnMenuOpened;

		private void OnValidate()
		{
			_ = this.TryGetComponentIfNull(ref _document);
			_ = this.TryGetComponentIfNull(ref _spawner);
		}

		private void Start()
		{
			_document.FindFirstVisualElement(out _foldout);
			_document.FindFirstVisualElement(out _currentTeam);

			FindButtonAndRegisterCallbacks(ref _buttonChangeTeam, Elements.ChangeTeamButton, ButtonClickedChangeTeam);
			FindButtonAndRegisterCallbacks(ref _buttonSpawnMode, Elements.SpawnMode, ButtonClickedSpawnMode);
			FindButtonAndRegisterCallbacks(ref _buttonSave, Elements.SaveButton, ButtonClickedSave);
			FindButtonAndRegisterCallbacks(ref _buttonLoad, Elements.LoadButton, ButtonClickedLoad);//todo unregister

			_foldout
				.RegisterCallbackElement<MouseEnterEvent>(CornerMouseEnter)
				.RegisterCallback<MouseLeaveEvent>(CornerMouseLeave);
		}

		public void Initialize(SceneObjectsSpawner spawner)
			=> _spawner = spawner;

		internal void ToggleMenu(CornerMenuState state)
			=> FoldSetState(state);

		private void ButtonClickedSpawnMode(EventBase evt)
		{
			_spawner.ToggleSpawnMode();
			FoldSetState(CornerMenuState.Closed);
		}

		private void ButtonClickedSave(EventBase evt)
		{
			SceneGlobals.SaveCurrentScene();
			FoldSetState(CornerMenuState.Closed);
		}

		private void ButtonClickedLoad(EventBase evt)
		{
			SceneGlobals.LoadCurrentScene();
			FoldSetState(CornerMenuState.Closed);
		}

		private void ButtonClickedChangeTeam(EventBase evt)
		{
			var changeTo = SceneGlobals.CurrentTeam.Equals(1)
				? TeamId.GetTeamId(2)
				: TeamId.GetTeamId(1);

			SceneGlobals.ChangePlayerTeam(changeTo);
			_currentTeam.text = $"Team {changeTo.Value}";

			FoldSetState(CornerMenuState.Closed);
		}

		private void ChangeButtonText()
		{
			if (_spawner.InSpawnMode)
				_buttonSpawnMode.text = "Exit from Spawn Mode";
			else
				_buttonSpawnMode.text = "Switch to Spawn Mode";
		}

		private void CornerMouseEnter(EventBase evt)
			=> FoldSetState(CornerMenuState.Opened);

		private void CornerMouseLeave(EventBase evt)
			=> FoldSetState(CornerMenuState.Closed);

		private void FoldSetState(CornerMenuState state)
		{
			ChangeButtonText();

			if (state is CornerMenuState.Opened)
			{
				_buttonSpawnMode.focusable = true;
				_buttonSpawnMode.Focus();
				_foldout.style.marginLeft = 1;
				OnMenuOpened.SafeInvoke();
				return;
			}

			_buttonSpawnMode.focusable = false;
			_foldout.Focus();
			_foldout.style.marginLeft = -300;
			OnMenuClosed.SafeInvoke();
		}

		/*private void HoldInputsControl()
		{
			_inputs.Camera.Disable();
			_inputs.UI.Enable();
		}
		private void RestoreInputsControl()
		{
			_inputs.Camera.Enable();
			_inputs.UI.Disable();
		}*/

		private void NavCancel(NavigationCancelEvent evt)
			=> FoldSetState(CornerMenuState.Closed);


		private void FindButtonAndRegisterCallbacks(ref Button button,
											  Elements buttonName,
											  EventCallback<EventBase> buttonClickedCallback)
		{
			_document.FindVisualElement(buttonName.ToString(), out button);

			button
				.RegisterCallbackElement<NavigationSubmitEvent>(buttonClickedCallback)
				.RegisterCallbackElement<NavigationCancelEvent>(NavCancel)
				.RegisterCallback<ClickEvent>(buttonClickedCallback);
		}

		/*todo remove from code - elements order uncontrollable
		 static IEnumerable<V> FindElementsOfType<V,E>(UIDocument document, params E[] nameEnums)
			where V:VisualElement
			where E: Enum
		{
			var names = nameEnums.Select(x=>x.ToString()).ToList();
			var builder = new UQueryBuilder<V>(document.rootVisualElement);
			return builder.Where(v=>names.Contains(v.name)).ToList();
		}*/
	}
}