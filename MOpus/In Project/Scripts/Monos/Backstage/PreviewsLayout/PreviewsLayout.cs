using System;
using System.Collections.Generic;
using Extensions;
using Monos.Scene;
using Monos.Scene.Previews;
using NotMonos;
using NotMonos.Databases;
using NotMonos.PreviewsLayout;
using NotMonos.Processors;
using UnityEngine;

namespace Monos.Backstage.Previews
{
	internal sealed partial class PreviewsLayout : SceneSystem
	{
		[SerializeField] private Nest _nest;
		[SerializeField] private PreviewGhostsHolder _clusterPreview;
		[SerializeField] private SelectionMarker _selectIndicator;//todo maybe in another class

		private readonly Stack<(PreviewId, PrismType)> _confirmed = new(1);

		private PreviewsSystem _previews;
		private ScenePreviews _scenePreviews;
		private NestPuddle _puddle;

		private GridPoint _initiatorNewPosition;
		private PreviewId _pickedPreview;
		private UnitId _selectedUnit;
		private bool _lockApplyEvents;
		private bool _isMovedByNest;

		internal event Action ChooseClusterType;

		internal event Action ClearAllEvent;

		internal event Action UnitMoved;

		internal event Action<UnitId> UnitSelected;

		internal event Action UnitUnselected;

		private void OnEnable()
		{
			this.GameObjectNamed("nests", out var parent);
			_puddle = new(new[]
			{
				Instantiate(_nest, parent),
				Instantiate(_nest, parent),
				Instantiate(_nest, parent)
			});
			_puddle.SubscribeOnPickedTo(NestPicked);

			_selectIndicator = Instantiate(_selectIndicator);
			_selectIndicator.gameObject.SetActive(false);

			_previews = new();
			_scenePreviews = new(this, _clusterPreview, _previews);
		}

		internal void ClearAll()
		{
			_puddle.DisableAll();
			_scenePreviews.ClearAll();
			_selectIndicator.gameObject.SetActive(false);
			_previews.ClearAll();
			UnlockApplyEvents();
			_isMovedByNest = false;
			ClearAllEvent.SafeInvoke();

			SceneGlobals.SetState(SceneState.Default);
		}

		internal void ConfirmPickedPreview(PrismType prismType)
		{
			_confirmed.Push((_pickedPreview, prismType));

			PeekLogger.LogItems(_confirmed);
			if (_confirmed.Count > 1)
			{
				PeekLogger.Log($"get subs for {_pickedPreview}");
				GridPoint previewAxis = _previews.GetSubPreviewAxis(_pickedPreview);
				PeekLogger.Log($"pr ax {previewAxis}");
				_scenePreviews.AddConfirmedAxis(previewAxis);
			}

			if (_scenePreviews.NoAvailableSubPreviews || _isMovedByNest)
			{
				PeekLogger.Log("confirm all");
				ConfirmAllPreviews();
				return;
			}

			PeekLogger.Log("have avail");
			PeekLogger.Log($" {_confirmed.Count}");

			LaunchSubSequence();
		}

		internal void DeselectCurrentUnit()
			=> _selectedUnit = null;

		internal void InitiatorSelected(UnitId initiatorId)
		{
			ClearAll(); //todo is it before or after AlreadySelected check?

			if (IsAlreadySelected(initiatorId))
				return;

			PreviewsState state = _previews.PreparePreviews(initiatorId);
			switch (state)
			{
				case PreviewsState.NoMovesAvailable:
					PeekLogger.LogMessageTab("No Moves Available");
					break;

				case PreviewsState.NoPreviews:
					PeekLogger.LogMessageTab("No Previews");
					SceneGlobals.SetState(SceneState.PreviewMode);
					_puddle.EnableNests(_previews.GetAvailableMoves);
					break;

				case PreviewsState.HavePreviews:
					SceneGlobals.SetState(SceneState.PreviewMode);
					_puddle.EnableNests(_previews.GetAvailableMoves);
					LaunchMainSequence();
					PeekLogger.LogMessageTab("All Previews displayed");
					break;

				default:
					break;
			}
		}

		internal void InvokeCancel()
			=> _scenePreviews.CancelInvoked();

		internal void RivalSelected(UnitId rivalId)
		//todo temporal solution. need also check charge on atacker, etc. selecting Rival is not attack order. separate logic
		{
			if (!DataCenter.Grid.IsReachable(_selectedUnit, rivalId))
				return;

			AttackProcessor.GenerateDamage(_selectedUnit, rivalId);
			DeselectCurrentUnit();
		}

