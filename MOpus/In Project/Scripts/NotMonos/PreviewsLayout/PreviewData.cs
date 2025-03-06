using NotMonos.Databases;

namespace NotMonos.PreviewsLayout
{
	internal readonly struct PreviewData
	{
		public PreviewData(PreviewType previewType, ClusterType clusterType,
							GridPoint previewAxis, GridPoint prismFuturePosition,
							Prev_Cluster_Side side)
		=> (PreviewType, ClusterType, PreviewAxis, PrismFuturePosition, Side)
			= (previewType, clusterType, previewAxis, prismFuturePosition, side);

		internal readonly ClusterType ClusterType { get; }

		internal readonly GridPoint PreviewAxis { get; }

		internal readonly GridPoint PrismFuturePosition { get; }

		internal readonly PreviewType PreviewType { get; }

		internal readonly Prev_Cluster_Side Side { get; }

		public override string ToString() => $"[pr_axis: {PreviewAxis}, side: {Side}, cTp: {ClusterType}, pTp: {PreviewType}{(PreviewType is PreviewType.Destroy ? "" : $", future position: {PrismFuturePosition}")}]";
	}
}