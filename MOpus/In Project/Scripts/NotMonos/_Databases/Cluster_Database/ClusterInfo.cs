using Extensions;

namespace NotMonos.Databases
{
	internal sealed class ClusterInfo
	{
		internal const int ClusterUnitsCount = 3;	//todo maybe move to Constants
		internal const int SuperClusterUnitsCount = 7;

		public ClusterInfo(GridPoint axisPoint, ClusterType clusterType, PrismType prismType, GridPoint[] positions)
			: this(axisPoint, clusterType, positions)
			=> PrismType = prismType;

		internal ClusterInfo(GridPoint axisPoint, ClusterType clusterType, GridPoint[] positions)
			=> (AxisPoint, ClusterType, Positions) = (axisPoint, clusterType, positions);

		internal GridPoint AxisPoint { get; }
		internal ClusterType ClusterType { get; }
		internal GridPoint[] Positions { get; }
		internal PrismType PrismType { get; set; }

		public override string ToString()
			=> $"/axis: {AxisPoint}, cType: {ClusterType}, pType: {PrismType}, pos: {Positions.JoinItemsToString()}";
	}
}