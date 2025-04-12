using System;
using NotMonos.Databases;

namespace NotMonos.Previews
{
internal readonly struct PreviewData : IEquatable<PreviewData>
{
	public PreviewData(PreviewType previewType,
					   ClusterType clusterType,
					   GridPoint previewAxis,
					   GridPoint prismFuturePosition,
					   HolderSide side,
					   ScrollStatus scrollStatus)
	{
		(PreviewType, ClusterType, PreviewAxis, PrismFuturePosition, Side, ScrollStatus)
			= (previewType, clusterType, previewAxis, prismFuturePosition, side, scrollStatus);
	}

	internal ClusterType ClusterType { get; }

	internal ScrollStatus ScrollStatus { get; }

	internal GridPoint PreviewAxis { get; }

	internal GridPoint PrismFuturePosition { get; }

	internal PreviewType PreviewType { get; }

	internal HolderSide Side { get; }

	public override string ToString()
		=> $"[scroll: {ScrollStatus} pr_axis: {PreviewAxis}, side: {Side}, cTp: {ClusterType}, pTp: {PreviewType}{(PreviewType is PreviewType.Destroy || PrismFuturePosition.IsNAP ? "" : $", future position: {PrismFuturePosition}")}]";

	public bool Equals(ClusterType clusterType, HolderSide side)
		=> ClusterType == clusterType
		   && Side == side;

	public bool Equals(PreviewData other)
		=> PreviewType == other.PreviewType
		   && ClusterType == other.ClusterType
		   && Side == other.Side
		   && PreviewAxis == other.PreviewAxis;

	public override bool Equals(object obj)
		=> obj is PreviewData other && Equals(other);

	public override int GetHashCode()
		=> HashCode.Combine((int)ClusterType, PreviewAxis, (int)PreviewType, (int)Side);
}
}