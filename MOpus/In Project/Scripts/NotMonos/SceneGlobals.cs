using System;
using NotMonos.SaveLoad;

namespace NotMonos
{
	internal sealed class SceneGlobals
	{
		private static readonly Lazy<SceneGlobals> _instance = new(() => new SceneGlobals());
		private static SceneGlobals Instance
			=> _instance.Value; //todo is using this pattern reasonable?

		private readonly SaveLoadSystem _saveLoadSystem = new();
		private TeamId _playerTeam = TeamId.GetTeamId(1);
		private SceneState _state = 0;

		internal static SceneState CurrentState
			=> Instance._state;

		internal static TeamId CurrentTeam
			=> Instance._playerTeam;

		internal static void ChangePlayerTeam(TeamId playerTeam)
			=> Instance._playerTeam = playerTeam;

		internal static void LoadCurrentScene()
		{
			Instance._saveLoadSystem.LoadSavefile();
			SetState(SceneState.Default);//todo make states less dependent
		}

		internal static void SaveCurrentScene()
			=> Instance._saveLoadSystem.SaveSavefile();

		internal static void SetState(SceneState state)
			=> Instance._state = state;
	}
}