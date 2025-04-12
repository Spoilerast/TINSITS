using System;
using System.Collections.Generic;
using Extensions;
using NotMonos.Databases;
using NotMonos.Processors;

namespace NotMonos.Previews
{
public sealed class PreviewsModel
{
	private readonly Dictionary<byte, bool> _skipped = new(6);
	private readonly PreviewsSystem _system = new();
	private readonly Stack<(PreviewId, PrismType)> _waited = new(7);
	private (PreviewId, PrismType) _currentPickedInfo;
	private GridPoint _initiatorNewPosition;
	private bool _inSubSequence;
	private PreviewId _mainPreviewId;
	private bool _movingToNest;
	private byte _reclustersCount;
	internal event Action OnShowNextReclusters;
	internal event Action OnSubSequence;

	internal event Action OnTurnEnd;

	internal IEnumerable<GridPoint> GetAvailableMoves => _system.availableMoves;
	internal (byte, byte, byte) GetCounts => _system.counts;
	internal IEnumerable<(PreviewId, PreviewData)> GetPreviews => _system.Previews;
	internal IEnumerable<(PreviewId, PreviewData)> GetSubPreviews => _system.SubPreviews;
	internal IEnumerable<PreviewId> CurrentReclusters => _system.ReclustersOn(_reclustersCount);

	internal bool CancelInvoked()
	{
		PeekLogger.LogNameCollection(_waited);
		_waited.LogItemsThis();
		if (!_inSubSequence || _reclustersCount <= 0)
			return true;

		_reclustersCount--;
		PeekLogger.Log($"_skipped[{_reclustersCount}] is {_skipped[_reclustersCount]}");
		if (!_skipped[_reclustersCount])
			_ = _waited.Pop();
		ReclustersNext();
		return false;
	}

	internal void ClearModel()
	{
		PeekLogger.LogName();
		_reclustersCount = 0;
		_inSubSequence = false;
		_movingToNest = false;
		_waited.Clear();
		_skipped.Clear();
		_system.ClearAll();
	}

	internal void NestPicked(GridPoint position)
	{
		_initiatorNewPosition = position;
		if (!_system.HaveSubPreviews){
			ConfirmNestMove();
			return;
		}

		_movingToNest = true;
		StartSubSequence();
	}

	internal PreviewsState PreparePreviews()
		=> _system.PreparePreviews();

	internal void PreviewPicked(PreviewId pickedPreview, PrismType type)
	{ //behaviour must consider main and sub prevs
		_currentPickedInfo = (pickedPreview, type);
		PeekLogger.LogName(_currentPickedInfo);

		if (_inSubSequence){
			SubPreviewPicked();
			return;
		}

		if (!_system.HaveSubPreviews){
			ConfirmMainPreview(_currentPickedInfo);
			return;
		}

		_mainPreviewId = _currentPickedInfo.Item1;
		AddToWaited(false);
		StartSubSequence();
	}

	internal void SkipOneRecluster()
	{
		_skipped[_reclustersCount] = true;
		_reclustersCount++;
		ReclustersNext();
	}

	private void AddToWaited(bool addToReclusters = true)
	{
		PeekLogger.LogName(_currentPickedInfo);
		_waited.Push(_currentPickedInfo);
		if (!addToReclusters)
			return;

		_skipped[_reclustersCount] = false;
		_reclustersCount++;
	}

	private void CheckSelfDecluster()
	{
		if (_system.HaveSelfDecluster(out ClusterId clusterId))
			ClusterProcessor.Declusterize(clusterId);
	}

	private void ConfirmAllWaited()
	{
		PeekLogger.LogName();
		const int mainPreviewIndex = 0;

		if (_movingToNest){
			ConfirmNestMove();
			return;
		}

		ClusterProcessor.DeclusterizeRange(_system.DeclusterIds(_mainPreviewId));
		for (int i = _waited.Count - 1; i > mainPreviewIndex; i--)
			ConfirmSubPreview(_waited.Pop());

		ConfirmMainPreview(_waited.Pop());
	}

	private void ConfirmMainPreview((PreviewId pid, PrismType type) tuple)
	{
		PeekLogger.LogName();
		if (!_inSubSequence)
			CheckSelfDecluster();

		_system.InfoAndNewPosition(tuple.pid, out ClusterInfo clusterInfo, out GridPoint newPosition);
		clusterInfo.PrismType = tuple.type;
		MoveProcessor.MoveToPreview(newPosition, clusterInfo);
		OnTurnEnd.SafeInvoke();
	}

	private void ConfirmNestMove()
	{
		CheckSelfDecluster();
		MoveProcessor.MoveToNest(_initiatorNewPosition);
		OnTurnEnd.SafeInvoke();
	}

	private void ConfirmSubPreview((PreviewId pid, PrismType type) tuple)
	{
		PeekLogger.LogName(tuple);
		ClusterInfo clusterInfo = _system.ClusterInfo(tuple.pid);
		clusterInfo.PrismType = tuple.type;
		ClusterProcessor.ConfirmCluster(clusterInfo);
	}

	private void ReclustersNext()
	{
		PeekLogger.LogName($"sys rec {_system.ReclustersCount}; mod rec {_reclustersCount}");
		_waited.LogItemsThis();
		if (_system.ReclustersCount == _reclustersCount){
			ConfirmAllWaited();
			return;
		}

		OnShowNextReclusters.SafeInvoke();
	}

	private void StartSubSequence()
	{
		PeekLogger.LogName();
		if (_movingToNest){
			if (!_system.TryPrepareSubPreviews()){
				ConfirmNestMove();
				return;
			}
		}
		else if (!_system.TryPrepareSubPreviews(_waited.Peek().Item1)){
			ConfirmMainPreview(_currentPickedInfo);
			return;
		}

		_inSubSequence = true;
		OnSubSequence.SafeInvoke();
		//lock nests if active
	}

	private void SubPreviewPicked()
	{
		PeekLogger.LogName();
		AddToWaited();
		ReclustersNext();
	}
}
}