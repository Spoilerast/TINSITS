using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;
using static NotMonos.Databases.Constants;

namespace NotMonos.PreviewsLayout
{
	internal enum PreviewsCollectionType
	{
		MainPreviews, SubPreviews
	}

	internal sealed class PreviewsSystem
	{
		private readonly Dictionary<PreviewId, ClusterInfo> _clusterInfos = new(33);
		private readonly Dictionary<PreviewId, UnitId> _declusters = new(9);
		private readonly Dictionary<PreviewId, PreviewData> _previews = new(33);
		private readonly Dictionary<PreviewId, PreviewData> _subPreviews = new(20);

		private readonly SuperClusterSystem _superClusterSystem;
		private readonly ClusterSystem _clusterSystem;
		private readonly DeclusterSystem _declusterSystem;

		private GridPoint[] _availableMoves;
		private UnitId _initiatorId;

		public PreviewsSystem()
		{
			_superClusterSystem = new();
			_clusterSystem = new();
			_declusterSystem = new(_superClusterSystem);
		}

		internal IEnumerable<GridPoint> GetAvailableMoves
			=> _availableMoves;

		internal IEnumerable<UnitId> GetDeclustersIds
			=> _declusters.Values;

		internal bool HaveDeclusters
			=> _declusters.Count > 0;

		internal bool HaveSelfDecluster
			=> _declusterSystem.IsHaveSelfDecluster;

		internal bool HaveSubPreviews
			=> _subPreviews.Count > 0;

		internal void ClearAll()
		{
			PreviewId.Clear();
			_clusterInfos.Clear();
			_declusters.Clear();
			_previews.Clear();
			_subPreviews.Clear();

			_clusterSystem.ClearAll();
			_superClusterSystem.ClearAll();
			_declusterSystem.ClearAll();
		}

		internal ClusterInfo GetClusterInfo(PreviewId previewId)
			=> _clusterInfos[previewId];

		internal GridPoint GetNewPosition(PreviewId previewId)
			=> _previews[previewId].PrismFuturePosition;

		internal IEnumerable<(PreviewId, PreviewData)> GetPreviews(PreviewsCollectionType kind)
			=> kind is PreviewsCollectionType.MainPreviews
			? GetMainPreviews()
			: GetSubPreviews();

		internal GridPoint GetSubPreviewAxis(PreviewId previewId)
			=> _subPreviews[previewId].PreviewAxis;

		internal PreviewsState PreparePreviews(UnitId initiatorId)
		{
			ClearAll();
			_initiatorId = initiatorId;

			if (!HaveAvailableMoves())
				return PreviewsState.NoMovesAvailable; //questionable, cuz SCl WorstCase

			PeekLogger.LogTab("FindClusters");
			FindClusterPreviews();
			PeekLogger.LogTab("FindSuperClusters");
			FindSuperClusterPreviews();
			PeekLogger.LogTab("FindDeclusters");
			FindDeclusterPreviews();
			PeekLogger.LogTab("FindSubs");
			FindSubPreviews();

			return _previews.Count > 0 || _declusters.Count > 0
				? PreviewsState.HavePreviews
				: PreviewsState.NoPreviews;
		}

		internal bool TryGetDeclustersOnDirection(GridPoint moveDirection, out IEnumerable<UnitId> declustersOnDirection)
			=> _declusterSystem.TryGetDeclusterUnitIdsOnDirection(moveDirection, out declustersOnDirection);

		private void FindClusterPreviews()
		{
			if (_clusterSystem.TryFindFormableClusters(
				_initiatorId,
				_availableMoves,
				out var infos))
			{
				TransformToPreviews(infos);
				//PeekLogger.LogWarning($"num of Cs {infos.Count()}");
			}
		}

		private void FindDeclusterPreviews()
		{
			if (!_declusterSystem.TryFindDeclusters(_initiatorId))
				return;

			foreach (var (preview, uid) in _declusterSystem.Declusters())
			{
				PreviewId previewId = PreviewId.GetNewID();
				_previews.Add(previewId, preview);
				_declusters.Add(previewId, uid);
			}
		}

