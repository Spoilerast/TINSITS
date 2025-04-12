using System.Collections.Generic;
using System.Linq;
using Extensions;
using Inputs;
using NotMonos.Databases;
using NotMonos.SaveLoad;
using UnityEngine;

namespace NotMonos.Processors
{
internal abstract class SaveProcessor : Processor
{
	internal static SaveData CreateSaveData()
	{
		_ = UnityExtensions.TryFindObject(out CameraInput_Async camera);
		Vector3 cameraPos = camera.Position;
		PeekLogger.Log($"camera pos: {cameraPos}");
		var cameraData = new CameraData(cameraPos);

		IEnumerable<PowerSourceData> powerSources
			=
			from src in Grid.GetAllPowerSources
			let data = new PowerSourceData(src.id, src.point)
			select data;

		//todo scene is valid only if 1 or more power source(s) on scene

		IEnumerable<ClusterData> clusters = ClustersToData();

		IEnumerable<UnitData> units = UnitsToData();

		SaveData save = new(cameraData,
							powerSources,
							clusters,
							units);
		return save;
	}

	private static IEnumerable<ClusterData> ClustersToData()
	{
		if (Clusters.Count == 0)
			yield break;

		foreach (ClusterInfo item in Clusters.AllClusters)
			yield return InfoToClusterDataConversion(item);
	}

	private static ClusterData InfoToClusterDataConversion(ClusterInfo clusterInfo)
	{
		float axisX = clusterInfo.AxisPoint.X,
			  axisZ = clusterInfo.AxisPoint.Z;
		ushort[] unitIds
			= (
				from p in clusterInfo.Positions
				let check = new {
					HaveUnitHere = Grid.TryGetUnitId(p, out UnitId unitId),
					Id = (ushort)unitId
				}
				where check.HaveUnitHere
				select check.Id)
			.ToArray();
		var type = (byte)clusterInfo.PrismType;
		return new(type, axisX, axisZ, unitIds);
	}

	private static UnitData InfoToUnitDataConversion(UnitId id)
	{
		GridPoint position = Grid.GetPoint(id);

		(TeamId teamId,
		 PrismType type,
		 float integrity,
		 float capacity,
		 float charge,
		 float resistance) = Properties.GetProperties(id);

		return new(id,
				   teamId.Value,
				   (byte)type,
				   position.X,
				   position.Z,
				   integrity,
				   capacity,
				   charge,
				   resistance);
	}

	private static IEnumerable<UnitData> UnitsToData()
	{
		if (Units.Count == 0)
			yield break;

		foreach (UnitId id in Units.AllUnitIds)
			yield return InfoToUnitDataConversion(id);
	}
}
}