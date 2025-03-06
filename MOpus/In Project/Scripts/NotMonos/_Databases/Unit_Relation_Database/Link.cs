namespace NotMonos.Databases
{
	internal sealed partial class ConnectionsDB
	{
		private class Link
		{
			private readonly GridPoint _pointA, _pointB;
			private readonly UnitId _unitA, _unitB;

			public Link(UnitId initiatorId, GridPoint position, UnitId neighborId, GridPoint direction)
				=> (_unitA, _pointA, _unitB, _pointB) = (initiatorId, position, neighborId, direction);

			public override string ToString()
				=> $"LINK [{_unitA}, {_unitB}] {_pointA}--{_pointB}";

			internal bool ContainsUnit(UnitId unitId)
				=> _unitA == unitId || _unitB == unitId;

			internal UnitId GetNeighborOf(UnitId initiatorId)
				=> _unitA == initiatorId ? _unitB : _unitA;

			internal (GridPoint, GridPoint) GetPoints()
				=> (_pointA, _pointB);

			internal (UnitId, UnitId) GetUnits()
				=> (_unitA, _unitB);

			internal bool IsConnectionEqual(GridPoint pointA, GridPoint pointB)
				=> (pointA.Equals(_pointA) & pointB.Equals(_pointB))
				|| (pointA.Equals(_pointB) & pointB.Equals(_pointA));

			internal bool IsConnectionEqual(UnitId unitA, UnitId unitB)
				=> (unitA.Equals(_unitA) & unitB.Equals(_unitB))
				|| (unitA.Equals(_unitB) & unitB.Equals(_unitA));

			internal bool TryGetNeighborOf(UnitId initiatorId, out UnitId neighborId)
			{
				neighborId = null;
				if (!ContainsUnit(initiatorId))
					return false;

				neighborId = GetNeighborOf(initiatorId);
				return true;
			}
		}
	}
}