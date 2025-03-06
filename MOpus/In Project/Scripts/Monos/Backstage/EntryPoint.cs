using Extensions;
using Inputs;
using Monos.Backstage.Previews;
using Monos.Scene;
using NotMonos;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Monos.Systems
{
	internal sealed class EntryPoint : SceneSystem
	{
		[SerializeField] private Camera _mainCamera;
		[SerializeField] private CameraInput_Async _cameraInputs;
		[SerializeField] private MenuInput _menuInputs;
		[SerializeField] private InteractionInput _interaction;
		[SerializeField] private PlayerInput _playerInput;
		[SerializeField] private CornerMenu _cornerMenu;
		[SerializeField] private SceneObjectsSpawner _spawner;
		[SerializeField] private PreviewsLayout _previewsLayout;
		[SerializeField] private PrismTypeChooserUI _prismChooser;
		[SerializeField] private ClusterTypeChooserUI _clusterChooser;
		[SerializeField] private PropertiesViewerUI _propertiesViewer;
		[SerializeField] private ConnectionsLayout _connectionsLayout;
		[SerializeField] private CancelMenuUI _cancelMenu;

		private void Awake()
		{
			InputActions inputActions = InputsWrapper.Actions;
			inputActions.UI.Disable();
			_cameraInputs.Initialize(_mainCamera, _playerInput);
			_menuInputs.Initialize(_cornerMenu, _previewsLayout);
			_interaction.Initialize(_previewsLayout);
			_cornerMenu.Initialize(_spawner);

			_prismChooser.Initialize(_previewsLayout);
			_clusterChooser.Initialize(_previewsLayout);
			_propertiesViewer.Initialize(_previewsLayout);
			_cancelMenu.Initialize(_previewsLayout);

			_connectionsLayout.SubscribeOnMove(_previewsLayout);

			InitializeSceneObjects();
			SceneGlobals.SetState(SceneState.Default); //todo: remove

			ConnectUnitsOnScene();
			PeekLogger.Log("███ End of EntryPoint. Start of Play loop ███");
			//SceneGlobals.LoadCurrentScene();
			//PeekLogger.ClearLog();
		}

		private void ConnectUnitsOnScene()
		{
			NotMonos.Databases.ConnectionsDB connections = DataCenter.Connections;
			foreach (UnitId unitId in UnitId.GetAllIds)
			{
				connections.MakeConnections(unitId, ClusterStatus.NotClustered);
			}
			_connectionsLayout.MakeLinks();
		}

		private static void InitializeSceneObjects()
		{
			foreach (var item in FindObjectsByType<PowerSource>(FindObjectsSortMode.None))
				item.Initialize();

			foreach (var item in FindObjectsByType<Prism>(FindObjectsSortMode.None))
				item.Initialize();
		}

		private void OnValidate()
		{
			this.IsComponentNull(_mainCamera);
			this.IsComponentNull(_cameraInputs);
			this.IsComponentNull(_menuInputs);
			this.IsComponentNull(_interaction);
			this.IsComponentNull(_playerInput);
			this.IsComponentNull(_cornerMenu);
			this.IsComponentNull(_spawner);
			this.IsComponentNull(_previewsLayout);
			this.IsComponentNull(_prismChooser);
			this.IsComponentNull(_clusterChooser);
			this.IsComponentNull(_previewsLayout);
			this.IsComponentNull(_connectionsLayout);
			this.IsComponentNull(_cancelMenu);
		}

		internal void SubscribeOnPrism(Prism prism) //todo think about right place
		{
			prism.OnAllySelected += _previewsLayout.InitiatorSelected;
			prism.OnRivalSelected += _previewsLayout.RivalSelected;
		}
	}
}