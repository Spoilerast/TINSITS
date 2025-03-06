using System.Collections.Generic;
using Extensions;
using NotMonos.Databases;

namespace NotMonos.Processors
{
	internal sealed class ClusterProcessor : Processor
	{
		private ClusterProcessor()
		{ }

		internal static void ConfirmCluster(ClusterInfo info)
		{
			PeekLogger.LogNameCollection(info.Positions);
			IEnumerable<UnitId> unitIds = Grid.GetIdsOnPoints(info.Positions);
			Properties.SetPrismsType(unitIds, info.PrismType);
			MakeCluster(info, unitIds);
		}

		internal static void Declusterize(UnitId unitId)
		{
			if (!Units.TryGetClusterId(unitId, out var clusterId))
				return;

			UnitId[] disconnected = ClearCluster(clusterId);
			Clusters.Remove(clusterId);
			Connections.DropClusterConnections(disconnected);
		}

		internal static void DeclusterizeRange(IEnumerable<UnitId> declustersIds)
		{
			foreach (var unitId in declustersIds)
				Declusterize(unitId);
		}

		internal static void MakeCluster(ClusterInfo info, IEnumerable<UnitId> clusterUnitIds)
		{
			Clusters.AddCluster(info, out var clusterId);
			ClusterStatus status = info.ClusterType == ClusterType._3
				? ClusterStatus.Clustered
				: ClusterStatus.SuperClustered;

			foreach (UnitId unitId in clusterUnitIds)
			{
				Connections.DropConnections(unitId);
				/*if (status is ClusterStatus.Clustered)
					Units.ClusterizePrism(unitId, info.AxisPoint);
				*/
				Units.ClusterizeUnit(unitId, clusterId, status);
			}
			Neighborize(info, clusterUnitIds);//todo maybe to connections processor
		}

		private static UnitId[] ClearCluster(ClusterId clusterId)
		{
			ClusterInfo cluster = Clusters.GetClusterInfo(clusterId);
			if (cluster.ClusterType is ClusterType._3)
				Grid.RemovePlaceholder(cluster.AxisPoint);
			else
				Grid.RemovePlaceholders(cluster.AxisPoint);

			return Units.RemoveAllUnitsFromCluster(clusterId);
		}

		private static void Neighborize(in ClusterInfo info, IEnumerable<UnitId> clusterUnitIds)
		{
			GridPoint axis = info.AxisPoint;
			if (info.ClusterType is ClusterType._3)
			{
				Grid.AddPlaceholder(axis);
				Connections.NeighborizeCluster(clusterUnitIds);
				return;
			}
			Grid.AddPlaceholders(axis);
			Connections.NeighborizeSuperCluster(clusterUnitIds);
		}
	}
}