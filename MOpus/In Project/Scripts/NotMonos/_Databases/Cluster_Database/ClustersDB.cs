using System.Collections.Generic;

namespace NotMonos.Databases
{
	internal sealed class ClustersDB
	{
		private readonly Dictionary<ClusterId, ClusterInfo> _clusters = new();

		internal IEnumerable<ClusterInfo> AllClusters
			=> _clusters.Values;

		internal int Count
			=> _clusters.Count;

		internal void AddCluster(ClusterInfo info, out ClusterId clusterId)
		{
			clusterId = ClusterId.GetNewID();
			_clusters.Add(clusterId, info);
		}

		internal void Clear()
			=> _clusters.Clear();

		internal GridPoint ClusterAxis(ClusterId clusterId)
			=> _clusters[clusterId].AxisPoint;

		internal ClusterInfo GetClusterInfo(ClusterId clusterId)
			=> _clusters[clusterId];

		internal ClusterType GetClusterType(ClusterId clusterId)
			=> _clusters[clusterId].ClusterType;

		internal void Remove(ClusterId clusterId)
		{
			_ = _clusters.Remove(clusterId);
			clusterId.RemoveThisId();
		}
	}
}