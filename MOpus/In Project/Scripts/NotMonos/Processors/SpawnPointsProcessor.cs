using System.Collections.Generic;
using System.Linq;
using NotMonos.Databases;

namespace NotMonos.Processors
{
internal abstract class SpawnPointsProcessor : Processor
{
	/*internal static IEnumerable<Vector3> FreeSpawnVectorsForTeam(TeamId teamId)
	{
		foreach (var item in FreeSpawnPointsForTeam(teamId))
			yield return item.ToVector3;
	}*/

	internal static IEnumerable<GridPoint> FreeSpawnPointsForTeam(TeamId teamId)
	{
		IEnumerable<GridPoint> sourcedPoints = Grid.AllSpawnableDirectionsTo(teamId);
		IEnumerable<GridPoint> unitFreePoints
			=
			from id in Properties.AllSpawnable
			from dir in Grid.AllFreeDirectionsTo(id)
			select dir;

		return sourcedPoints.Concat(unitFreePoints);
	}
}
}