		private void FindSubPreviews()//todo its too long! remake
		{
			PeekLogger.LogName(_declusters.Count);
			if (_declusters.Count == 0)
				return;
			//-----------------------------------------------------------------------------
			ClustersDB clusters = DataCenter.Clusters;
			UnitsDB units = DataCenter.Units;
			GridPoint initiatorPosition = DataCenter.Grid.GetPoint(_initiatorId);

			IEnumerable<(ClusterInfo, GridPoint)> superDeclusters =
				from pair in _declusters
				where _previews[pair.Key].PreviewType == PreviewType.Destroy
					&& _previews[pair.Key].ClusterType == ClusterType._7
				select (
					clusters.GetClusterInfo(units.GetClusterId(pair.Value)),
					_previews[pair.Key].PrismFuturePosition);

			//-----------------------------------------------------------------------------
			GridPoint exceptPosition;
			PreviewId previewId;
			Prev_Cluster_Side prevSide;
			bool isHavePreviewDirection;
			IEnumerable<GridPoint> except;

			foreach (var (clusterInfo, previewDirection) in superDeclusters)
			{
				//-----------------------------------------------------------------------------
				isHavePreviewDirection = !previewDirection.IsNAP;
				PeekLogger.Log($"initiator {initiatorPosition}, direction {previewDirection}");
				if (isHavePreviewDirection)
				{
					exceptPosition = previewDirection == initiatorPosition
						? initiatorPosition
						: initiatorPosition + previewDirection;
					PeekLogger.Log($"except {exceptPosition}");
					except = GetClusterPoints(exceptPosition);
				}
				else //todo to many points on except, inefficient
				{
					except = (from av in _availableMoves
							  from cp in GetClusterPoints(av)
							  select cp)
							 .Append(initiatorPosition);
				}
				PeekLogger.LogItems(clusterInfo.Positions);

				//-----------------------------------------------------------------------------
				(GridPoint direction, IEnumerable<ClusterInfo> infos)
					= ClusterSystem.SplitSuperClusterOnClusters(clusterInfo, except);

				//-----------------------------------------------------------------------------
				foreach (var inf in infos)
				{
					PeekLogger.Log($" {inf}");
					previewId = PreviewId.GetNewID();
					prevSide = GetPreviewSide(direction, inf.AxisPoint);
					_subPreviews.Add(previewId,
						 new(PreviewType.Create,
						 ClusterType._3,
						 direction,
						 direction,
						 prevSide
						 ));

					_clusterInfos.Add(previewId, inf);
				}
			}
		}

		private void FindSuperClusterPreviews()
		{
			if (_superClusterSystem.TryFindFormableClusters(
				_initiatorId,
				_availableMoves,
				out var infos))
			{
				TransformToPreviews(infos);
				//PeekLogger.LogWarning($"num of SCs {infos.Count()}");
			}
		}

		private IEnumerable<(PreviewId, PreviewData)> GetMainPreviews()
			=> from preview in _previews
			   select (preview.Key, preview.Value);

		private IEnumerable<(PreviewId, PreviewData)> GetSubPreviews()
			=> from preview in _subPreviews
			   select (preview.Key, preview.Value);

		private bool HaveAvailableMoves()
		{
			_availableMoves = DataCenter.Grid.AllFreeDirectionsTo(_initiatorId).ToArray();
			return _availableMoves.Length > 0;
		}

		private void TransformToPreviews(IEnumerable<DirectionInfo> infos)
		{
			PeekLogger.LogName();
			Prev_Cluster_Side side = Prev_Cluster_Side.NotASide;
			foreach (var item in infos)
			{
				foreach (var clust in item.infos)
				{
					//PeekLogger.LogItemsVarious(clust.AxisPoint, clust.ClusterType, clust.PrismType);
					//PeekLogger.LogItems(clust.Positions);
					PreviewId previewId = PreviewId.GetNewID();
					var previewAxis = clust.AxisPoint;
					if (clust.ClusterType is ClusterType._3)
					{
						side = GetPreviewSide(item.direction, clust.AxisPoint);
						previewAxis = item.direction;
					}
					_previews.Add(previewId,
						new(PreviewType.Create,
						clust.ClusterType,
						previewAxis,
						item.direction,
						side)
						);
					//PeekLogger.LogItemsVarious(previewId, side, item.direction);
					_clusterInfos.Add(previewId, clust);
				}
			}
		}
	}
}