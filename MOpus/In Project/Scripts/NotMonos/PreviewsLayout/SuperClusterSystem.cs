using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;
using NotMonos.Processors;
using static NotMonos.Databases.Constants;

namespace NotMonos.PreviewsLayout
{
	internal sealed class SuperClusterSystem
	{
		private readonly Dictionary<GridPoint, List<UnitId>> _declusters = new(20);
		private readonly List<DirectionInfo> _directions = new(20);
		private readonly List<(GridPoint direction, GridPoint center)> _potentionalCenters = new(10);

		private readonly PropertiesDB _properties = DataCenter.Properties;
		private readonly GridDB _grid = DataCenter.Grid;
		private readonly UnitsDB _units = DataCenter.Units;

		private GridPoint[] _availableMoves;
		private UnitId _initiatorId;
		private GridPoint _initiatorPoint;

		internal void ClearAll()
		{
			_directions.Clear();
			_potentionalCenters.Clear();
			_declusters.Clear();
		}

		internal bool TryFindFormableClusters(UnitId initiatorId,
					GridPoint[] availableMoves,
			out IEnumerable<DirectionInfo> infos)
		{
			_initiatorId = initiatorId;
			_availableMoves = availableMoves;

			//PeekLogger.LogItems(_availableMoves);
			_initiatorPoint = _grid.GetPoint(_initiatorId);
			var availableAndSelf = availableMoves.Append(_initiatorPoint);
			//PeekLogger.Log("availables+self. check WC", availableAndSelf);
			WorstCaseCheck(availableAndSelf);
			//PeekLogger.Log("availables check");
			AvailableMovesCheck();

			infos = _directions;
			return _directions.Count > 0;
		}

		internal bool TryGetDeclusters(out IEnumerable<(GridPoint moveDirection, UnitId[] unitsToDecluster)> declusters)
		{
			declusters = Enumerable.Empty<(GridPoint, UnitId[])>();
			//PeekLogger.LogName(_declusters.Count);
			if (_declusters.Count == 0)
				return false;

			declusters =
				from pair in _declusters
				select (pair.Key, pair.Value.ToArray());
			//PeekLogger.LogItems(declusters);
			return true;
		}

		private static GridPoint[] SortPositions(GridPoint[] unsortedPositions)
		//todo try to simplify
		{
			if (unsortedPositions.Length != 7)
			{
				PeekLogger.LogItems(unsortedPositions);
				throw new ArgumentException("Invalid SuperCluster positions", "unsortedPositions");
			}

			//last vector in a sequence always should be axis position, we dont need it
			List<GridPoint> list = unsortedPositions.SkipLast(1).ToList();

			var arr = new GridPoint[7];
			arr[0] = unsortedPositions[0];
			arr[6] = unsortedPositions[6];

			foreach (GridPoint v in list.Skip(1))
				if (v.X < arr[0].X)
					arr[0] = v;

			_ = list.Remove(arr[0]); //sort start in the leftest position
			float
				x_double_step = ClusterRight.X,
				z_step = ClusterForwardRight.Z,
				x_step = ClusterForwardRight.X,
				x_difference = arr[0].X + x_step,
				z_difference = arr[0].Z + z_step,
				z_minus_difference = arr[0].Z - z_step,
				second_x = float.NegativeInfinity;

			foreach (GridPoint v in list)
				if (GridPoint.AreFloatEquals(v.X, x_difference))
				{
					if (GridPoint.AreFloatEquals(v.Z, z_difference))
					{
						arr[1] = v;
						second_x = v.X;
					}
					else if (GridPoint.AreFloatEquals(v.Z, z_minus_difference))
						arr[5] = v;
				}

			if (second_x is float.NegativeInfinity)
				throw new InvalidOperationException("second vector does not setted in the right way");

			_ = list.Remove(arr[1]);
			_ = list.Remove(arr[5]);

			x_difference = second_x + x_double_step;

			foreach (GridPoint v in list)
				if (GridPoint.AreFloatEquals(v.X, x_difference))
				{
					if (GridPoint.AreFloatEquals(v.Z, arr[1].Z))
						arr[2] = v;
					else
						arr[4] = v;
				}

			_ = list.Remove(arr[2]);
			_ = list.Remove(arr[4]);

			if (list.Count != 1)
			{
				PeekLogger.LogItems(list);
				throw new InvalidOperationException("list must have only one item at this time");
			}

			arr[3] = list[0]; //rightest position - the middle in sort
			return arr;
		}

