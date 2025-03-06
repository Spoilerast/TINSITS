using System.Collections.Generic;
using System.Linq;
using NotMonos.Databases;
using NotMonos.SaveLoad;

namespace NotMonos.Processors
{
	internal sealed class SaveProcessor : Processor
	{
		internal SaveData CreateSaveData()
		{
			_ = Extensions.UnityExtensions.TryFindObject(out Inputs.CameraInput_Async camera);

			var cameraData = new CameraData(camera.transform.position);

			IEnumerable<PowerSourceData> powerSources
				= from src in Grid.GetAllPowerSources
				  let data = new PowerSourceData(src.id, src.point)
				  select data;

			//todo scene is valid only if 1 or more power source(s) on scene

			IEnumerable<ClusterData> clusters = ClustersToDatas();

			IEnumerable<UnitData> units = UnitsToDatas();

			SaveData save = new(cameraData,
				powerSources,
				clusters,
				units);
			return save;
		}

		private static UnitData InfoToUnitDataConvertion(UnitId id)
		{
			GridPoint position = Grid.GetPoint(id);

			(TeamId teamId,
			PrismType type,
			float integrity,
			float capacity,
			float charge,
			float resistance) = Properties.GetProperties(id);

			return new UnitData(
				unitId: id,
				teamId: teamId.Value,
				type: (byte)type,
				coord_x: position.X,
				coord_z: position.Z,
				integrity: integrity,
				capacity: capacity,
				charge: charge,
				resistance: resistance
				);
		}

		private IEnumerable<ClusterData> ClustersToDatas()
		{
			if (Clusters.Count == 0)
				yield break;

			foreach (var item in Clusters.AllClusters)
				yield return InfoToClusterDataConvertion(item);
		}

		private ClusterData InfoToClusterDataConvertion(ClusterInfo clusterInfo)
		{
			float axis_x = clusterInfo.AxisPoint.X,
			axis_z = clusterInfo.AxisPoint.Z;
			ushort[] unit_ids
				= (from p in clusterInfo.Positions
				   let check = new
				   {
					   HaveUnitHere = Grid.TryGetUnitId(p, out var unitId),
					   Id = (ushort)unitId
				   }
				   where check.HaveUnitHere
				   select check.Id)
					.ToArray();
			byte type = (byte)clusterInfo.PrismType;
			return new(type, axis_x, axis_z, unit_ids);
		}

		private static IEnumerable<UnitData> UnitsToDatas()
		{
			if (Units.Count == 0)
				yield break;

			foreach (var id in Units.AllUnitIds)
				yield return InfoToUnitDataConvertion(id);
		}
	}
}