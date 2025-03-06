using System;
using System.Collections.Generic;
using System.Linq;
using NotMonos;
using UnityEngine;

namespace Monos.Scene.Previews
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
		//[Space]
		//[SerializeField] private Material _declusterGhost;

		private readonly Dictionary<Prev_Cluster_Side, Preview> _ghosts = new(7);
		private readonly Dictionary<PreviewId, Prev_Cluster_Side> _previews = new(7);

		private bool _containsDecluster = false;
		private bool _containsHex = false;
		private bool _containsTri = false;

		internal event Action<PreviewId> OnPickedEvent;

		private IEnumerable<Prev_Cluster_Side> EnabledTrisSides
			=> from prev in _previews
			   where prev.Value != Prev_Cluster_Side.NotASide
			   select prev.Value;

		internal void AddPreview(PreviewId previewId, Prev_Cluster_Side side, PreviewType type)
		{
			//PeekLogger.Log($"add prev {side} type {type}");
			if (type is PreviewType.Create)
			{
				_ghosts[side].OnPicked += OnPickedEvent;
				_ghosts[side].Id = previewId;
				_previews.Add(previewId, side);
				if (side is Prev_Cluster_Side.NotASide)
				{
					_containsHex = true;
					return;
				}
				_containsTri = true;
			}
			else
			{
				_containsDecluster = true;
				_ghosts[side]
					.Initialize()
					.MakeDeclusterPreview()
					.Activate();
			}
		}

		internal void Clear()//todo unsubscribe?
		{
			_previews.Clear();
			_containsHex = false;
			_containsTri = false;
		}

		internal void HideAllExcept(PreviewId previewId)
		{
			if (!_containsTri)
				return;

			IEnumerable<Prev_Cluster_Side> trisToHide =
				from prev in _previews
				where prev.Key != previewId && prev.Value != Prev_Cluster_Side.NotASide
				select prev.Value;
			foreach (var side in trisToHide)
				_ghosts[side].Deactivate();
		}

		internal bool IsHavePreview(PreviewId previewId)
			=> _previews.ContainsKey(previewId) || _containsDecluster;

		internal void ScrollView(ClusterStatus status)
		{
			if (status is ClusterStatus.SuperClustered)
			{
				DisableTrisGhosts();
				EnableHexGhost();
			}
			else if (status is ClusterStatus.Clustered)
			{
				DisableHexGhost();
				EnableTrisGhosts();
			}
			else
			{
				DisableHexGhost();
				DisableTrisGhosts();
			}
		}

		private void AddGhost(Prev_Cluster_Side side, Preview ghost)
		{
			if (!ghost)
				throw new ArgumentNullException("ghost", "Previews in ClusterPreviews must not be null");
			_ghosts[side] = ghost;
		}

		private void Awake()
		{
			AddGhost(Prev_Cluster_Side.Forward, _forwardGhost);
			AddGhost(Prev_Cluster_Side.ForwardLeft, _forwardLeftGhost);
			AddGhost(Prev_Cluster_Side.ForwardRight, _forwardRightGhost);
			AddGhost(Prev_Cluster_Side.BackLeft, _backLeftGhost);
			AddGhost(Prev_Cluster_Side.Back, _backGhost);
			AddGhost(Prev_Cluster_Side.BackRight, _backRightGhost);
			AddGhost(Prev_Cluster_Side.NotASide, _superClusterGhost);
		}

		private void DisableHexGhost()
		{
			if (!_containsHex)
				return;

			_superClusterGhost.Deactivate();
		}

		private void DisableTrisGhosts()
		{
			if (!_containsTri)
				return;

			foreach (var side in EnabledTrisSides)
				_ghosts[side].Deactivate();
		}

		private void EnableHexGhost()
		{
			if (!_containsHex)
				return;
			_superClusterGhost.Activate();
		}

		private void EnableTrisGhosts()
		{
			if (!_containsTri && !_containsDecluster)
				return;

			foreach (var side in EnabledTrisSides)
				_ghosts[side].Activate();
		}
	}
}