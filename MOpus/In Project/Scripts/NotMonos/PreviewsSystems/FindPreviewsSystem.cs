using System.Collections.Generic;
using System.Linq;
using NotMonos.Databases;
using NotMonos.Processors;

namespace NotMonos.Previews
{
internal abstract class FindPreviewsSystem : Processor
{
	protected const int MaxClusters = 6; //maximum amount of clusters from center = 6
	private readonly List<DirectionInfo> _directionsInfos = new(20);
	protected GridPoint[] availableMoves;

	internal byte PreviewsCount { get; set; }

	internal int DirectionsCount => _directionsInfos.Count;

	protected static GridPoint InitiatorPoint => Grid.GetPoint(InitiatorId);

	protected static UnitId InitiatorId => Selection.SelectedUnit;

	protected IEnumerable<GridPoint> AvailableAndSelf => availableMoves.Append(InitiatorPoint);

	internal void Clear() { _directionsInfos.Clear(); }

	internal static bool HaveAvailableMoves(out byte nestsCounts, out GridPoint[] availableMoves)
	{
		availableMoves = Grid.AllFreeDirectionsTo(InitiatorId).ToArray();
		nestsCounts = (byte)availableMoves.Length; //always in range from 0 to 3
		return availableMoves.Length > 0;
	}

	internal static bool InitiatorInCluster(out ClusterId clusterId, out ClusterType clusterType)
	{
		clusterType = default;
		if (!UnitInCluster(InitiatorId, out clusterId))
			return false;

		clusterType = Clusters.GetClusterType(clusterId);
		return true;
	}

	internal bool TryFindFormableClusters(GridPoint[] available, out IEnumerable<DirectionInfo> infos)
	{
		PreviewsCount = 0;
		availableMoves = available;
		GetInfos();
		infos = _directionsInfos;
		return _directionsInfos.Count > 0;
	}

	internal static bool TryGetClusterIdFromPoint(GridPoint position, out ClusterId clusterId, out ClusterType clusterType)
	{
		clusterId = null;
		clusterType = default;
		if (!Grid.TryGetUnitId(position, out UnitId unitId))
			return false;

		if (!UnitInCluster(unitId, out clusterId))
			return false;

		clusterType = Clusters.GetClusterType(clusterId);
		return true;
	}

	protected void AddToDirections(GridPoint direction, ClusterInfo clusterInfo)
	{
		_directionsInfos.Add(new(direction, clusterInfo));
		PreviewsCount++;
	}

	protected void AddToDirections(GridPoint direction, IEnumerable<ClusterInfo> clusterInfos)
	{
		_directionsInfos.Add(new(direction, clusterInfos)); //previews must be counted before!
	}

	protected static GridPoint FindAxis(GridPoint[] positions)
	{
		GridPoint footA, footB, perpendicular;
		if (positions[0].IsSameHeight(positions[1])){
			footA = positions[0];
			footB = positions[1];
			perpendicular = positions[2];
		}
		else if (positions[0].IsSameHeight(positions[2])){
			footA = positions[0];
			footB = positions[2];
			perpendicular = positions[1];
		}
		else{
			footA = positions[1];
			footB = positions[2];
			perpendicular = positions[0];
		}

		float x = MiddleOf2(footA.X, footB.X),
			  z = MiddleOf2(footA.Z, perpendicular.Z);
		return new(x, z);
	}

	protected abstract void GetInfos();

	private static float MiddleOf2(in float a, in float b)
		=> float.IsNegative(a - b)
			? a + Constants.Step
			: a - Constants.Step;

	private static bool UnitInCluster(UnitId unitId, out ClusterId clusterId)
		=> Units.TryGetClusterId(unitId, out clusterId);
}
}