		private void AddUnitToDeclusters(GridPoint direction, GridPoint center)
		{
			_ = _grid.TryGetUnitId(center, out UnitId uid);
			AddUnitToDeclusters(direction, uid);
		}

		private void AddUnitToDeclusters(GridPoint direction, UnitId uid)
		{
			if (_declusters.TryGetValue(direction, out var declustersList))
			{
				declustersList.Add(uid);
				return;
			}
			_declusters.Add(direction, new(2) { uid });
		}

		private void AreaSClusterizable()
		{
			(GridPoint, GridPoint) variant;

			for (int i = _potentionalCenters.Count - 1; i >= 0; i--)
			{
				variant = _potentionalCenters[i];
				if (!CheckRemainsPositions(variant))
					_ = _potentionalCenters.Remove(variant);
			}
		}

		private void AvailableMovesCheck()
		{
			if (!HaveClustersAround()) //does available areas have clusters
				return;
			//PeekLogger.Log("areas full");
			if (!SuperClusterAreasFull()) //does whole areas have six prisms
				return;
			//PeekLogger.Log("clzbl?");
			AreaSClusterizable(); //does remained prisms is not parts of another cluster
		}

		private bool AxisIsNotFree_Worst(GridPoint direction)
		{
			//PeekLogger.Log($" {_initiatorPoint}");
			bool any = GetMoveDirectionPoints(direction).Any(point => !_grid.IsAxisFree(point));
			//PeekLogger.Log(any);
			return any;
		}

		private bool CheckDirection(GridPoint direction, out IEnumerable<GridPoint> verticesPoints)
		{
			verticesPoints = default;
			List<GridPoint> list = new(6);

			if (AxisIsNotFree_Worst(direction))
				return false;

			foreach (var clusterDirection in GetClusterPoints(direction))
			{
				/*var r = UnitIsUnavailable(clusterDirection);
				PeekLogger.LogItemsVarious(clusterDirection,r);
				if (r)*/
				if (UnitIsUnavailable(clusterDirection))
					return false;

				list.Add(clusterDirection);
			}
			if (list.Count != 6)
				throw new InvalidOperationException("SuperCluster is possible only with six Units on vertices");

			verticesPoints = list;
			return true;
		}

		private bool CheckPositionForPotentialCenter(GridPoint position)
				=> _grid.TryGetUnitId(position, out UnitId directionUnitId)
				&& _units.IsClustered(directionUnitId)
				&& _properties.InSameTeam(_initiatorId, directionUnitId);

		private bool CheckRemainsPositions(in (GridPoint direction, GridPoint center) variant)
		{
			UnitId uid;
			List<GridPoint> list = new(7);

			var col = GetClusterOtherVertices(variant.center).ToArray();
			if (col.Length == 0)
				throw new InvalidOperationException(); //todo add message

			list.AddRange(col); // side of cluster
			list.Add(variant.direction); //direction (where prism comes)

			//get remains vectors exclude previous and check them
			var remains = GetClusterPoints().ToList();
			foreach (GridPoint v in list)
				_ = remains.Remove(v - variant.center);

			foreach (GridPoint clusterVector in remains)
			{
				GridPoint other = variant.center + clusterVector;
				if (_grid.TryGetUnitId(other, out uid))
				{
					if (!_units.IsNotClustered(uid))
						return false;//must have only one cluster in area

					list.Add(other);//remains available prisms directions
				}
			}

			if (list.Count < 6)
				return false;

			list.Add(variant.center);//center of cluster/super cluster (s.c. axis)

			AddUnitToDeclusters(variant.direction, variant.center);
			//PeekLogger.LogItems(list);
			var sortedCluster = SortPositions(list.ToArray());
			//PeekLogger.LogItems(sortedCluster);
			//PeekLogger.Log($"cen {variant.center}");

			var cluster = new ClusterInfo(variant.center,
						ClusterType._7,
						sortedCluster);

			_directions.Add(new DirectionInfo(variant.direction, cluster));
			return true;
		}

