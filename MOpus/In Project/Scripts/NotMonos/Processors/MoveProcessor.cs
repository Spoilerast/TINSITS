using Extensions;
using NotMonos.Databases;

namespace NotMonos.Processors
{
internal abstract class MoveProcessor : Processor
{
	public static void MoveToNest(GridPoint newPosition)
	{
		PeekLogger.LogName($"{newPosition} <=");
		UnitId initiator = Selection.SelectedUnit;

		Connections.RemoveConnections(initiator);
		MoveUnit(initiator, newPosition);
		Connections.RefreshConnections(); //todo maybe move somewhere
	}

	public static void MoveToPreview(GridPoint newPosition, ClusterInfo clusterInfo)
	{
		PeekLogger.LogName($"{newPosition} <= {clusterInfo}");
		UnitId initiator = Selection.SelectedUnit;

		Connections.RemoveConnections(initiator);
		MoveUnit(initiator, newPosition);

		ClusterProcessor.ConfirmCluster(clusterInfo);
		Connections.RefreshConnections(); //todo maybe move somewhere
	}

	private static void MoveUnit(UnitId initiator, GridPoint newPosition)
	{
		if (Grid.TryMoveUnitTo(initiator, newPosition))
			Units.MoveUnitTo(initiator, newPosition);
	}
}
}