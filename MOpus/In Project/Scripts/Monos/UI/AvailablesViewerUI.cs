using System.Collections.Generic;
using Extensions;
using NotMonos;
using NotMonos.Previews;
using UnityEngine;
using UnityEngine.UIElements;
using Elements = NotMonos.UI.Enums.AvailablesViewer;

namespace Monos.Systems
{
[RequireComponent(typeof(UIDocument))]
internal class AvailablesViewerUI : SceneUI
{
	private const string SelectedClassName = "Group_selected";

	private GroupBox _clusters;
	private Label _clustersCount;
	private Dictionary<ClusterStatus, GroupBox> _groups;
	private GroupBox _moves;
	private Label _movesCount;
	private PreviewsController _previewsController;
	private GroupBox _superClusters;
	private Label _superClustersCount;

	internal void Initialize(PreviewsController previewsController)
	{
		_previewsController = previewsController;

		UIDocument doc = this.GetUIDocument();
		doc.FindVisualElement(Elements.GroupMoves, out _moves);
		doc.FindVisualElement(Elements.GroupClusters, out _clusters);
		doc.FindVisualElement(Elements.GroupSuperClusters, out _superClusters);

		_moves.FindFirstVisualElement(out _movesCount);
		_clusters.FindFirstVisualElement(out _clustersCount);
		_superClusters.FindFirstVisualElement(out _superClustersCount);

		_previewsController.AvailableCounted += OnAvailablesCounted;
		_previewsController.PreviewsScrolledEvent += PreviewsScrolled;
		_previewsController.UnitUnselected += HideAll;

		_groups = new(3) {
			{ ClusterStatus.NotClustered, _moves },
			{ ClusterStatus.Clustered, _clusters },
			{ ClusterStatus.SuperClustered, _superClusters }
		};
	}

	private void HideAll()
	{
		HideBox(ClusterStatus.NotClustered);
		HideBox(ClusterStatus.Clustered);
		HideBox(ClusterStatus.SuperClustered);
	}

	private void HideBox(ClusterStatus status)
		=> _groups[status].HideElement();

	private void OnAvailablesCounted((byte moves, byte clusters, byte superClusters) counts)
	{
		if (counts.moves > 0){
			SetLabelCount(_movesCount, counts.moves);
			ShowBox(ClusterStatus.NotClustered);
		}

		if (counts.clusters > 0){
			SetLabelCount(_clustersCount, counts.clusters);
			ShowBox(ClusterStatus.Clustered);
		}

		if (counts.superClusters > 0){
			SetLabelCount(_superClustersCount, counts.superClusters);
			ShowBox(ClusterStatus.SuperClustered);
		}
	}

	private void PreviewsScrolled(ClusterStatus status)
	{
		SelectBox(status);
		foreach (ClusterStatus key in _groups.Keys)
			if (key != status)
				UnselectBox(key);
	}

	private void SelectBox(ClusterStatus status)
		=> _groups[status].AddToClassList(SelectedClassName);

	private static void SetLabelCount(Label label, byte i)
		=> label.text = i.ToString();

	private void ShowBox(ClusterStatus status)
		=> _groups[status].ShowElement();

	private void UnselectBox(ClusterStatus status)
		=> _groups[status].RemoveFromClassList(SelectedClassName);
}
}