using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;

namespace NotMonos.PreviewsLayout
{
	internal sealed class DeclusterSystem
	{
		private readonly ClustersDB _clusters = DataCenter.Clusters;
		private readonly Dictionary<PreviewData, UnitId> _declusters = new(9);
		private readonly SuperClusterSystem _superClusterSystem;
		private readonly UnitsDB _units = DataCenter.Units;

		internal DeclusterSystem(SuperClusterSystem superClusterSystem)
			=> _superClusterSystem = superClusterSystem;

		internal bool IsHaveSelfDecluster { get; private set; }

		internal void ClearAll()
			=> _declusters.Clear();

		internal IEnumerable<(PreviewData, UnitId)> Declusters()
		{
			foreach (var item in _declusters)
				yield return (item.Key, item.Value);
		}

		internal bool TryFindDeclusters(UnitId initiatorId)
		{
			IsHaveSelfDecluster = !_units.IsNotClustered(initiatorId);

			if (!_superClusterSystem.TryGetDeclusters(
				out IEnumerable<(GridPoint moveDirection, UnitId[] unitsToDecluster)> declusters)
				&& !IsHaveSelfDecluster)
				return false;

			if (IsHaveSelfDecluster)
			{
				var clusterId = _units.GetClusterId(initiatorId);
				MakeDeclusterPreview(GridPoint.NAP, clusterId, initiatorId);
			}

			foreach (var (moveDirection, unitsToDecluster) in declusters)
				MakeUniqueDeclusterPreviewsOnDirection(moveDirection, unitsToDecluster);

			return _declusters.Count != 0;
		}

		internal bool TryGetDeclusterUnitIdsOnDirection(GridPoint moveDirection, out IEnumerable<UnitId> declustersOnDirection)
		{
			declustersOnDirection =
				from pair in _declusters
				where pair.Key.PrismFuturePosition == moveDirection
				select pair.Value;
			return declustersOnDirection.Any();
		}

		private void MakeDeclusterPreview(GridPoint moveDirection, ClusterId clusterId, UnitId unitId)
		{
			ClusterInfo clusterInfo = _clusters.GetClusterInfo(clusterId);
			ClusterType clusterType = clusterInfo.ClusterType;
			bool isSuperCluster = clusterType == ClusterType._7;

			GridPoint previewAxis = isSuperCluster
				? clusterInfo.AxisPoint
				: clusterInfo.Positions[0];
			Prev_Cluster_Side previewSide = isSuperCluster
				? default
				: Constants.GetPreviewSide(clusterInfo.Positions[0], clusterInfo.AxisPoint);

			MakeDeclusterPreview(clusterType, previewAxis, moveDirection, previewSide, unitId);
		}

		private void MakeDeclusterPreview(ClusterType clusterType,
									GridPoint previewAxis,
									GridPoint moveDirection,
									Prev_Cluster_Side previewSide,
									UnitId unitId)
			=> _declusters.Add(
					new(PreviewType.Destroy,
					clusterType,
					previewAxis,
					moveDirection,
					previewSide),
				unitId);

		private void MakeUniqueDeclusterPreviewsOnDirection(GridPoint moveDirection, UnitId[] unitsToDecluster)
		{
			if (unitsToDecluster.Length == 0)
				return;

			PeekLogger.Log($"direction {moveDirection}", unitsToDecluster);
			HashSet<ClusterId> set = new();
			ClusterId clusterId;
			foreach (UnitId unitId in unitsToDecluster)
			{
				clusterId = _units.GetClusterId(unitId);
				if (set.Contains(clusterId))
					continue; //only one preview for same cluster

				_ = set.Add(clusterId);
				MakeDeclusterPreview(moveDirection, clusterId, unitId);
			}
		}
	}
}