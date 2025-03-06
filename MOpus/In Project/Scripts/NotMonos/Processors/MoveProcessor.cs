using System.Collections.Generic;
using Extensions;
using NotMonos.Databases;
using NotMonos.PreviewsLayout;

namespace NotMonos.Processors
{
	internal sealed class MoveProcessor : Processor
	{
		internal static void MoveByNest(UnitId initiator, GridPoint newPosition, in PreviewsSystem previewsSystem)
		{
			PeekLogger.LogName();
			Connections.Debug_PrintNets("MoveByNest");
			if (previewsSystem.HaveSelfDecluster)
			{
				PeekLogger.LogItems("before declusters", previewsSystem.GetDeclustersIds);
				ClusterProcessor.Declusterize(initiator);
				Connections.Debug_PrintNets("after declusters");
			}
			Connections.RemoveConnections(initiator);
			Connections.Debug_PrintNets("after deconnect");
			MoveUnit(initiator, newPosition);
			//PeekLogger.LogTab("after move");
			//Connections.DebugPrintNets();
			Connections.RefreshConnections();//todo maybe move somewhere
			Connections.Debug_PrintNets("after refresh");
		}

		internal static void MoveByPreview(
			UnitId initiator,
			GridPoint newPosition,
			ClusterInfo clusterInfo,
			in PreviewsSystem previewsSystem)
		{
			PeekLogger.LogName();
			Connections.Debug_PrintNets("MoveByPreview");

			if (previewsSystem.HaveDeclusters)
			{
				PeekLogger.LogItems("before declusters", previewsSystem.GetDeclustersIds);
				if (previewsSystem.HaveSelfDecluster)
				{
					ClusterProcessor.Declusterize(initiator);
					Connections.Debug_PrintNets("after self decluster");
				}

				PeekLogger.LogTab($"newpos {newPosition} cltp {clusterInfo.ClusterType}");
				if (clusterInfo.ClusterType is ClusterType._7
					&& previewsSystem.TryGetDeclustersOnDirection(newPosition, out var declustersOnDirection))
				{
					PeekLogger.LogItems("declusters", declustersOnDirection);
					Declusterize(declustersOnDirection);
					Connections.Debug_PrintNets("after declusters");
				}
			}
			MoveUnit(initiator, newPosition);
			//PeekLogger.LogTab("after move");
			//Connections.DebugPrintNets();

			ClusterProcessor.ConfirmCluster(clusterInfo);
			Connections.Debug_PrintNets("after confirm");
			Connections.RefreshConnections();//todo maybe move somewhere
			Connections.Debug_PrintNets("after refresh");
		}

		private static void Declusterize(IEnumerable<UnitId> declustersIds)
			=> ClusterProcessor.DeclusterizeRange(declustersIds);

		private static void MoveUnit(UnitId initiator, GridPoint newPosition)
		{
			if (Grid.MoveUnitTo(initiator, newPosition))
				Units.MoveUnitTo(initiator, newPosition);
		}
	}
}