		private void FindDeclusters(GridPoint direction, IEnumerable<GridPoint> superClusterPoints)
		{
			HashSet<ClusterId> usedClusters = new(6);
			foreach (var item in superClusterPoints)
			{
				//PeekLogger.LogName(item);
				if (!_grid.TryGetUnitId(item, out var unitId))
					continue;
				if (_units.IsNotClustered(unitId))
					continue;

				var cid = _units.GetClusterId(unitId);
				if (usedClusters.Contains(cid))
					continue;

				//PeekLogger.LogTab($"declusters add {unitId}");
				AddUnitToDeclusters(direction, unitId);
				_ = usedClusters.Add(cid);
			}
		}

		private IEnumerable<GridPoint> GetClusterOtherVertices(GridPoint here)
		{
			_ = _grid.TryGetUnitId(here, out UnitId id);
			var ids = _units.GetClusterNeighborsIds(id);
			foreach (var uid in ids)
				yield return _grid.GetPoint(uid);
		}

		private bool HaveClusterAround(GridPoint direction)
		{
			//PeekLogger.LogName(direction);
			var directionPotentialCenters
				= (from potentialCenter in GetClusterPoints(direction)
				   where CheckPositionForPotentialCenter(potentialCenter)
				   && IsAxisFree(potentialCenter)
				   select (direction, potentialCenter))
				  .ToArray();

			//PeekLogger.LogItems("have around", directionPotentialCenters);
			_potentionalCenters.AddRange(directionPotentialCenters);
			return directionPotentialCenters.Length > 0;
		}

		private bool HaveClustersAround()
			=> Array.Exists(_availableMoves, direction => HaveClusterAround(direction));

		private bool IsAxisFree(GridPoint direction)
		{
			//PeekLogger.LogName();
			//PeekLogger.LogParams(direction);
			var dirs = GetMoveDirectionPoints(direction).Except(_initiatorPoint);//initiator must be ignored, because its move
			/*foreach (var dir in dirs)
			{
				PeekLogger.LogMessage($"{dir} => {!_grid.IsFree(dir)}");
			}*/
			return dirs.Any(point => !_grid.IsFree(point));
		}

		private bool SuperClusterAreaFull(in (GridPoint direction, GridPoint center) variant)
		{
			//PeekLogger.LogName(variant);
			foreach (var checkThis in GetClusterPoints(variant.center))
			{
				//PeekLogger.Log($"check {checkThis}");
				if (checkThis == variant.direction)
					continue;

				if (UnitIsUnavailable(checkThis))
					return false;
			}

			return true;
		}

		private bool SuperClusterAreasFull()
		{
			bool finded = false;
			(GridPoint, GridPoint) variant;

			for (int i = _potentionalCenters.Count - 1; i >= 0; i--)
			{
				variant = _potentionalCenters[i];
				if (SuperClusterAreaFull(variant))
				{
					finded = true;
					continue;
				}
				_ = _potentionalCenters.Remove(variant);
			}

			return _potentionalCenters.Count > 0 && finded;
		}

		private bool UnitIsUnavailable(GridPoint direction)
				=> !ConnectionProcessor.DirectionHaveUnitInSameTeam(_initiatorId, direction, out _);

		private void WorstCaseCheck(IEnumerable<GridPoint> directions)
		{
			foreach (var direction in directions)
				if (CheckDirection(direction, out var verticesPoints))
				{
					//PeekLogger.LogItems(verticesPoints);
					GridPoint[] superClusterPoints = verticesPoints.Append(direction).ToArray();
					GridPoint[] sortedCluster = SortPositions(superClusterPoints);
					var cluster = new ClusterInfo(
						direction,
						ClusterType._7,
						sortedCluster);

					_directions.Add(new DirectionInfo(direction, cluster));
					FindDeclusters(direction, superClusterPoints);
				}
		}
	}
}