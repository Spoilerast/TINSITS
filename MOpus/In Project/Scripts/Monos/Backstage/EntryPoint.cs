#if UNITY_EDITOR
#define IN_EDITOR
#endif

using Extensions;
using Inputs;
using Monos.Scene;
using NotMonos;
using NotMonos.Backstage;
using NotMonos.Databases;
using NotMonos.Previews;
using NotMonos.SaveLoad;
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
	[SerializeField] private PreviewsController _previewsController;
	[SerializeField] private PrismTypeChooserUI _prismChooser;
	[SerializeField] private ClusterTypeChooserUI _clusterChooser;
	[SerializeField] private PropertiesViewerUI _propertiesViewer;
	[SerializeField] private ConnectionsLayout _connectionsLayout;
	[SerializeField] private CancelMenuUI _cancelMenu;
	[SerializeField] private AvailablesViewerUI _availablesViewerUI;

	private void Awake()
	{
#if IN_EDITOR
		SceneGlobals.InEditor = true;
#endif
		PeekLogger.Log($"Save Path is {SaveLoadSystem.DefaultSavePath}");

		InputActions inputActions = InputsWrapper.Actions;
		inputActions.UI.Disable();
		_cameraInputs.Initialize(_mainCamera, _playerInput);
		_menuInputs.Initialize(_cornerMenu, _previewsController);
		_interaction.Initialize(_previewsController);
		_cornerMenu.Initialize(_spawner);

		_prismChooser.Initialize(_previewsController);
		_clusterChooser.Initialize(_previewsController);
		_propertiesViewer.Initialize();
		_cancelMenu.Initialize(_previewsController);
		_availablesViewerUI.Initialize(_previewsController);

		_connectionsLayout.SubscribeOnMove(_previewsController);

		InitializeSceneObjects();
		SceneGlobals.SetState(SceneState.Default); //todo: remove?

		ConnectUnitsOnScene();
		//Cursor.SetCursor(_texture2D, new Vector2(-.9f,.9f), CursorMode.Auto);
		PeekLogger.Log("███ End of EntryPoint. Start of Play loop ███");
		/*var test = (2, 9f, 'u', "suck", 8L);
		(UnitId, TeamId, ClusterInfo, PreviewData) test2 = (UnitId.Zero, TeamId.GetTeamId(1), default(ClusterInfo), default(PreviewData));
		test.LogThis();
		var li = new List<(int, float, char, string, long)> { test, test, test };
		var lis=new List<(UnitId, TeamId, ClusterInfo, PreviewData)>{test2,test2,test2};
		PeekLogger.LogItems(lis);
		PeekLogger.LogItems(lis.Skip(1));
		PeekLogger.LogItems(li);
		PeekLogger.LogItems(li.Skip(1));*/
		//SceneGlobals.LoadCurrentScene();
		//PeekLogger.ClearLog();
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
		this.IsComponentNull(_previewsController);
		this.IsComponentNull(_prismChooser);
		this.IsComponentNull(_clusterChooser);
		this.IsComponentNull(_previewsController);
		this.IsComponentNull(_connectionsLayout);
		this.IsComponentNull(_cancelMenu);
		this.IsComponentNull(_availablesViewerUI);
	}

	private void ConnectUnitsOnScene()
	{
		ConnectionsDB connections = DataCenter.Connections;
		foreach (UnitId unitId in UnitId.GetAllIds)
			connections.MakeConnections(unitId, ClusterStatus.NotClustered);

		_connectionsLayout.MakeLinks();
	}

	private static void InitializeSceneObjects()
	{
		foreach (PowerSource item in FindObjectsByType<PowerSource>(FindObjectsSortMode.None))
			item.Initialize();

		foreach (Prism item in FindObjectsByType<Prism>(FindObjectsSortMode.None))
			item.Initialize();
	}
}
}