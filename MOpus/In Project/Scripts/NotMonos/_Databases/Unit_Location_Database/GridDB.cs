using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using static NotMonos.Databases.Constants;

namespace NotMonos.Databases
{
#nullable enable

	internal sealed class GridDB
	{
		private readonly Dictionary<GridPoint, UnitId> _grid = new();
		private readonly Dictionary<GridPoint, TeamId> _powerSources = new(2);

		internal IEnumerable<(TeamId id, GridPoint point)> GetAllPowerSources
			=> from src in _powerSources
			   select (src.Value, src.Key);

		private UnitId this[GridPoint point]
		{
			get
			{
				if (!IsPointPresent(point))
					GridNotHavePointException(point);

				return _grid[point];
			}
			set
			{
				if (IsPointPresent(point))
					GridHavePointException(point);

				_grid[point] = value;
			}
		}

		public bool TryGetUnitId(GridPoint point, out UnitId unitId)
			=> TryGetPointUnit(point, out unitId) && !unitId.IsZero && !unitId.IsPowerSource;

		public Connectability TryGetUnitIdForConnect(GridPoint point, out UnitId unitId, TeamId unitTeam)
		{
			if (!TryGetPointUnit(point, out unitId))
				return Connectability.Unconnectable;

			if (unitId.IsZero)
				return Connectability.Unconnectable;

			if (!unitId.IsPowerSource)
				return Connectability.Connectable;

			return _powerSources[point].Equals(unitTeam)
				? Connectability.PowerSourceConnection
				: Connectability.Unconnectable;
		}

		internal void AddOnGrid(UnitId unitId, in float x, in float z)
			=> AddEntity(new GridPoint(x, z), unitId);

		internal void AddOnGrid(UnitId unitId, GridPoint position)
			=> AddEntity(position, unitId);

		internal void AddPlaceholder(GridPoint point)
		{
			if (!IsPointPresent(point))
				AddEntity(point, UnitId.Zero);
		}

		internal void AddPlaceholders(GridPoint axis)
		{
			foreach (var item in GetMoveDirectionPoints(axis))
				AddPlaceholder(item);
		}

		internal void AddPowerSource(TeamId teamId, GridPoint point)
		{
			AddEntity(point, UnitId.PowerSource);
			_powerSources.Add(point, teamId);
		}

		internal void AddUnitToGrid(GridPoint point, UnitId unitId)
			=> AddEntity(point, unitId);

		internal IEnumerable<GridPoint> AllFreeDirectionsTo(UnitId unitId)
			=> from dir in GetMoveDirectionPoints(GetPoint(unitId))
			   where !IsPointPresent(dir)
			   select dir;

		internal IEnumerable<GridPoint> AllSpawnableDirectionsTo(TeamId teamId)
			=> from pair in _powerSources
			   where !IsPointPresent(pair.Key) && pair.Value.Equals(teamId)
			   select pair.Key;

		internal void Clear()
		{
			_grid.Clear();
			_powerSources.Clear();
		}

		internal IEnumerable<UnitId> GetIdsOnPoints(GridPoint[] positions)
		{
			foreach (var item in positions)
				yield return this[item];
		}

		internal GridPoint GetPoint(UnitId unitId)
			=> unitId == UnitId.Zero
				? throw new ArgumentException("Zero-Id may be in multiple positions", "unitId")
				: _grid.Single(x => x.Value.Equals(unitId)).Key;

		internal bool IsAxisFree(GridPoint point)
			=> !TryGetPointUnit(point, out UnitId unitId) || unitId.IsZero;

		internal bool IsFree(GridPoint axisPoint)
			=> !IsPointPresent(axisPoint);

		internal bool IsFreeOrSelf(GridPoint axisPoint, UnitId unitId)
			=> IsFree(axisPoint) || _grid[axisPoint].Equals(unitId);

		internal bool IsReachable(UnitId selectedUnit, UnitId pickedUnit)
		{
			GridPoint selectedPoint = GetPoint(selectedUnit),
					pickedPoint = GetPoint(pickedUnit);
			bool res = GetMoveDirectionPoints(selectedPoint).Any(x => pickedPoint.Equals(x));

			//PeekLogger.Log($"is {pickedUnit} reachable for {selectedUnit}? - {res}");
			return res;
		}

		internal bool MoveUnitTo(UnitId unitId, GridPoint newPoint)
		{
			GridPoint oldPoint = GetPoint(unitId);
			PeekLogger.LogTab($"Moving from {oldPoint} to {newPoint}");
			if (newPoint.Equals(oldPoint))
				return false;

			AddEntity(newPoint, unitId);
			RemoveEntity(oldPoint);
			return true;
		}

		internal void RemovePlaceholder(GridPoint point)
			=> RemoveEntity(point);

		internal void RemovePlaceholders(GridPoint axis)
		{
			foreach (var item in GetMoveDirectionPoints(axis))
				RemovePlaceholder(item);
		}

		internal void RemoveUnit(UnitId unitId)
			=> _grid.Remove(
				_grid.First(x => x.Value == unitId)
				.Key);

		private static void GridHavePointException(GridPoint point)
			=> throw new ArgumentException($"Grid already have this point ({point})", "point");

		private static void GridNotHavePointException(GridPoint point)
			=> throw new ArgumentException($"Grid do not have this point ({point})", "point");

		private void AddEntity(GridPoint point, UnitId unitId)
			=> this[point] = unitId;

		private bool IsPointPresent(GridPoint point)
			=> _grid.Keys.Contains(point);

		private void RemoveEntity(GridPoint point)
			=> _grid.Remove(point);

		private bool TryGetPointUnit(GridPoint point, out UnitId unitId)
			=> _grid.TryGetValue(point, out unitId);
	}
}