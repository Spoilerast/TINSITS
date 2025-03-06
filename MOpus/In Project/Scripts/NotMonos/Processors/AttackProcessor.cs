using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;

namespace NotMonos.Processors
{
	internal sealed class AttackProcessor : Processor
	{
		private AttackProcessor()
		{ }

		internal static void GenerateDamage(UnitId attackerId, UnitId victimId)
		{
			float attackPower = GetAttackerPower(attackerId);
			if (GridPoint.AreFloatEquals(0, attackPower))
			{
				PeekLogger.Log("attack power is zero. early return");
				return; //todo: check charge before attack move or something kind of
			}
			DoDamage(attackPower, victimId);
			SceneGlobals.SetState(SceneState.Default);

			//todo end of turn
		}

		private static float GetAttackerPower(UnitId attackerId)
		{
			if (Units.IsNotClustered(attackerId))
			{
				return Connections.TryGetNeighborsTo(attackerId, out UnitId[] neighbors)
					? AttackPowerMultiple(attackerId, neighbors)
					: AttackPowerSingle(attackerId);
			}
			return AttackPowerCluster(attackerId);
		}

		private static float AttackPowerMultiple(UnitId attackerId, UnitId[] neighbors)
		{
			IEnumerable<UnitId> units = neighbors.Append(attackerId);
			float power = Properties.GetAttackAmount(units);
			PeekLogger.Log($"multiple atck pwr {power}");
			units.ForEach(Properties.ClearCharge);
			return power;
		}

		private static float AttackPowerCluster(UnitId attackerId)//todo extract pattern
		{
			IEnumerable<UnitId> units = Units.GetClusterNeighborsIds(attackerId).Append(attackerId);
			float power = Properties.GetAttackAmount(units);
			PeekLogger.Log($"cluster atck pwr {power}");
			units.ForEach(Properties.ClearCharge);
			return power;
		}

		private static float AttackPowerSingle(UnitId attackerId)
		{
			float power = Properties.GetAttackAmount(attackerId);
			PeekLogger.Log($"single atck pwr {power}");
			Properties.ClearCharge(attackerId);
			return power;
		}

		private static void DoDamage(in float attackPower, UnitId victimId)
		{
			if (Units.IsNotClustered(victimId))
			{
				if (Connections.TryGetNeighborsTo(victimId, out UnitId[] neighbors))
					ReceiveAttackMultiple(victimId, attackPower, ref neighbors);
				else
					ReceiveAttackSingle(victimId, attackPower);
				return;
			}
			ReceiveAttackCluster(victimId, attackPower);
		}

		private static void ReceiveAttackSingle(UnitId victimId, in float attackPower)
			=> Properties.DamageUnit(victimId, attackPower, out _);

		private static void ReceiveAttackMultiple(UnitId victimId, in float attackPower, ref UnitId[] neighbors)
			=> Properties.DamageUnitAndNeighbors(victimId, attackPower, ref neighbors);

		private static void ReceiveAttackCluster(UnitId victimId, float attackPower)
		{
			IEnumerable<UnitId> clusterUnits = Units.GetClusterNeighborsIds(victimId).Append(victimId);
			Properties.DamageCluster(clusterUnits, attackPower);
		}
	}
}