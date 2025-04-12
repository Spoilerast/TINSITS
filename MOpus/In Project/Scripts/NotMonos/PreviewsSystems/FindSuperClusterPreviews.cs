using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;
using static NotMonos.Databases.Constants;

namespace NotMonos.Previews
{
internal sealed class FindSuperClusterPreviews : FindPreviewsSystem
{
	private readonly List<(GridPoint direction, GridPoint center)> _potentialCenters = new(10);

	internal static bool TrySplitSuperCluster(ClusterId initiatorClusterId, out DirectionInfo positions)
	{
		IEnumerable<GridPoint> except = Grid.GetPoint(Selection.SelectedUnit).AsIEnumerable();
		return TrySplitSuperCluster(initiatorClusterId, except, out positions);
	}

	internal static bool TrySplitSuperCluster(ClusterId clusterId, IEnumerable<GridPoint> positions, out DirectionInfo directionInfo)
	{
		directionInfo = null;
		if (Clusters.GetClusterType(clusterId) != ClusterType._7)
			return false;

		GridPoint axisPoint = Clusters.ClusterAxis(clusterId);
		if (!TrySplitSuperCluster(axisPoint, positions, out IEnumerable<ClusterInfo> infos))
			return false;

		directionInfo = new(axisPoint, infos);
		return true;
	}

	protected override void GetInfos()
	{
		//PeekLogger.Log("availables+self. check WC", availableAndSelf);
		WorstCaseCheck();
		//PeekLogger.Log("availables check");
		AvailableMovesCheck();
	}

	private void AreaSClusterizable()
	{
		(GridPoint direction, GridPoint center) variant;

		for (int i = _potentialCenters.Count - 1; i >= 0; i--){
			variant = _potentialCenters[i];
			if (IsPotentialCenterValid(variant, out ClusterInfo cluster))
				AddToDirections(variant.direction, cluster);
			else //todo is reasonable?
				_ = _potentialCenters.Remove(variant);
		}
	}

	private void AvailableMovesCheck()
	{
		PeekLogger.LogName();
		if (!HaveClustersAround()) //does available areas have clusters
			return;
		DirectionsCount.LogTabThis();
		PeekLogger.Log("areas full");
		if (!SuperClusterAreasFull()) //does whole areas have six prisms
			return;
		DirectionsCount.LogTabThis();

		PeekLogger.Log("clzbl?");
		AreaSClusterizable(); //do remained prisms is not parts of another cluster
		DirectionsCount.LogTabThis();
	}

	private static bool AxisIsNotFree_Worst(GridPoint direction)
		=> GetMoveDirectionPoints(direction).Any(point => !Grid.IsAxisFree(point));

	private static bool CheckDirection(GridPoint direction, out GridPoint[] sortedPoints)
	{
		PeekLogger.LogName(direction);
		sortedPoints = null;
		if (AxisIsNotFree_Worst(direction))
			return false;

		sortedPoints = GetSortedClusterPoints(direction).ToArray();
		//PeekLogger.LogItems(sortedPoints);
		return sortedPoints.All(IsAvailableUnit);
	}

	private static bool CheckPositionForPotentialCenter(GridPoint position)
		=> Grid.TryGetUnitId(position, out UnitId directionUnitId)
		   && Units.IsClustered(directionUnitId)
		   && Properties.InSameTeam(InitiatorId, directionUnitId);

	private static IEnumerable<GridPoint> GetClusterOtherVertices(GridPoint here)
	{
		if (!Grid.TryGetUnitId(here, out UnitId id))
			yield break;

		IEnumerable<UnitId> ids = Units.GetClusterNeighborsIds(id);
		foreach (UnitId uid in ids)
			yield return Grid.GetPoint(uid);
	}

	private bool HaveClusterAround(GridPoint direction)
	{
		PeekLogger.LogName(direction);
		(GridPoint, GridPoint)[] directionPotentialCenters
			= (
				from potentialCenter in GetClusterPoints(direction)
				where CheckPositionForPotentialCenter(potentialCenter)
					  && IsAxisFree(potentialCenter)
				select (direction, potentialCenter))
			.ToArray();

		//PeekLogger.LogItems("<size=15>potential SCs direction and center:</size>", directionPotentialCenters);
		_potentialCenters.AddRange(directionPotentialCenters);
		return directionPotentialCenters.Length > 0;
	}

