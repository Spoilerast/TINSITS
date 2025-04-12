using System;
using Extensions;
using Monos;
using Monos.Backstage.NewPreviews;
using Monos.Scene;
using NotMonos.Backstage;
using NotMonos.Databases;
using UnityEngine;

namespace NotMonos.Previews
{
public sealed class PreviewsController : SceneSystem
{
	[SerializeField] private PreviewGhostsHolder _ghostsHolder;
	[SerializeField] private Nest _nest;

	private readonly PreviewsModel _model = new();
	private bool _inSubSequence;
	private bool _pickDisallowed;
	private PreviewId _pickedPreview;
	private NestPuddle _puddle;
	private PreviewsView _view;

	internal event Action<(byte, byte, byte)> AvailableCounted;
	internal event Action ChooseClusterType;
	internal event Action ClearAllEvent;
	internal event Action OnSubSequence;
	internal event Action<ClusterStatus> PreviewsScrolledEvent;
	internal event Action ShowCancelWindow;
	internal event Action UnitMoved;
	internal event Action UnitUnselected;

	private void Awake()
	{ //todo handle all this events mess (unsubscribe in proper places)
		_view = new(_ghostsHolder);
		_puddle = new(_nest);
		_puddle.SubscribeOnPickedTo(NestPicked);
		SceneGlobals.OnClearScene += ClearAll;
		DataCenter.Selection.UnitSelected += UnitSelected;
		_view.PreviewPicked += PreviewPicked;
		_view.PreviewsScrolledEvent += PreviewsScrolled;
		PreviewsScrolledEvent += _puddle.PreviewsScrolled;
		_model.OnTurnEnd += EndTurn;
		_model.OnSubSequence += LaunchSubSequence;
		_model.OnShowNextReclusters += ShowReclusters;
	}

	internal void ConfirmPickedPreview(PrismType type)
	{
		_model.PreviewPicked(_pickedPreview, type);
		_pickDisallowed = false;
	}

	internal void InvokeCancel()
	{
		if (_inSubSequence){
			bool returnToMain = _model.CancelInvoked();
			if (!returnToMain){
				ShowCancelWindow.SafeInvoke();
				return;
			}

			PeekLogger.LogWarning("returnToMain");
			_inSubSequence = false;
		}

		ClearAll();
	}

	internal void InvokeSkip()
	{
		_model.SkipOneRecluster();
		_pickDisallowed = false;
	}

	internal void ScrollAllPreviews()
	{
		if (SceneGlobals.CurrentState is not SceneState.PreviewMode)
			return;

		_view.ScrollAllPreviews();
	}

	private void ClearAll()
	{
		PeekLogger.LogName();
		ClearScene();
		_model.ClearModel();
		UnselectUnit();
		SceneGlobals.SetState(SceneState.Default);
		ClearAllEvent.SafeInvoke();
	}

	private void ClearScene()
	{
		_inSubSequence = false;
		_pickDisallowed = false;
		PeekLogger.LogName();
		UnpickPreview();
		_puddle.DisableAll();
		_view.ClearView();
	}

	private void EndTurn()
	{
		PeekLogger.LogName();
		ClearAll();
		UnitMoved.SafeInvoke();
	}

	private void LaunchSubSequence()
	{
		PeekLogger.LogName();
		_inSubSequence = true;
		_view.SubSequence(_model.GetSubPreviews);
		ShowReclusters();
		OnSubSequence.SafeInvoke();
	}

	private void NestPicked(GridPoint obj)
		=> _model.NestPicked(obj);

	private void PreviewPicked(PreviewId previewId)
	{
		PeekLogger.LogName(previewId);
		if (_pickDisallowed)
			return;
		//_pickDisallowed.LogThis();
		ChooseClusterType.SafeInvoke();
		_pickDisallowed = true;
		_pickedPreview = previewId;
	}

	private void PreviewsScrolled(ClusterStatus obj)
		=> PreviewsScrolledEvent.SafeInvoke(obj);

	private void ShowReclusters()
		=> _view.HideAllExcept(_model.CurrentReclusters);

	private void UnitSelected()
	{
		PreviewsState state = _model.PreparePreviews();
		string message = state switch {
			PreviewsState.NoMovesAvailable => "(0) No Moves Available",
			PreviewsState.NoPreviews       => "(1) No Previews",
			PreviewsState.HavePreviews     => "(2) All Previews displayed",
			_                              => "Error"
		};
		PeekLogger.LogMessageTab(message);
		if (state == PreviewsState.NoMovesAvailable){
			if (DataCenter.SelectedIsNotClustered())
				return;

			UnselectUnit();
			return;
		}

		SceneGlobals.SetState(SceneState.PreviewMode);
		_puddle.EnableNests(_model.GetAvailableMoves);
		AvailableCounted.SafeInvoke(_model.GetCounts);

		if (state == PreviewsState.HavePreviews)
			_view.MainSequence(_model.GetPreviews);
	}

	private void UnpickPreview()
		=> _pickedPreview = null;

	private void UnselectUnit()
	{
		UnitUnselected.SafeInvoke();
		DataCenter.Selection.UnselectCurrent();
	}
}
}