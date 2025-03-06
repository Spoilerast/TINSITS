using System.Collections.Generic;
using System.Linq;
using NotMonos.Databases;
using UnityEngine;

namespace NotMonos.Processors
{
	internal sealed class SpawnPointsProcessor : Processor
	{
		private SpawnPointsProcessor()
		{ }

		/*internal static IEnumerable<Vector3> FreeSpawnVectorsForTeam(TeamId teamId)
		{
			foreach (var item in FreeSpawnPointsForTeam(teamId))
				yield return item.ToVector3;
		}*/

		internal static IEnumerable<GridPoint> FreeSpawnPointsForTeam(TeamId teamId)
		{
			var sourcedPoints = Grid.AllSpawnableDirectionsTo(teamId);
			var unitFreePoints
				= from id in Properties.AllSpawnable
				  from dir in Grid.AllFreeDirectionsTo(id)
				  select dir;

			return sourcedPoints.Concat(unitFreePoints);
		}
	}
}