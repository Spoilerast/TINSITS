using System.Collections.Generic;
using NotMonos.Databases;

namespace NotMonos.Processors
{
	internal sealed partial class ConnectionProcessor : Processor
	{
		private static readonly ConnectionsProducer _producer = new(Units, Clusters); //todo: is this a singleton?

		private ConnectionProcessor()
		{ }

		internal static bool DirectionHaveUnitInSameTeam(UnitId unitId, GridPoint direction, out UnitId neighborId)
			=> Grid.TryGetUnitId(direction, out neighborId)
			&& Properties.InSameTeam(unitId, neighborId);

		internal static bool DirectionHaveUnitOrSourceInSameTeam(UnitId unitId, GridPoint direction, out UnitId neighborId)
		{
			Connectability ability = CheckConnectability(unitId, direction, out neighborId);
			return ability is not Connectability.Unconnectable
				&& (ability is Connectability.PowerSourceConnection
					|| Properties.InSameTeam(unitId, neighborId));
		}

		/*
				internal static bool DirectionValidForLongConnection(UnitId unitId, GridPoint direction, out UnitId neighborId)
					=> DirectionHaveUnitInSameTeam(unitId, direction, out neighborId)
					&& Properties.IsLType(neighborId)
					&& !Units.IsNotClustered(neighborId);*/

		internal static Connectability DirectionValidForLongConnection(UnitId unitId, GridPoint direction, out UnitId neighborId)
		{
			Connectability ability = CheckConnectability(unitId, direction, out neighborId);

			if (!Properties.IsLType(unitId))
				return Connectability.Unconnectable;

			if (ability is not Connectability.Connectable)
				return ability;

			return Properties.InSameTeam(unitId, neighborId)
					&& Properties.IsLType(neighborId)
					&& !Units.IsNotClustered(neighborId)
				? Connectability.Connectable
				: Connectability.Unconnectable;
		}

		internal static Connectability DirectionValidForLongestConnection(UnitId unitId, GridPoint direction, out UnitId neighborId)
		{
			Connectability ability = CheckConnectability(unitId, direction, out neighborId);

			if (!Properties.IsLType(unitId))
				return Connectability.Unconnectable;

			if (ability is not Connectability.Connectable)
				return ability;

			return Properties.InSameTeam(unitId, neighborId)
					&& Properties.IsLType(neighborId)
					&& Units.IsSuperClustered(neighborId)
				? Connectability.Connectable
				: Connectability.Unconnectable;
		}

		private static Connectability CheckConnectability(UnitId unitId, GridPoint direction, out UnitId neighborId)
		{
			TeamId unitTeam = Properties.GetTeam(unitId);
			return Grid.TryGetUnitIdForConnect(direction, out neighborId, unitTeam);
		}

		/*
internal static bool DirectionValidForLongestConnection(UnitId unitId, GridPoint direction, out UnitId neighborId)
	=> DirectionHaveUnitInSameTeam(unitId, direction, out neighborId)
	&& Properties.IsLType(neighborId)
	&& Units.IsSuperClustered(neighborId);
*/

		internal static IEnumerable<(UnitId, GridPoint, UnitId, GridPoint)> GetAllClusterConnections(UnitId unitId)
			=> _producer.MakeClusterConnectionsFor(unitId);

		internal static IEnumerable<(UnitId, GridPoint, UnitId, GridPoint)> GetAllSuperClusterConnections(UnitId unitId)
			=> _producer.MakeSuperClusterConnectionsFor(unitId);

		internal static bool GetSingleConnections(UnitId unitId,
											out GridPoint unitPosition,
											out IEnumerable<(UnitId, GridPoint)> connections)
		{
			List<(UnitId, GridPoint)> available = new(3);
			unitPosition = Grid.GetPoint(unitId);
			foreach (var direction in Constants.GetMoveDirectionPoints(unitPosition))
			{
				if (DirectionHaveUnitOrSourceInSameTeam(unitId, direction, out UnitId neighborId))
				{
					available.Add((neighborId, direction));
				}
			}
			connections = available;
			return available.Count > 0;
		}

		/*
		internal static IEnumerable<(UnitId, GridPoint)> GetSingleConnections(UnitId unitId, out GridPoint unitPosition)
		{
			List<(UnitId, GridPoint)> available = new(3);
			unitPosition = Grid.GetPoint(unitId);
			foreach (var direction in Constants.GetDirectionPoints(unitPosition))
			{
				if (DirectionHaveUnitInSameTeam(unitId, direction, out UnitId neighborId))
				{
					available.Add((neighborId, direction));
				}
			}
			return available;
		}*/
	}
}