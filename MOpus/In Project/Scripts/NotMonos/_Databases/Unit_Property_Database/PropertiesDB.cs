using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Processors;

namespace NotMonos.Databases
{
	internal sealed class PropertiesDB
	{
		private readonly Dictionary<UnitId, PrismProperties> _properties = new();

		private UnitId _currentId;

		internal IEnumerable<UnitId> AllSpawnable
			=> from pair in _properties
			   where pair.Value.Type == PrismType._L
			   select pair.Key;

		private float Capacity
			=> _properties[_currentId].Capacity;

		private float Charge
			=> _properties[_currentId].Charge;

		private float Integrity
			=> _properties[_currentId].Integrity;

		private float Resistance
			=> _properties[_currentId].Resistance;

		private TeamId Team
			=> _properties[_currentId].Team;

		private PrismType Type
			=> _properties[_currentId].Type;

		internal void Add(UnitId id)
			=> _properties.Add(id, new());

		internal void Add(UnitId id, PrismProperties properties)
			=> _properties.Add(id, properties);

		internal void Clear()
			=> _properties.Clear();

		internal void ClearCharge(UnitId id) //maybe only SetCharge?
			=> _properties[id].Charge = 0;

		private void SetCurrentCharge(in float v)
			=> SetCharge(_currentId, v);

		private void SetIntegrity(in float value)
			=> _properties[_currentId].Integrity = value;

		internal void DamageCluster(IEnumerable<UnitId> clusterUnits, in float attackPower)
		{
			UnitId[] units = clusterUnits.ToArray();
			float partDamage = attackPower / units.Length;
			_currentId = units[0];
			float defence = GetDefenceAmount();
			foreach (var unit in units)
			{
				_currentId = unit;
				_ = DamageCurrentUnit(ref partDamage, ref defence, out _);
			}
		}

		internal bool DamageUnit(UnitId victimId, float attackPower, out float remainedPower)
		{
			_currentId = victimId;
			float defence = GetDefenceAmount();
			return DamageCurrentUnit(ref attackPower, ref defence, out remainedPower);
		}

		internal void DamageUnitAndNeighbors(UnitId victimId, in float attackPower, ref UnitId[] neighbors)
		{
			bool isHaveCollateralDamage = DamageUnit(victimId, attackPower, out float collateralDamage);
			PeekLogger.Log($"attack was {attackPower} collat dmg {collateralDamage} neis count {neighbors.Length}");
			if (!isHaveCollateralDamage)
				return;

			float partDamage = collateralDamage / neighbors.Length;
			partDamage.LogThis();
			foreach (var neighbor in neighbors)//todo maybe recursive damage chain for neighbors of neighbors
			{
				_ = DamageUnit(neighbor, partDamage, out _);//todo what if neighbor is clusterpart?
			}
		}

		internal float GetAttackAmount(UnitId id)
		{
			_currentId = id;
			float attack = Charge;
			PeekLogger.Log($"unit original ch {attack}");
			attack = Type switch
			{
				PrismType._R => .1f,
				PrismType._L => attack * 3,
				PrismType._C => attack,
				_ => throw new ArgumentException("Prism must have type")
			};
			return attack;
		}

		internal float GetAttackAmount(IEnumerable<UnitId> units)
			=> units.Sum(x => GetAttackAmount(x));

		internal float GetCharge(UnitId id)
			=> _properties[id].Charge;

		internal PrismType GetPrismType(UnitId id)
			=> _properties[id].Type;

		internal (TeamId, PrismType, float, float, float, float) GetProperties(UnitId id)
		{
			var (tid, type, i, ca, cha, re) = _properties[id];
			return (tid, type, i, ca, cha, re);
		}

		internal TeamId GetTeam(UnitId id)
			=> _properties[id].Team;

		internal bool InSameTeam(UnitId initiatorId, UnitId directionUnitId)
			=> _properties[initiatorId].Team == _properties[directionUnitId].Team;

		internal bool IsInCurrentPlayerTeam(UnitId id)
			=> SceneGlobals.CurrentTeam.Equals(_properties[id].Team);

		internal bool IsLType(UnitId id)
			=> _properties[id].Type is PrismType._L;

		internal void Remove(UnitId id) => _properties.Remove(id);

		internal void SetCharge(UnitId id, in float value)
			=> _properties[id].Charge = value;

		internal void SetPrismsType(IEnumerable<UnitId> clusterUnitIds, PrismType prismType)
		{
			foreach (var unitId in clusterUnitIds)
				_properties[unitId].Type = prismType;
		}

		internal PrismType SetPrismType(UnitId id, PrismType prismType)
			=> _properties[id].Type = prismType;

		private bool AttackRemainedAfterDefence(in float attackPower, in float defence, out float remainedPower)
		{
			remainedPower = 0;
			if (attackPower < defence)
			{
				SetCurrentCharge((Charge * attackPower) / defence);
				/*			D = (A⋅C)÷B
				 where A - current charge
					   B - defence power based on current charge
					   C - incoming attack power
					   D - remained charge after defence
				*/
				return false;
			}

			ClearCurrentCharge();
			remainedPower = attackPower - defence;
			return true;
		}

		private void ClearCurrentCharge()
			=> ClearCharge(_currentId);

		//internal event Action<UnitId> DestroyUnit;
		private bool DamageCurrentUnit(ref float attackPower, ref float defence, out float remainedPower)
		{
			PeekLogger.Log($"atck pwr after res {attackPower} - {defence} = {attackPower - defence}");
			if (!AttackRemainedAfterDefence(attackPower, defence, out remainedPower))
				return false;

			float health = Integrity;
			float remainedHealth = health - remainedPower;
			PeekLogger.Log($"health after atck pwr {health} - {remainedPower} = {remainedHealth}");
			if (remainedHealth > 0)
			{
				SetIntegrity(remainedHealth);
				PeekLogger.Log($"setted {_currentId} integr to {remainedHealth}");
			}
			else
			{
				PeekLogger.Log($"destroy {_currentId}");
				//DestroyUnit.SafeInvoke(_currentId);
				UnitDestroyProcessor.Destroy(_currentId);
			}

			PeekLogger.Log($"remained power {remainedPower} - {health}");
			remainedPower -= health;
			PeekLogger.Log($"remained power {remainedPower} {remainedPower >= 0.01f}");
			return remainedPower >= 0.01f; //todo make chain damage longer than 2 units
		}

		private float GetDefenceAmount()
		{
			float defence = Resistance;
			PeekLogger.Log($"unit original res {defence}");
			defence = Type switch
			{
				PrismType._R => defence * 2.9f,
				PrismType._L => defence,
				PrismType._C => .1f,
				_ => throw new ArgumentException("Prism must have type")
			};
			PeekLogger.Log($"unit res by type {defence}");

			float charge = Charge;
			defence = charge > 0
				? defence * charge
				: .1f;

			PeekLogger.Log($"unit res by charge {defence}");
			return defence;
		}
	}
}