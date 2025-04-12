using System.Collections.Generic;
using Extensions;
using NotMonos.Databases;

namespace NotMonos.Previews
{
internal sealed class PreviewsSystem
{
	private readonly Dictionary<PreviewId, ClusterInfo> _clusterInfos = new(33);
	private readonly FindClusterPreviews _clusters = new();

	private readonly Dictionary<PreviewId, HashSet<ClusterId>> _declusters = new(9); //need better name
	private readonly Dictionary<PreviewId, PreviewData> _previewsMain = new(33);
	private readonly Dictionary<PreviewId, PreviewData> _previewsSub = new(14);
	private readonly Dictionary<byte, HashSet<PreviewId>> _reclusters = new(14); //need better name
	private readonly List<ClusterId> _subDeclustersIds = new(9);
	private readonly FindSuperClusterPreviews _superClusters = new();
	private ClusterId _initiatorClusterId;
	internal GridPoint[] availableMoves;

	internal (byte nests, byte clusters, byte superClusters) counts;

	internal byte ReclustersCount { get; private set; }
	internal bool HaveSubPreviews => _subDeclustersIds.Count > 0;

	internal IEnumerable<(PreviewId, PreviewData)> Previews => _previewsMain.PairsToTuples();
	internal IEnumerable<(PreviewId, PreviewData)> SubPreviews => _previewsSub.PairsToTuples();

	public PreviewsState PreparePreviews()
	{
		//ClearAll();

		if (!FindPreviewsSystem.HaveAvailableMoves(out counts.nests, out availableMoves))
			return PreviewsState.NoMovesAvailable; //questionable, cuz SCl WorstCase

		PeekLogger.LogTab("FindSelfDecluster");
		InitiatorHaveClusterCheck();
		PeekLogger.LogTab("FindClusters");
		FindPreviews(_clusters, out counts.clusters);
		PeekLogger.LogTab("FindSuperClusters");
		FindPreviews(_superClusters, out counts.superClusters);
		counts.LogTabThis();
		_previewsMain.Count.LogThis();
		_declusters.Count.LogThis();
		bool haveselfdec = HaveSelfDecluster(out ClusterId cid);
		PeekLogger.Log($"HaveSelfDecluster is {haveselfdec}  {(cid == null ? "" : cid)}");
		HaveSubPreviews.LogThis();
		return _previewsMain.Count > 0 || _declusters.Count > 0
			? PreviewsState.HavePreviews
			: PreviewsState.NoPreviews;
	}

	internal void ClearAll()
	{
		ReclustersCount = 0;
		_initiatorClusterId = null;
		_previewsMain.Clear();
		_previewsSub.Clear();
		_declusters.Clear();
		_subDeclustersIds.Clear();
		_clusters.Clear();
		_superClusters.Clear();
		PreviewId.ClearCache();
	}

	internal ClusterInfo ClusterInfo(PreviewId previewId)
		=> _clusterInfos[previewId];

	internal IEnumerable<ClusterId> DeclusterIds(PreviewId previewId)
		=> _declusters[previewId];

	internal bool HaveSelfDecluster(out ClusterId clusterId)
	{
		clusterId = _initiatorClusterId;
		return _initiatorClusterId != null;
	}

	internal void InfoAndNewPosition(PreviewId previewId, out ClusterInfo clusterInfo, out GridPoint futurePosition)
		=> (clusterInfo, futurePosition) = (ClusterInfo(previewId), _previewsMain[previewId].PrismFuturePosition);

	internal IEnumerable<PreviewId> ReclustersOn(byte counter)
		=> _reclusters[counter];

	internal bool TryPrepareSubPreviews()
	{
		PeekLogger.LogName("for nest move");
		if (!FindSuperClusterPreviews.TrySplitSuperCluster(_initiatorClusterId, out DirectionInfo reclusters))
			return false;

		ReclustersCount = 0;
		MakeReclusters(reclusters);
		PeekLogger.LogItems(_previewsSub);
		return _previewsSub.Count > 0;
	}

	internal bool TryPrepareSubPreviews(PreviewId previewId)
	{
		PeekLogger.LogName(previewId);

		if (!_declusters.TryGetValue(previewId, out HashSet<ClusterId> ids))
			return false;

		ReclustersCount = 0;
		foreach (ClusterId clusterId in ids){
			if (!FindSuperClusterPreviews
					.TrySplitSuperCluster(clusterId,
										  _clusterInfos[previewId].Positions,
										  out DirectionInfo reclusters))
				continue;

			MakeReclusters(reclusters);
		}

		PeekLogger.LogItems(_previewsSub);
		return _previewsSub.Count > 0;
	}

