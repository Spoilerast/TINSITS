using System.Collections.Generic;
using Extensions;
using Monos.Scene;
using NotMonos;
using NotMonos.Databases;
using UnityEngine;

namespace Monos.Systems
{
	/*internal interface ISpawner : ISearchable //todo rework it
	{
		abstract void ClearNests();

		//abstract void PlacePowerSource(in float x, in float z, in byte team);

		abstract bool InSpawnMode { get; }

		abstract void ToggleSpawnMode();

		abstract void SpawnPrismAt(in Vector3 position);
	}*/

	internal class SceneObjectsSpawner : SceneSystem
	{
		[SerializeField] private Nest _nest;
		[SerializeField] private PowerSource _powerSource;
		[SerializeField] private Prism _unitT1;
		[SerializeField] private Prism _unitT2;
		private readonly Stack<Nest> _nests = new();
		public bool InSpawnMode { get; private set; } = false;

		internal static void DestroyObjects()
		{
			foreach (var item in FindObjectsByType<Prism>(sortMode: FindObjectsSortMode.None))
				item.DestroySceneObject();
			foreach (var item in FindObjectsByType<PowerSource>(sortMode: FindObjectsSortMode.None))
				item.DestroySceneObject();
		}

		internal Prism LoadPrismAt(TeamId teamId, in float x, in float z)
		{
			Prism unit = teamId.Equals(1)
				? _unitT1
				: _unitT2;
			return this.InstantiateXZ(unit, x, z);
		}

		internal void PlacePowerSource(in float x, in float z, in byte team)
		{
			PowerSource ps = this.InstantiateXZ(_powerSource, x, z);
			ps.gameObject.isStatic = true;
			ps.Initialize(team);
		}

		internal void ToggleSpawnMode()
		{
			InSpawnMode = !InSpawnMode; //todo strange logic

			if (InSpawnMode)
			{
				Debug.Log("Enter Spawn Mode");
				SceneGlobals.SetState(SceneState.SpawnMode);
				InstantiateNests();
				return;
			}
			Debug.Log("Exit Spawn Mode");
			SceneGlobals.SetState(SceneState.Default);
			ClearNests();
		}

		private static IEnumerable<GridPoint> GetDirectionsForAllSpawnPoints()
		{
			TeamId team = SceneGlobals.CurrentTeam;
			return NotMonos.Processors
				.SpawnPointsProcessor.FreeSpawnPointsForTeam(team);
		}

		private void ClearNests()
		{
			while (_nests.Count > 0)
				_nests.Pop().DestroySceneObject();
		}

		private void InstantiateNests()
		{
			foreach (var point in GetDirectionsForAllSpawnPoints())
				SpamnNestAt(point);
		}

		private void OnValidate()
		{
			_ = this.IsComponentNull(_nest);
			_ = this.IsComponentNull(_unitT1);
			_ = this.IsComponentNull(_unitT2);
			_ = this.IsComponentNull(_powerSource);
		}

		private void SpamnNestAt(GridPoint position)
		{
			if (!this.TryInstantiate(_nest, position, out Nest nest))
				return;
			nest.OnPickedSpawn += SpawnPrismAt;
			_nests.Push(nest);
		}

		private void SpawnPrismAt(GridPoint position)
		{
			Prism unit = SceneGlobals.CurrentTeam.Equals(1) //todo make it less hardcoded
				? _unitT1
				: _unitT2;
			if (!this.TryInstantiate(unit, position, out Prism prism))
				return;
			prism.Initialize();
			ClearNests();
			InstantiateNests();
		}
	}
}