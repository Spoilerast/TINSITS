using System.Collections.Generic;
using System.Linq;
using Extensions;
using Monos.Scene.Previews;
using NotMonos;
using NotMonos.Databases;
using NotMonos.PreviewsLayout;
using UnityEngine;

namespace Monos.Backstage.Previews
{
	internal sealed partial class PreviewsLayout //todo need to consider
	{
		private class ScenePreviews
		{
			private readonly PreviewGhostsHolder _clusterPreview;
			private readonly Dictionary<Vector2, PreviewGhostsHolder> _clusterPreviewsOnScene = new(21);
			private readonly List<GridPoint> _confirmedSubPreviewsAxises = new();
			private readonly PreviewsLayout _layout;
			private readonly PreviewsSystem _previews;
			private int _currentScrollIndex;
			private bool _inLateSequence;
			private List<ClusterStatus> _scrollTypes;
			private (PreviewId, PreviewData)[] _subPreviewsData;

			public ScenePreviews(PreviewsLayout layout, PreviewGhostsHolder clusterPreview, PreviewsSystem previewsSystem)
			{
				_clusterPreview = clusterPreview;
				_layout = layout;
				_previews = previewsSystem;
			}

			internal bool InSubSequence { get; private set; }

			internal bool NoAvailableSubPreviews
				=> !HaveAvailableSubPreviews();

			internal void AddConfirmedAxis(GridPoint previewAxis)
			{
				PeekLogger.LogName(previewAxis);
				_confirmedSubPreviewsAxises.Add(previewAxis);
				_inLateSequence = true;
			}

			internal void CancelInvoked()
			{
				if (_inLateSequence)
				{
					PeekLogger.LogTab("clear late seq");
					ClearScene();
					ClearLateSequence();
					ShowSubPreviews();
					return;
				}

				if (InSubSequence)
				{
					PeekLogger.LogTab("clear sub seq");
					ClearScene();
					_layout.EnablePuddleColliders();
					ShowMainPreviews();
					return;
				}

				_layout.UnselectInitiator();
			}

			internal void ClearAll()
			{
				ClearScene();
				ClearLateSequence();
				_subPreviewsData = null;
			}

			internal void CreatePreview(in PreviewId previewId, PreviewData data)
			{
				GridPoint previewAxis = data.PreviewAxis;
				if (!LayoutHaveThisPreview(previewAxis, out var clusterPreview))
				{
					InstantiatePreview(previewAxis, out clusterPreview);
					clusterPreview.OnPickedEvent += _layout.PreviewPicked;
				}

				clusterPreview.AddPreview(previewId, data.Side, data.PreviewType);
				if (data.PreviewType is PreviewType.Create)
					AddClusterTypeToScrolls(data.ClusterType);
			}

			internal void HideAllPreviewsExcept(PreviewId previewId)
			{
				KeyValuePair<Vector2, PreviewGhostsHolder> pair;
				PreviewGhostsHolder preview;
				for (int i = _clusterPreviewsOnScene.Count - 1; i >= 0; i--)
				{
					pair = _clusterPreviewsOnScene.ElementAt(i);
					preview = pair.Value;

					if (!preview.IsHavePreview(previewId))
					{
						preview.DestroySceneObject(); //todo maybe disable?
						_ = _clusterPreviewsOnScene.Remove(pair.Key);
						continue;
					}
					preview.HideAllExcept(previewId);
				}
			}

			internal void InstantiatePreview(GridPoint previewAxis, out PreviewGhostsHolder clusterPreview)
			{
				_ = UnityExtensions.TryInstantiate(_clusterPreview, previewAxis, out clusterPreview);
				_clusterPreviewsOnScene.Add(previewAxis.ToVector2, clusterPreview);
				clusterPreview.SetName(previewAxis);
			}

			internal void LaunchMainSequence()
			{
				InSubSequence = false;
				ShowMainPreviews();
			}

			internal void LaunchSubSequence()
			{
				PeekLogger.LogName();
				InSubSequence = true;
				ClearScene();
				ShowSubPreviews();
			}

