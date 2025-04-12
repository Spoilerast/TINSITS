using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;

namespace NotMonos.Processors
{
internal abstract class ClusterProcessor : Processor
{
	private ClusterProcessor() {}

	internal static void ConfirmCluster(ClusterInfo info)
	{
		PeekLogger.LogNameCollection(info.Positions);
		UnitId[] unitIds = Grid.GetIdsOnPoints(info.Positions).ToArray();
		DeclusterizeRange(unitIds);
		Properties.SetPrismsType(unitIds, info.PrismType);
		MakeCluster(info, unitIds);
	}

	internal static void Declusterize(UnitId unitId)
	{
		if (!Units.TryGetClusterId(unitId, out ClusterId clusterId))
			return;

		Declusterize(clusterId);
	}

	internal static void Declusterize(ClusterId clusterId)
	{
		UnitId[] disconnected = ClearCluster(clusterId);
		Clusters.Remove(clusterId);
		Connections.DropClusterConnections(disconnected);
	}

	internal static void DeclusterizeRange(IEnumerable<ClusterId> declustersIds)
	{
		foreach (ClusterId clusterId in declustersIds)
			Declusterize(clusterId);
	}

	internal static void MakeCluster(ClusterInfo info, IEnumerable<UnitId> clusterUnitIds)
	{
		Clusters.AddCluster(info, out ClusterId clusterId);
		ClusterStatus status = info.ClusterType == ClusterType._3
			? ClusterStatus.Clustered
			: ClusterStatus.SuperClustered;

		UnitId[] unitIds = clusterUnitIds.ToArray();
		foreach (UnitId unitId in unitIds){
			Connections.DropConnections(unitId);
			/*if (status is ClusterStatus.Clustered)
				Units.ClusterizePrism(unitId, info.AxisPoint);
			*/
			Units.ClusterizeUnit(unitId, clusterId, status);
		}

		Neighborize(info, unitIds); //todo maybe to connections processor
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

	private static void DeclusterizeRange(IEnumerable<UnitId> declustersIds)
	{
		foreach (UnitId unitId in declustersIds)
			Declusterize(unitId);
	}

	private static void Neighborize(in ClusterInfo info, IEnumerable<UnitId> clusterUnitIds)
	{
		GridPoint axis = info.AxisPoint;
		if (info.ClusterType is ClusterType._3){
			Grid.AddPlaceholder(axis);
			Connections.NeighborizeCluster(clusterUnitIds);
			return;
		}

		Grid.AddPlaceholders(axis);
		Connections.NeighborizeSuperCluster(clusterUnitIds);
	}
}
}