	private void AddDeclusterPreview(ClusterId clusterId, ClusterType clusterType, ScrollStatus scrollStatus)
	{
		PeekLogger.LogName();
		ClusterInfo clusterInfo = DataCenter.Clusters.GetClusterInfo(clusterId);

		bool isSuperCluster = clusterType == ClusterType._7;
		GridPoint previewAxis = isSuperCluster
			? clusterInfo.AxisPoint
			: clusterInfo.Positions[0];
		HolderSide previewSide = isSuperCluster
			? default
			: Constants.GetPreviewSide(clusterInfo.Positions[0], clusterInfo.AxisPoint);

		if (isSuperCluster)
			_subDeclustersIds.Add(clusterId);

		_subDeclustersIds.LogItemsThis();
		AddPreview(PreviewType.Destroy,
				   PreviewId.GetNewID(),
				   clusterType,
				   previewAxis,
				   GridPoint.NAP,
				   previewSide,
				   scrollStatus);
	}

	private void AddPreview(PreviewType type,
							PreviewId previewId,
							ClusterType clusterType,
							GridPoint previewAxis,
							GridPoint futurePosition,
							HolderSide side,
							ScrollStatus scrollStatus)
		=> _previewsMain[previewId] = new(type, clusterType, previewAxis, futurePosition, side, scrollStatus);

	private void CreatePreviewsOnDirection(DirectionInfo item)
	{
		PeekLogger.LogName();
		foreach (ClusterInfo clusterInfo in item.infos){
			var previewId = PreviewId.GetNewID();
			bool isSuperCluster = clusterInfo.ClusterType == ClusterType._7;
			GridPoint previewAxis = isSuperCluster
				? clusterInfo.AxisPoint
				: item.direction;
			HolderSide side = isSuperCluster
				? HolderSide.NotASide
				: Constants.GetPreviewSide(item.direction, clusterInfo.AxisPoint);
			ScrollStatus scrollStatus = isSuperCluster
				? ScrollStatus.SuperClusters
				: ScrollStatus.Clusters;

			_clusterInfos[previewId] = clusterInfo;
			AddPreview(PreviewType.Create,
					   previewId,
					   clusterInfo.ClusterType,
					   previewAxis,
					   item.direction,
					   side,
					   scrollStatus);

			MakeDeclusters(previewId, scrollStatus);
		}
	}

	private void FindPreviews(FindPreviewsSystem findSystem, out byte previewsCount)
	{
		PeekLogger.LogName();
		if (findSystem.TryFindFormableClusters(availableMoves, out IEnumerable<DirectionInfo> infos))
			TransformToPreviews(infos);
		previewsCount = findSystem.PreviewsCount;
		previewsCount.LogTabThis();
	}

	private void InitiatorHaveClusterCheck()
	{
		if (!FindPreviewsSystem.InitiatorInCluster(out ClusterId clusterId, out ClusterType type))
			return;

		PeekLogger.LogName();
		_initiatorClusterId = clusterId;
		AddDeclusterPreview(clusterId, type, ScrollStatus.All);
	}

	private void MakeDecluster(GridPoint position, ScrollStatus scrollStatus, ref HashSet<ClusterId> set)
	{
		if (!FindPreviewsSystem.TryGetClusterIdFromPoint(position, out ClusterId clusterId, out ClusterType type))
			return;

		if (!set.Add(clusterId))
			return;

		AddDeclusterPreview(clusterId, type, scrollStatus);
	}

	private void MakeDeclusters(PreviewId previewId, ScrollStatus scrollStatus)
	{
		HashSet<ClusterId> uniqueClusters = HaveSelfDecluster(out _)
			? new() { _initiatorClusterId }
			: new();

		foreach (GridPoint position in _clusterInfos[previewId].Positions)
			MakeDecluster(position, scrollStatus, ref uniqueClusters);

		_declusters[previewId] = uniqueClusters;
	}

	private void MakeReclusters(DirectionInfo directionInfo)
	{
		PeekLogger.LogName(ReclustersCount);
		HashSet<PreviewId> reclusterSet = new(4);
		PreviewId previewId;
		foreach (ClusterInfo info in directionInfo.infos){
			previewId = PreviewId.GetNewID();
			_clusterInfos[previewId] = info;
			_previewsSub[previewId] = new(PreviewType.Create,
										  ClusterType._3,
										  directionInfo.direction,
										  GridPoint.NAP,
										  Constants.GetPreviewSide(directionInfo.direction, info.AxisPoint),
										  ScrollStatus.Clusters); //clusters or superclusters?
			reclusterSet.Add(previewId);
		}

		_reclusters[ReclustersCount] = reclusterSet;
		_reclusters[ReclustersCount].LogItemsThis();
		ReclustersCount++;
		PeekLogger.Log($"recluster count at end = {ReclustersCount}");
	}

	private void TransformToPreviews(IEnumerable<DirectionInfo> infos)
	{
		PeekLogger.LogName();
		foreach (DirectionInfo item in infos)
			CreatePreviewsOnDirection(item);
	}
}
}