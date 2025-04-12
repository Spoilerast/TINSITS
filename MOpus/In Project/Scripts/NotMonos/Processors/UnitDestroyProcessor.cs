using Extensions;
using Monos.Systems;

namespace NotMonos.Processors
{
internal abstract class UnitDestroyProcessor : Processor
{
	private static ConnectionsLayout _connections;

	internal static void Destroy(UnitId unitId)
	{
		_ = UnityExtensions.TryFindObjectIfNull(ref _connections);

		if (!Units.IsNotClustered(unitId))
			ClusterProcessor.Declusterize(unitId);

		Connections.DropConnections(unitId);
		Grid.RemoveUnit(unitId);
		Properties.Remove(unitId);
		Units.DestroyUnit(unitId);
		_connections.MakeLinks();
	}
}
}