		internal void ScrollAllPreviews()
		{
			if (SceneGlobals.CurrentState is not SceneState.PreviewMode)
				return;
			_scenePreviews.ScrollAllPreviews();
		}

		private void ConfirmAllPreviews()
		{
			ConfirmSubPreviews();

			if (_isMovedByNest)
				ConfirmMove();
			else
				ConfirmMainPreview();

			DeselectCurrentUnit();
		}

		private void ConfirmMainPreview()
		{
			PeekLogger.LogMessageTabTab("unit moving by preview");
			if (_confirmed.Count != 1)
				throw new NotImplementedException(); //todo make right exception

			GetCluster(out var clusterInfo, out _pickedPreview);
			GridPoint newPosition = _previews.GetNewPosition(_pickedPreview);
			MoveProcessor.MoveByPreview(_selectedUnit, newPosition, clusterInfo, _previews);
			UnitMoved.SafeInvoke();
			ClearAll();
		}

		private void ConfirmMove()
		{
			PeekLogger.LogMessageObjectsTabTab($"unit moving by nest", _selectedUnit);
			MoveProcessor.MoveByNest(_selectedUnit, _initiatorNewPosition, _previews);
			UnitMoved.SafeInvoke();
			ClearAll();
		}

		private void ConfirmSubPreviews()
		{
			int end = _isMovedByNest
				? 0
				: 1;//last confirmed is main preview
			while (_confirmed.Count > end)
			{
				GetCluster(out var clusterInfo, out _);
				ClusterProcessor.ConfirmCluster(clusterInfo);
			}
		}

		private void EnablePuddleColliders()
			=> _puddle.EnableColliders();

		private void GetCluster(out ClusterInfo clusterInfo, out PreviewId previewId)
		{
			var (pid, type) = _confirmed.Pop();
			clusterInfo = _previews.GetClusterInfo(pid);
			clusterInfo.PrismType = type;
			previewId = pid;
		}

		private bool IsAlreadySelected(UnitId initiatorId)
		{
			string currentSelected = _selectedUnit == null
				? "<color=red>null</red>"
				: _selectedUnit.ToRichString;
			PeekLogger.LogName($"new selected: {initiatorId.ToRichString}; current selected: {currentSelected}");

			if (_selectedUnit && initiatorId.Equals(_selectedUnit))
				return true;

			_selectedUnit = initiatorId;
			ShowSelectorindicator(initiatorId);
			UnitSelected.SafeInvoke(initiatorId);
			return false;
		}

		private void LaunchMainSequence()
		{
			UnlockApplyEvents();
			_scenePreviews.LaunchMainSequence();
		}

		private void LaunchSubSequence()
		{
			PeekLogger.LogName();
			UnlockApplyEvents();
			_puddle.DisableColliders();
			_scenePreviews.LaunchSubSequence();
			SceneGlobals.SetState(SceneState.SubPreviewMode);
		}

		private void LockApplyEvents()
			=> _lockApplyEvents = true;

		private void NestPicked(GridPoint pointPosition)
		{
			PeekLogger.LogName();
			if (_lockApplyEvents)
				return;

			LockApplyEvents();

			_isMovedByNest = true;
			_initiatorNewPosition = pointPosition;

			if (_scenePreviews.NoAvailableSubPreviews)
			{
				PeekLogger.Log($" Have not prevs");
				ConfirmMove();
				DeselectCurrentUnit();
				return;
			}
			PeekLogger.Log($" Have prevs");
			LaunchSubSequence();
		}

		private void PreviewPicked(PreviewId previewId)
		{
			PeekLogger.LogMessageTabTab("Picked preview "+previewId);
			if (_lockApplyEvents)
				return;

			LockApplyEvents();
			_pickedPreview = previewId;
			//dont forget - for subprevs too
			if (!_scenePreviews.InSubSequence)
			{
				UnitUnselected.SafeInvoke(); //hide prism chooser
				_puddle.DisableAll();
			}

			ChooseClusterType.SafeInvoke();
			_scenePreviews.HideAllPreviewsExcept(previewId);
		}

		private void ShowSelectorindicator(UnitId initiatorId)
		{
			Vector3 initiatorPosition = DataCenter.Grid.GetPoint(initiatorId).ToVector3;
			_selectIndicator.transform.position = initiatorPosition;
			_selectIndicator.gameObject.SetActive(true);
		}

		private void UnlockApplyEvents()
			=> _lockApplyEvents = false;

		private void UnselectInitiator()
		{
			ClearAll();
			DeselectCurrentUnit();
			SceneGlobals.SetState(SceneState.Default);//todo consider
		}
	}
}