using System;
using NotMonos.Databases;

namespace NotMonos
{
	internal static class DataCenter
	{
		private static readonly Lazy<UnitsDB> _units = new(() => new UnitsDB());
		private static readonly Lazy<GridDB> _grid = new(() => new GridDB());
		private static readonly Lazy<PropertiesDB> _properties = new(() => new PropertiesDB());
		private static readonly Lazy<ClustersDB> _clusters = new(() => new ClustersDB());
		private static readonly Lazy<ConnectionsDB> _connections = new(() => new ConnectionsDB());

		internal static UnitsDB Units
			=> _units.Value;

		internal static GridDB Grid
			=> _grid.Value;

		internal static PropertiesDB Properties
			=> _properties.Value;

		internal static ClustersDB Clusters
			=> _clusters.Value;

		internal static ConnectionsDB Connections
			=> _connections.Value;

		internal static void ClearAllGameData()
		{
			UnitId.ClearIds();
			ClusterId.ClearIds();
			Units.Clear();
			Grid.Clear();
			Properties.Clear();
			Clusters.Clear();
			Connections.Clear();
		}
	}
}