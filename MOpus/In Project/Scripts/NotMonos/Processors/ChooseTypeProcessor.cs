using System.Linq;
using Extensions;

namespace NotMonos.Processors
{
internal abstract class ChooseTypeProcessor : Processor
{
	internal static PrismType GetPrismType(UnitId id)
		=> Properties.GetPrismType(id);

	internal static bool IsNotClustered(UnitId id)
		=> !Units.TryGetClusterId(id, out _);

	internal static void SetPrismType(UnitId id, PrismType type) { Properties.SetPrismType(id, type); }

	internal static void SetPrismTypeForCluster(ClusterId clusterId, PrismType type) //todo unfinished
	{
		PeekLogger.LogName();
		UnitId[] clusterUnits = Units.GetUnitsInCluster(clusterId).ToArray();
		foreach (UnitId id in clusterUnits)
			_ = Properties.SetPrismType(id, type);

		ClusterStatus status = Units.IsClustered(clusterUnits[0]) //todo just get status!
			? ClusterStatus.Clustered
			: ClusterStatus.SuperClustered;
		Connections.Debug_PrintNets("before conn");
		Connections.RemakeConnections(clusterUnits, status);
		Connections.Debug_PrintNets("after conn");
	}
}
}