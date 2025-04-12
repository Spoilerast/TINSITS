using System;
using NotMonos.Backstage;

namespace NotMonos.Databases
{
internal abstract class DataCenter
{
	private static readonly Lazy<UnitsDB> _units = new(() => new());
	private static readonly Lazy<GridDB> _grid = new(() => new());
	private static readonly Lazy<PropertiesDB> _properties = new(() => new());
	private static readonly Lazy<ClustersDB> _clusters = new(() => new());
	private static readonly Lazy<ConnectionsDB> _connections = new(() => new());
	private static readonly Lazy<SelectionController> _selection = new(() => new());

	internal static UnitsDB Units => _units.Value;
	internal static GridDB Grid => _grid.Value;
	internal static PropertiesDB Properties => _properties.Value;
	internal static ClustersDB Clusters => _clusters.Value;
	internal static ConnectionsDB Connections => _connections.Value;
	internal static SelectionController Selection => _selection.Value;

	internal static bool SelectedIsNotClustered() //maybe move to some Processor
		=> Units.IsNotClustered(Selection.SelectedUnit);

	protected static void ClearAllGameData()
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