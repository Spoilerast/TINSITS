using Monos.Systems;

namespace NotMonos.Processors
{
	internal class UnitDestroyProcessor
	{
		private UnitDestroyProcessor()
		{ }

		private static ConnectionsLayout _connections;

		internal static void Destroy(UnitId unitId)
		{
			_ = Extensions.UnityExtensions.TryFindObjectIfNull(ref _connections);

			if (!DataCenter.Units.IsNotClustered(unitId))
				ClusterProcessor.Declusterize(unitId);

			DataCenter.Connections.DropConnections(unitId);
			DataCenter.Grid.RemoveUnit(unitId);
			DataCenter.Properties.Remove(unitId);
			DataCenter.Units.DestroyUnit(unitId);
			_connections.MakeLinks();
		}
	}
}