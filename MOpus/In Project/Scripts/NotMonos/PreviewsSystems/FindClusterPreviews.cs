using System.Collections.Generic;
using System.Linq;
using NotMonos.Databases;

namespace NotMonos.Previews
{
internal sealed class FindClusterPreviews : FindPreviewsSystem
{
	protected override void GetInfos()
	{
		foreach (GridPoint direction in AvailableAndSelf)
			Check4Clusters(direction);
	}

	private void Check4Clusters(GridPoint center)
	{
		List<ClusterInfo> clusterInfos = new(MaxClusters);
		GridPoint[] possibles = Constants.GetClusterPoints(center).ToArray();
		GridPoint direction1, direction2;
		int end = possibles.Length;

		for (var i = 0; i < end; i++){
			direction1 = possibles[i];
			direction2 = possibles[(i + 1) % end];

			if (!TryCheckPairAndAxis(direction1, direction2, center,
									 ref i, out ClusterInfo info))
				continue;

			clusterInfos.Add(info);
			PreviewsCount++;
		}

		if (clusterInfos.Count == 0)
			return;

		AddToDirections(center, clusterInfos);
	}

	private static bool CheckDirectionForClusterPart(GridPoint direction)
		=> Grid.TryGetUnitId(direction, out UnitId directionUnitId)
		   && directionUnitId != InitiatorId
		   && Units.IsNotClustered(directionUnitId)
		   && Properties.InSameTeam(InitiatorId, directionUnitId);

	private static bool TryCheckPairAndAxis(GridPoint direction1,
											GridPoint direction2,
											GridPoint center,
											ref int loopCounter,
											out ClusterInfo info)
	{
		//PeekLogger.LogParams(direction1.ToRichString, direction2.ToRichString, center);
		bool checkFirst = CheckDirectionForClusterPart(direction1),
			 checkNext = CheckDirectionForClusterPart(direction2);
		if (!checkNext)
			loopCounter++; //skip next iteration

		if (checkFirst && checkNext){
			GridPoint[] positions = {
				center,
				direction1,
				direction2
			};
			GridPoint axisPoint = FindAxis(positions);

			//PeekLogger.LogTabTab("clust and axis");
			//PeekLogger.LogItems(positions);
			//PeekLogger.Log(axisPoint);
			if (Grid.IsFreeOrSelf(axisPoint, InitiatorId)){
				info = new(axisPoint, ClusterType._3, positions);
				return true;
			}
		}

		info = null;
		return false;
	}
}
}