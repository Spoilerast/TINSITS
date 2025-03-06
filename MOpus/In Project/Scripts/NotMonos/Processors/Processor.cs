using NotMonos.Databases;

namespace NotMonos.Processors
{
	internal abstract class Processor
	{
		protected Processor()
		{ }

		internal static ClustersDB Clusters => DataCenter.Clusters;

		internal static ConnectionsDB Connections => DataCenter.Connections;

		internal static GridDB Grid => DataCenter.Grid;

		internal static PropertiesDB Properties => DataCenter.Properties;

		internal static UnitsDB Units => DataCenter.Units;
	}
}