using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Inputs;
using Monos.Scene;
using Monos.Systems;
using NotMonos.Backstage;
using NotMonos.Databases;
using NotMonos.SaveLoad;
using UnityEngine;
using static Extensions.UnityExtensions;

namespace NotMonos.Processors
{
internal abstract class LoadProcessor : Processor
{
	private static SceneObjectsSpawner _sceneSpawner;

	internal static void AddPowerSourceOnGrid(byte team, GridPoint point)
	{
		var teamId = TeamId.GetTeamId(team);
		Grid.AddPowerSource(teamId, point);
	}

	internal static UnitId AddPrism(Prism prism)
	{
		(byte team, PrismType type, float i, float ca, float cha, float re) = prism;

		GridPoint position = prism.PositionAsPoint;
		var unitId = UnitId.GetNewID();
		AddToUnitsAndProperties(unitId, prism, team, type, i, ca, cha, re);
		Grid.AddOnGrid(unitId, position);

		return unitId;
	}

	internal static void EmbodySaveData(SaveData sd)
	{
		if (!TryFindObjectIfNull(ref _sceneSpawner)
			|| !TryFindObject(out ConnectionsLayout connectionsLayout))
			return;

		SceneGlobals.ClearScene();
		ClearAllGameData();
		SceneObjectsSpawner.DestroyObjects();
		connectionsLayout.DestroyLinks();
		//todo make load validation

		//sd.LogThis();
		EmbodyPowerSources(sd.PowerSources);
		EmbodyUnits(sd.Units);
		LoadClusters(sd.Clusters);

		connectionsLayout.MakeLinks();
		MoveCameraTo(sd.CameraPosition());
		SceneGlobals.SetState(SceneState.Default);
	}

	private static void AddToUnitsAndProperties(UnitId unitId,
												Prism prism,
												byte teamId,
												PrismType type,
												float integrity,
												float capacity,
												float charge,
												float resistance)
	{
		Units.AddUnit(unitId, prism);
		PrismProperties properties = new(teamId, type, integrity, capacity, charge, resistance);
		Properties.Add(unitId, properties);
		prism.OnAllySelected += Selection.AllySelected;
		prism.OnRivalSelected += Selection.RivalSelected;

		if (SceneGlobals.InEditor)
			prism.name = unitId.ToString();
	}

	private static void EmbodyPowerSources(IEnumerable<PowerSourceData> powerSources)
	{
		PeekLogger.LogName();
		foreach (PowerSourceData powerSource in powerSources){
			(float x, float z, byte team) = powerSource.GetData;
			_sceneSpawner.PlacePowerSource(x, z, team);
		}
	}

	private static void EmbodyUnits(IEnumerable<UnitData> units)
	{
		PeekLogger.LogName();
		foreach (UnitData unit in units){
			(ushort uid, byte tid, byte type, float coordX, float coordZ, float integrity, float capacity, float charge, float resistance)
				= unit;

			UnitId unitId = UnitId.LoadID(uid);
			Prism prism = _sceneSpawner.LoadPrismAt(TeamId.GetTeamId(tid), coordX, coordZ);
			Grid.AddOnGrid(unitId, coordX, coordZ);
			AddToUnitsAndProperties(unitId, prism, tid, (PrismType)type, integrity, capacity, charge, resistance);
			prism.SetId(unitId);
			Connections.MakeConnections(unitId, ClusterStatus.NotClustered);
		}
	}

	private static void LoadCluster(in ClusterData clusterData)
	{
		(byte type, float x, float z, ushort[] ids) = clusterData.GetData;
		int len = ids.Length;
		if (len is not (ClusterInfo.ClusterUnitsCount or ClusterInfo.SuperClusterUnitsCount))
			throw new ArgumentException("Not valid cluster", nameof(clusterData));

		GridPoint axisPoint = new(x, z);
		ClusterType clusterType = len is ClusterInfo.SuperClusterUnitsCount
			? ClusterType._7
			: ClusterType._3;
		var prismType = (PrismType)type;

		UnitId[] clusterUnitIds
			= (from id in ids
			   select UnitId.Find(id))
			.ToArray();
		IEnumerable<GridPoint> positions = clusterUnitIds
			.Select(id => Grid.GetPoint(id));

		ClusterInfo info = new(axisPoint, clusterType, prismType, positions);
		ClusterProcessor.MakeCluster(info, clusterUnitIds);
	}

	private static void LoadClusters(IEnumerable<ClusterData> clusters)
	{
		foreach (ClusterData cluster in clusters)
			LoadCluster(cluster);
	}

	private static void MoveCameraTo(Vector3 vector3)
	{
		if (!TryFindObject(out CameraInput_Async camera))
			return;

		camera.SetCameraPosition(vector3);
	}
}
}