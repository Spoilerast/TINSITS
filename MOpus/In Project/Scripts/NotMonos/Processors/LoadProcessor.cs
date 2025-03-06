using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;
using NotMonos.SaveLoad;
using static Extensions.UnityExtensions;

namespace NotMonos.Processors
{
	internal sealed class LoadProcessor : Processor
	{
		private Monos.Systems.SceneObjectsSpawner _sceneSpawner;

		internal void EmbodySaveData(SaveData sd)
		{
			if (!TryFindObjectIfNull(ref _sceneSpawner)
				|| !TryFindObject(out Monos.Systems.ConnectionsLayout connectionsLayout)
				|| !TryFindObject(out Monos.Backstage.Previews.PreviewsLayout previewsLayout))
				return;

			previewsLayout.ClearAll();
			ClearAllGameData();
			Monos.Systems.SceneObjectsSpawner.DestroyObjects();
			connectionsLayout.DestroyLinks();
			//todo make load validation

			EmbodyPowerSources(sd.PowerSources);
			EmbodyUnits(sd.Units);
			LoadClusters(sd.Clusters);

			connectionsLayout.MakeLinks();
			MoveCameraTo(sd.CameraPosition());
		}

		private static void ClearAllGameData()
			=> DataCenter.ClearAllGameData();

		private void EmbodyPowerSources(IEnumerable<PowerSourceData> powerSources)
		{
			PeekLogger.LogName();
			foreach (var powerSource in powerSources)
			{
				var (x, z, team) = powerSource.GetData;
				_sceneSpawner.PlacePowerSource(x, z, team);
			}
		}

		private void EmbodyUnits(IEnumerable<UnitData> units)
		{
			PeekLogger.LogName();
			foreach (UnitData unit in units)
			{
				var (uid, tid, type,
				coord_x, coord_z,
				integrity, capacity, charge, resistance) = unit;

				var unitId = UnitId.LoadID(uid);
				PrismProperties properties = new(tid, type, integrity, capacity, charge, resistance);

				PeekLogger.LogItemsVarious(unitId, properties);

				Properties.Add(unitId, properties);
				Grid.AddOnGrid(unitId, coord_x, coord_z);

				Monos.Scene.Prism prism = _sceneSpawner.LoadPrismAt(TeamId.GetTeamId(tid), coord_x, coord_z);
				prism.SetId(unitId);
				Units.AddUnit(unitId, prism);
				Connections.MakeConnections(unitId, ClusterStatus.NotClustered);
			}
		}

		private static void LoadCluster(in ClusterData clusterData)
		{
			var (type, x, z, ids) = clusterData.GetData;
			int len = ids.Length;
			if (len is not ClusterInfo.ClusterUnitsCount and not ClusterInfo.SuperClusterUnitsCount)
				throw new ArgumentException("Not valid cluster", "clusterData");

			GridPoint axisPoint = new(x, z);
			ClusterType clusterType = len is ClusterInfo.SuperClusterUnitsCount
				? ClusterType._7
				: ClusterType._3;
			PrismType prismType = (PrismType)type;

			IEnumerable<UnitId> clusterUnitIds
				= from id in ids
				  select UnitId.Find(id);
			GridPoint[] positions = clusterUnitIds
				.Select(id => Grid.GetPoint(id))
				.ToArray();

			ClusterInfo info = new(axisPoint, clusterType, prismType, positions);
			ClusterProcessor.MakeCluster(info, clusterUnitIds);
		}

		private static void LoadClusters(IEnumerable<ClusterData> clusters)
		{
			foreach (ClusterData cluster in clusters)
				LoadCluster(cluster);
		}

		private static void MoveCameraTo(UnityEngine.Vector3 vector3)
		{
			if (!TryFindObject(out Inputs.CameraInput_Async camera))
				return;

			camera.SetCameraPosition(vector3);
		}
	}
}