	private bool HaveClustersAround()
		=> Array.Exists(availableMoves, HaveClusterAround);

	private static bool IsAvailableUnit(GridPoint direction)
		=> Grid.TryGetUnitId(direction, out UnitId neighborId)
		   && Properties.InSameTeam(InitiatorId, neighborId);

	private static bool IsAxisFree(GridPoint direction)
		=> GetMoveDirectionPoints(direction).AllExcept(Grid.IsFree, InitiatorPoint);

	private static bool IsNotClustered(GridPoint direction)
		=> Grid.TryGetUnitId(direction, out UnitId uid)
		   && Units.IsNotClustered(uid);

	private static bool IsPotentialCenterValid(in (GridPoint direction, GridPoint center) variant,
											   out ClusterInfo cluster)
	{
		GridPoint[] potentialClusterPoints = GetSortedClusterPoints(variant.center).ToArray();
		PeekLogger.LogItems("potential", potentialClusterPoints);
		IEnumerable<GridPoint> exceptPoints = GetClusterOtherVertices(variant.center)
											  .Append(variant.direction)
											  .Append(variant.center);
		IEnumerable<GridPoint> remained = potentialClusterPoints.Except(exceptPoints);
		//PeekLogger.LogItems("remained", remained);

		bool isValid = remained.All(IsNotClustered);
		if (isValid){
			cluster = new(variant.center,
						  ClusterType._7,
						  potentialClusterPoints);
			return true;
		}

		cluster = null;
		return false;
	}

	private static void MakeClusters(GridPoint axisPoint, GridPoint[] excepts, out IEnumerable<ClusterInfo> infos)
	{
		List<ClusterInfo> clusterInfos = new(MaxClusters);
		GridPoint[] possibles = GetClusterPoints(axisPoint).ToArray();
		GridPoint direction1, direction2;
		int end = possibles.Length;
		for (var i = 0; i < end; i++){
			direction1 = possibles[i];
			direction2 = possibles[(i + 1) % end];

			if (excepts.Contains(direction1))
				continue;
			if (excepts.Contains(direction2)){
				i++; //attention, this is skips next loop
				continue;
			}

			GridPoint[] clusterPositions = {
				axisPoint,
				direction1,
				direction2
			};
			clusterInfos.Add(new(FindAxis(clusterPositions),
								 ClusterType._3,
								 clusterPositions));
		}

		infos = clusterInfos;
	}

	private static bool SuperClusterAreaFull(in (GridPoint direction, GridPoint center) variant)
		=> GetClusterPoints(variant.center).AllExcept(IsAvailableUnit, variant.direction);

	private bool SuperClusterAreasFull()
	{
		var finded = false;
		(GridPoint, GridPoint) variant;

		for (int i = _potentialCenters.Count - 1; i >= 0; i--){
			variant = _potentialCenters[i];
			if (SuperClusterAreaFull(variant)){
				finded = true;
				continue;
			}

			_ = _potentialCenters.Remove(variant);
		}

		return _potentialCenters.Count > 0 && finded;
	}

	private static bool TrySplitSuperCluster(GridPoint axisPoint, IEnumerable<GridPoint> positions, out IEnumerable<ClusterInfo> infos)
	{
		PeekLogger.LogName();
		infos = Enumerable.Empty<ClusterInfo>();
		GridPoint[] excepts = positions.CastToArray();
		if (excepts.Contains(axisPoint))
			return false;

		MakeClusters(axisPoint, excepts, out infos);
		PeekLogger.LogItems("reclusters", infos);
		return true;
	}

	private void WorstCaseCheck()
	{
		PeekLogger.LogName();
		foreach (GridPoint direction in AvailableAndSelf){
			if (!CheckDirection(direction, out GridPoint[] sortedCluster))
				continue;
			PeekLogger.LogItems(sortedCluster);
			var cluster = new ClusterInfo(direction,
										  ClusterType._7,
										  sortedCluster);
			AddToDirections(direction, cluster);
		}
	}
}
}