			internal bool LayoutHaveThisPreview(GridPoint previewAxis, out PreviewGhostsHolder clusterPreview)
			{
				clusterPreview = default;
				var key = previewAxis.ToVector2;
				if (_clusterPreviewsOnScene.ContainsKey(key))
				{
					clusterPreview = _clusterPreviewsOnScene[key];
					return true;
				}
				return false;
			}

			internal void ScrollAllPreviews()
			{
				if (_scrollTypes.Count <= 1)
					return;

				ScrollAllPreviews(_scrollTypes[_currentScrollIndex]);
				_currentScrollIndex = (_currentScrollIndex + 1) % _scrollTypes.Count;
			}

			internal void ShowMainPreviews()
			{
				IEnumerable<(PreviewId, PreviewData)> previewsData = _previews.GetPreviews(PreviewsCollectionType.MainPreviews);
				PeekLogger.LogNameCollection(previewsData);
				CreatePreviews(previewsData);
			}

			internal void ShowSubPreviews()
			{/*
				if (_confirmedSubPreviewsAxises.Count == 0)
				{
					var previewsData = _previews.GetPreviews(PreviewsCollectionType.SubPreviews);
					PeekLogger.LogNameCollection(previewsData);
					PeekLogger.LogNameCollection();
					CreatePreviews(previewsData);
					return;
				}
				var previewsData2 =
					from pr in _previews.GetPreviews(PreviewsCollectionType.SubPreviews)
					from ca in _confirmedSubPreviewsAxises
					where pr.Item2.PreviewAxis != ca
					select pr;
				PeekLogger.LogNameCollection(previewsData2);*/
				PeekLogger.LogNameCollection(_subPreviewsData);
				CreatePreviews(_subPreviewsData);
			}

			private void AddClusterTypeToScrolls(ClusterType clusterType)
			{
				var status = clusterType is ClusterType._3
					? ClusterStatus.Clustered
					: ClusterStatus.SuperClustered;

				if (_scrollTypes.Contains(status))
					return;

				PeekLogger.LogTabTab($"add {status} to scrolls");
				_scrollTypes.Add(status);
			}

			private void ClearLateSequence()
			{
				_confirmedSubPreviewsAxises.Clear();
				_inLateSequence = false;
				_subPreviewsData = _previews.GetPreviews(PreviewsCollectionType.SubPreviews).ToArray();
			}

			private void ClearScene()
			{
				while (_clusterPreviewsOnScene.Count > 0)
				{
					KeyValuePair<Vector2, PreviewGhostsHolder> pair = _clusterPreviewsOnScene.Last();
					pair.Value.DestroySceneObject();
					_ = _clusterPreviewsOnScene.Remove(pair.Key);
				}
			}

			private void CreatePreviews(IEnumerable<(PreviewId, PreviewData)> previewsData)
			{
				InitializeScrolls();
				foreach (var (id, data) in previewsData)
					CreatePreview(id, data);

				ScrollAllPreviews();
			}

			private bool HaveAvailableSubPreviews()
			{
				PeekLogger.LogName($"have sub {_previews.HaveSubPreviews} confirmed {_confirmedSubPreviewsAxises.Count}");
				if (!_previews.HaveSubPreviews)
					return false;

				PeekLogger.LogItems(_confirmedSubPreviewsAxises);

				IEnumerable<(PreviewId, PreviewData)> subPrevsWithoutConfirmed = _confirmedSubPreviewsAxises.Count == 0
					? _previews.GetPreviews(PreviewsCollectionType.SubPreviews)
					: (
						from tuple in _previews.GetPreviews(PreviewsCollectionType.SubPreviews)
						where !_confirmedSubPreviewsAxises.Contains(tuple.Item2.PreviewAxis)
						select tuple
					);
				PeekLogger.LogItems(subPrevsWithoutConfirmed);
				if (!subPrevsWithoutConfirmed.Any())
					return false;

				_subPreviewsData = subPrevsWithoutConfirmed.ToArray();
				return true;
			}

			private void InitializeScrolls()
			{
				_scrollTypes = new(3) { ClusterStatus.NotClustered };
				_currentScrollIndex = _scrollTypes.Count;
			}

			private void ScrollAllPreviews(ClusterStatus status)
			{
				foreach (var cluster in _clusterPreviewsOnScene.Values)
					cluster.ScrollView(status);
			}
		}
	}
}