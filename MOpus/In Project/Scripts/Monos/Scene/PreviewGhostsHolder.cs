using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Monos.Scene;
using Monos.Scene.Previews;
using NotMonos;
using NotMonos.Backstage;
using UnityEngine;

namespace Monos.Backstage.NewPreviews
{
internal sealed class PreviewGhostsHolder : SceneObject
{
	[SerializeField] private SuperClusterGhost _superClusterGhost;
	[SerializeField] private ClusterGhost _forwardRightGhost;
	[SerializeField] private ClusterGhost _backRightGhost;
	[SerializeField] private ClusterGhost _backGhost;
	[SerializeField] private ClusterGhost _backLeftGhost;
	[SerializeField] private ClusterGhost _forwardLeftGhost;
	[SerializeField] private ClusterGhost _forwardGhost;

	private readonly Dictionary<HolderSide, Preview> _ghosts = new(7);
	private readonly Dictionary<PreviewId, HolderSide> _previews = new(7);
	private readonly Dictionary<HolderSide, ScrollStatus> _previewsStatusMap = new(7);

	internal event Action<PreviewId> OnPickedEvent;

	private void Awake()
	{
		AddGhost(HolderSide.Forward, _forwardGhost);
		AddGhost(HolderSide.ForwardLeft, _forwardLeftGhost);
		AddGhost(HolderSide.ForwardRight, _forwardRightGhost);
		AddGhost(HolderSide.BackLeft, _backLeftGhost);
		AddGhost(HolderSide.Back, _backGhost);
		AddGhost(HolderSide.BackRight, _backRightGhost);
		AddGhost(HolderSide.NotASide, _superClusterGhost);
	}

	public void DeactivateSuperDecluster()
	{
		PeekLogger.LogName();
		SuperClusterGhost ghost = _superClusterGhost;
		ghost.Deactivate();
		/*PeekLogger.LogName(ghost.isActiveAndEnabled);
		if (ghost.isActiveAndEnabled)
			ghost.Deactivate();
		else
			ghost.Activate();*/
	}

	public void HideHolderIfNotContains(IEnumerable<PreviewId> reclusterToShow)
	{
		IEnumerable<PreviewId> previewIds = reclusterToShow.CastToArray();
		if (!previewIds.Any(_previews.ContainsKey)){
			Deactivate();
			return;
		}

		if (!isActiveAndEnabled)
			Activate();

		foreach (PreviewId id in _previews.Keys.Except(previewIds))
			_ghosts[_previews[id]].Deactivate();
	}

	internal void AddPreview(PreviewType type, ScrollStatus status, HolderSide side, PreviewId previewId)
	{
		//PeekLogger.Log($"add prev {side} type {type}");
		_previewsStatusMap[side] = status;

		if (type is PreviewType.Create)
			AddCreatePreview(previewId, side);
		else
			AddDestroyPreview(side);
	}

	internal void ScrollView(ClusterStatus status)
	{
		foreach (KeyValuePair<HolderSide, ScrollStatus> pair in _previewsStatusMap)
			if (IsViable(status, pair.Value))
				_ghosts[pair.Key].Activate();
			else
				_ghosts[pair.Key].Deactivate();
	}

	private void AddCreatePreview(PreviewId previewId, HolderSide side)
	{
		_ghosts[side].OnPicked += OnPickedEvent;
		_ghosts[side].Id = previewId;
		_previews[previewId] = side;
		if (SceneGlobals.InEditor)
			name += $"; {previewId}";
	}

	private void AddDestroyPreview(HolderSide side)
	{
		_ghosts[side]
			.Initialize()
			.MakeDeclusterPreview();
	}

	private void AddGhost(HolderSide side, Preview ghost)
	{
		if (!ghost)
			throw new ArgumentNullException(nameof(ghost), "Previews in ClusterPreviews must not be null");

		_ghosts[side] = ghost;
	}

	private static bool IsViable(ClusterStatus status, ScrollStatus scrollStatus)
		=> (status, scrollStatus) switch {
			(_, ScrollStatus.All)                                      => true,
			(ClusterStatus.Clustered, ScrollStatus.Clusters)           => true,
			(ClusterStatus.SuperClustered, ScrollStatus.SuperClusters) => true,
			_                                                          => false
		};
}
}