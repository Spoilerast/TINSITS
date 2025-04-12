using System;
using System.Collections.Generic;
using Extensions;
using Monos.Backstage.NewPreviews;
using NotMonos.Backstage;
using NotMonos.Databases;

namespace NotMonos.Previews
{
public sealed class PreviewsView
{
	private readonly PreviewGhostsHolder _holder;
	private readonly Dictionary<GridPoint, PreviewGhostsHolder> _holdersOnScene = new(21);

	private int _currentScrollIndex;
	private List<ClusterStatus> _scrollTypes;

	internal PreviewsView(PreviewGhostsHolder holder) { _holder = holder; }

	internal event Action<PreviewId> PreviewPicked;
	internal event Action<ClusterStatus> PreviewsScrolledEvent;

	internal void ClearView()
	{
		PeekLogger.LogName();
		_holdersOnScene.Values.ForEach(x => x.DestroySceneObject());
		_holdersOnScene.Clear();
		//_holdersOnScene.Count.LogThis();
	}

	internal void HideAllExcept(IEnumerable<PreviewId> currentReclusters)
	{
		PeekLogger.LogName();
		_holdersOnScene.Values.ForEach(x => x.HideHolderIfNotContains(currentReclusters));
	}

	internal void MainSequence(IEnumerable<(PreviewId, PreviewData)> previews)
	{
		PeekLogger.LogName();
		CreatePreviews(previews);
	}

	internal void ScrollAllPreviews()
	{
		var status = ClusterStatus.NotClustered;

		if (_scrollTypes.Count > 1){
			status = _scrollTypes[_currentScrollIndex];
			_currentScrollIndex = (_currentScrollIndex + 1) % _scrollTypes.Count;
		}

		PeekLogger.LogName(status);
		_holdersOnScene.Values.ForEach(x => x.ScrollView(status));
		PreviewsScrolledEvent.SafeInvoke(status);
	}

	internal void SubSequence(IEnumerable<(PreviewId, PreviewData)> previews)
	{
		PeekLogger.LogName();
		CreatePreviews(previews);
		_holdersOnScene.Values.ForEach(x => x.DeactivateSuperDecluster());
	}

	private void AddClusterTypeToScrolls(ClusterType clusterType)
	{
		ClusterStatus status = clusterType is ClusterType._3
			? ClusterStatus.Clustered
			: ClusterStatus.SuperClustered;

		if (_scrollTypes.Contains(status))
			return;

		//PeekLogger.LogTabTab($"add {status} to scrolls");
		_scrollTypes.Add(status);
	}

	private void CreatePreview(PreviewId previewId, PreviewData data)
	{
		GridPoint previewAxis = data.PreviewAxis;
		if (!LayoutHaveThisPreview(previewAxis, out PreviewGhostsHolder holder)){
			InstantiatePreview(previewAxis, out holder);
			holder.OnPickedEvent += PreviewPicked;
		}

		holder.AddPreview(data.PreviewType, data.ScrollStatus, data.Side, previewId);
		if (data.PreviewType is PreviewType.Create)
			AddClusterTypeToScrolls(data.ClusterType);
	}

	private void CreatePreviews(IEnumerable<(PreviewId, PreviewData)> previewsData)
	{
		//PeekLogger.LogNameCollection(previewsData);
		InitializeScrolls();
		previewsData.ForEach(x => CreatePreview(x.Item1, x.Item2));
		ScrollAllPreviews();
	}

	private void InitializeScrolls()
	{
		_scrollTypes = new(3) { ClusterStatus.NotClustered };
		_currentScrollIndex = _scrollTypes.Count;
	}

	private void InstantiatePreview(GridPoint previewAxis, out PreviewGhostsHolder holder)
	{
		_ = UnityExtensions.TryInstantiate(_holder, previewAxis, out holder);
		_holdersOnScene.Add(previewAxis, holder);

		if (SceneGlobals.InEditor)
			holder.name = previewAxis.ToString();
	}

	private bool LayoutHaveThisPreview(GridPoint previewAxis, out PreviewGhostsHolder holder)
		=> _holdersOnScene.TryGetValue(previewAxis, out holder);
}
}