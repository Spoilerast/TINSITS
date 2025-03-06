using System.Collections.Generic;
using System.Linq;
using Monos.Scene;
using NotMonos.Entities;

namespace NotMonos.Databases
{
	internal sealed class UnitsDB
	{
		private readonly Dictionary<UnitId, Unit> _units = new();

		internal IEnumerable<UnitId> AllUnitIds
			=> _units.Keys;

		internal int Count
			=> _units.Count;

		internal void AddUnit(UnitId unitId, Prism prism)
		{
			Unit unit = new(prism);
			_units.Add(unitId, unit);
			prism.name = unitId.ToString();//remove?
		}

		internal void Clear()
			=> _units.Clear();

		internal void ClusterizeUnit(UnitId unitId, ClusterId clusterId, ClusterStatus status)
			=> _units[unitId].Clusterize(clusterId, status);

		internal void DestroyUnit(UnitId unitId)
		{
			Prism prism = _units[unitId].Prism;
			prism.DestroySceneObject();
			_ = _units.Remove(unitId);
		}

		internal ClusterId GetClusterId(UnitId unitId)
			=> _units[unitId].ClusterId;

		internal IEnumerable<UnitId> GetClusterNeighborsIds(UnitId id)
		{
			ClusterId clusterId = _units[id].ClusterId;
			return
				from pair in _units
				where pair.Key != id && pair.Value.ClusterId == clusterId
				select pair.Key;
		}

		internal Unit GetUnit(UnitId unitId)
			=> _units[unitId];

		internal IEnumerable<UnitId> GetUnitsInCluster(ClusterId clusterId)
			=> from pair in _units
			   where pair.Value.ClusterId == clusterId
			   select pair.Key;

		internal bool IsClustered(UnitId unitId)
			=> _units[unitId].Status is ClusterStatus.Clustered;

		internal bool IsNotClustered(UnitId unitId)
			=> _units[unitId].Status is ClusterStatus.NotClustered;

		internal bool IsSuperClustered(UnitId unitId)
			=> _units[unitId].Status is ClusterStatus.SuperClustered;

		internal void MoveUnitTo(UnitId initiator, GridPoint newPosition)
			=> _units[initiator].Prism.Move(newPosition);

		internal UnitId[] RemoveAllUnitsFromCluster(ClusterId clusterId)
		{
			var clusterUnits = GetUnitsInCluster(clusterId).ToArray();
			foreach (var item in clusterUnits)
			{
				_units[item].Declusterize();
				//_units[item].Prism.Declusterize();//todo
			}
			return clusterUnits;
		}

		internal bool TryGetClusterId(UnitId unitId, out ClusterId clusterId)
		{
			clusterId = GetClusterId(unitId);
			return !clusterId.IsNotInCluster;
		}
	}
}