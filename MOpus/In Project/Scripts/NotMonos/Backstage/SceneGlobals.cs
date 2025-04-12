using System;
using Extensions;
using NotMonos.SaveLoad;

namespace NotMonos.Backstage
{
internal sealed class SceneGlobals
{
	private static readonly Lazy<SceneGlobals> _instance = new(() => new());

	//private readonly SaveLoadSystem _saveLoadSystem = new();
	private TeamId _playerTeam = TeamId.GetTeamId(1);
	private SceneState _state = 0;

	internal static event Action OnClearScene;
	private static SceneGlobals Instance => _instance.Value;

	internal static SceneState CurrentState => Instance._state;

	internal static TeamId CurrentTeam => Instance._playerTeam;
	public static bool InEditor { get; set; }

	public static void ClearScene()
		=> OnClearScene.SafeInvoke();

	internal static void ChangePlayerTeam(TeamId playerTeam) { Instance._playerTeam = playerTeam; }

	internal static void LoadCurrentScene()
	{
		PeekLogger.LogName();
		SaveLoadSystem.LoadSavefile();
	}

	internal static void SaveCurrentScene() { SaveLoadSystem.SaveSavefile(); }

	internal static void SetState(SceneState state)
	{
		Instance._state = state;
		PeekLogger.LogWarning(state);
	}
}
}