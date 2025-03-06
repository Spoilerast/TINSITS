using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;
using UnityEngine.UIElements;
using CS = NotMonos.Databases.Constants;

namespace NotMonos.Processors
{
	internal sealed partial class ConnectionProcessor//todo: questionable place, reconsider
	{
		private sealed class ConnectionsProducer
		{
			private readonly List<(UnitId, GridPoint, UnitId, GridPoint)> _allConnections = new(33);
			private readonly List<ClusterId> _alreadyLinkedClusters = new(6);
			private readonly ClustersDB _clusters;
			private readonly UnitsDB _units;
			private readonly GridPoint _way = CS.Forward.ScaleOnAxis(Axis.Z, 3);
			private GridPoint _initiatorAxis;
			private ClusterInfo _initiatorCluster;
			private UnitId _initiatorId;

			internal ConnectionsProducer(UnitsDB units, ClustersDB clusters)
				=> (_units, _clusters) = (units, clusters);

			private enum LinkLevel
			{ Two, Three, Four }

			internal IEnumerable<(UnitId, GridPoint, UnitId, GridPoint)> MakeClusterConnectionsFor(UnitId unitId)
			{
				FirstPart(unitId);

				//level 2
				CS.VectorsOrientation clusterOrientation = GetClusterOrientation(out var start);
				start -= CS.ClusterRight;
				ClusterLevel2Connections(start, clusterOrientation);

				//level 3
				CS.VectorsOrientation startOrientation = CS.GetVectorOrientation(start);
				GridPoint variable = clusterOrientation is CS.VectorsOrientation.Bottom
					? start + CS.Forward
					: start + CS.BackRight;
				start = startOrientation is CS.VectorsOrientation.Top
					? variable
					: start + _way;
				ClusterLevel3Connections(start, clusterOrientation, startOrientation);
				return _allConnections;
			}

			internal IEnumerable<(UnitId, GridPoint, UnitId, GridPoint)> MakeSuperClusterConnectionsFor(UnitId unitId)
			{
				FirstPart(unitId);

				//level 2
				GridPoint start = _initiatorCluster.Positions[0] + CS.ClusterForwardLeft;
				ClusterHexagonConnections(start, LinkLevel.Two);

				//level 3
				CS.VectorsOrientation orientation = CS.GetVectorOrientation(start);

				start = orientation is CS.VectorsOrientation.Top
					? start - (_way + CS.ClusterForwardRight)
					: start + CS.ForwardLeft;

				SuperClusterLevel3Connections(start, orientation);

				//level 4
				start = orientation is CS.VectorsOrientation.Top
					? start + (CS.ForwardRight + (CS.ClusterForwardLeft * 3))
					: start + CS.Forward;
				ClusterHexagonConnections(start, LinkLevel.Four);
				return _allConnections;
			}

			private void AddConnectionIfNotLinkedYet(UnitId neighborId)
			{
				ClusterId neighborClusterId = Units.GetClusterId(neighborId);
				if (_alreadyLinkedClusters.Contains(neighborClusterId))
					return;

				GridPoint neighborAxis = Clusters.GetClusterInfo(neighborClusterId).AxisPoint;
				_alreadyLinkedClusters.Add(neighborClusterId);
				_allConnections.Add((_initiatorId, _initiatorAxis, neighborId, neighborAxis));
			}

			private void CheckPositionForLongConnection(GridPoint point)
			{
				Connectability ability = DirectionValidForLongConnection(_initiatorId, point, out var neighborId);
				if (ability is Connectability.Unconnectable)
					return;

				if (ability is Connectability.PowerSourceConnection)
				{
					_allConnections.Add((_initiatorId, _initiatorAxis, neighborId, point));
					return;
				}

				AddConnectionIfNotLinkedYet(neighborId);
			}

			private void CheckPositionForLongestConnection(GridPoint point)
			{
				Connectability ability = DirectionValidForLongestConnection(_initiatorId, point, out var neighborId);
				if (ability is Connectability.Unconnectable)
					return;

				if (ability is Connectability.PowerSourceConnection)
				{
					_allConnections.Add((_initiatorId, _initiatorAxis, neighborId, point));
					return;
				}

				AddConnectionIfNotLinkedYet(neighborId);
			}

			private void Clear()
			{
				_allConnections.Clear();
				_alreadyLinkedClusters.Clear();
			}

			private void ClusterHexagonConnections(GridPoint start, LinkLevel level)
			{
				var vectors = new Queue<GridPoint>(CS.GetClusterPoints());
				GridPoint vector;
				bool isLvl4 = level == LinkLevel.Four;

				ChooseCheck();

				vector = vectors.Dequeue();
				start += vector;
				ChooseCheck();

				if (isLvl4)
				{
					start += vector;
					ChooseCheck();
				}

				int stepsNumber = isLvl4
					? 3
					: 2;
				while (vectors.Count > 0)
				{
					vector = vectors.Dequeue();
					for (int i = stepsNumber; i > 0; i--)
					{
						start += vector;

						ChooseCheck();
					}
				}

				void ChooseCheck()
				{
					if (level is LinkLevel.Two)
					{
						CheckPositionForLongConnection(start);
						return;
					}
					CheckPositionForLongConnection(start);
				}
			}

			private void ClusterLevel2Connections(GridPoint start, CS.VectorsOrientation orientation)
			{
				Queue<GridPoint> vectors = new(CS.ClusterLevel2Vectors(orientation));

				CheckPositionForLongConnection(start);
				while (vectors.Count > 0)
				{
					start += vectors.Dequeue();
					CheckPositionForLongConnection(start);
				}
			}

			private void ClusterLevel3Connections(GridPoint start,
										 CS.VectorsOrientation clusterOrientation,
										 CS.VectorsOrientation startOrientation)
			{
				if (clusterOrientation is CS.VectorsOrientation.Bottom)
				{
					if (startOrientation is CS.VectorsOrientation.Top)
						ClusterHexagonConnections(start, LinkLevel.Three);
					else
						ClusterPyramideConnections(start, clusterOrientation);
					return;
				}

				if (startOrientation is CS.VectorsOrientation.Top)
					ClusterPyramideConnections(start, clusterOrientation);
				else
					ClusterHexagonConnections(start, LinkLevel.Three);
			}

			private void ClusterPyramideConnections(GridPoint start, CS.VectorsOrientation orientation)
			{
				Queue<GridPoint> vectors = new(CS.ClusterPyramideVectors(orientation));
				GridPoint vector = GridPoint.NAP;

				//3x 1st, 1x 2nd, 3x 3rd, 1x 4th, 3x 5th (1-3-1-3-1-3)
				JigsawLoopLongestCheck(3, 1, ref start, ref vectors, ref vector);
			}

			private bool DirectionValidForConnectionLevel1(
							GridPoint direction,
				out UnitId neighborId,
				out ClusterId clusterId)
			{
				clusterId = null;
				if (!DirectionHaveUnitOrSourceInSameTeam(_initiatorId, direction, out neighborId))
					return false;

				if (neighborId.IsPowerSource)
					return true;

				if (!Units.IsNotClustered(neighborId))
				{
					clusterId = Units.GetClusterId(neighborId);
				}
				return true;
			}

			private void FirstPart(UnitId unitId)
			{
				Clear();
				Initialize(unitId);
				Level1Connections();
			}

			private CS.VectorsOrientation GetClusterOrientation(out GridPoint start)
			{
				//not effective but short and readable
				ClusterInfo info = _initiatorCluster;
				start = info.Positions.OrderBy(v => v.X).First();
				float minimumZ = info.Positions.Min(v => v.Z);
				var points = info.Positions.Where(v => GridPoint.AreFloatEquals(v.Z, minimumZ))
													.ToArray();
				return points.Length == 1
					? CS.VectorsOrientation.Bottom
					: CS.VectorsOrientation.Top;

				/* ==//=> effective classic and readable but too procedural
				start = info.Positions[0];
				float minZ = float.MaxValue;
				int count = 0;

				for (int i = 0; i < info.Positions.Length; i++)
				{
					GridPoint v = info.Positions[i];
					if (v.X < start.X)
						start = v;

					if (v.Z < minZ)
					{
						minZ = v.Z;
						count = 1;
					}

					else if (GridPoint.AreFloatEquals(v.Z, minZ))
						count++;
				}
				return count == 1
					? CS.VectorsOrientation.Bottom
					: CS.VectorsOrientation.Top;
				*/

				/* ==||=> effective functional but unreadable
				 var (minX,_,count) =
					info.Positions.Aggregate
					(
						(minX: info.Positions[0], minZ: float.MaxValue, count: 0),
						(acc, v)
							=> (
								minX: v.X < acc.minX.X
									? v
									: acc.minX,
								minZ: v.Z < acc.minZ
									? v.Z
									: acc.minZ,
								count: GridPoint.AreFloatEquals(v.Z, acc.minZ)
						   ? (v.Z < acc.minZ
								? 1
								: acc.count + 1)
						   : acc.count
						)
					);
				start = minX;
				return count == 1
					? CS.VectorsOrientation.Bottom
					: CS.VectorsOrientation.Top;*/
			}

			private void Initialize(UnitId unitId)
			{
				_initiatorId = unitId;
				ClusterId cid = _units.GetClusterId(unitId);
				_initiatorCluster = _clusters.GetClusterInfo(cid);
				_initiatorAxis = _initiatorCluster.AxisPoint;
			}

			private void JigsawLoopLongestCheck(int oddIterationsCount, int evenIterationsCount, ref GridPoint start, ref Queue<GridPoint> vectors, ref GridPoint vector)
			{
				const int numberOfTurns = 5; //starting from side A of hexagon you need 5 turns to come on side F
											 //(1-N-K-N-K-N) N is odd, K is even
				CheckPositionForLongestConnection(start);

				for (int j = numberOfTurns; j > 0; j--)
				{
					vector = vectors.Dequeue();
					if (j % 2 != 0)
					{
						for (int i = oddIterationsCount; i > 0; i--)
						{
							start += vector;
							CheckPositionForLongestConnection(start);
						}
					}
					else
					{
						for (int i = evenIterationsCount; i > 0; i--)
						{
							start += vector;
							CheckPositionForLongestConnection(start);
						}
					}
				}
			}

			private void Level1Connections()
			{
				UnitId idA, idB;
				GridPoint pointA, pointB;
				foreach (var vertex in _initiatorCluster.Positions)
				{
					if (!Grid.TryGetUnitId(vertex, out idA))
						continue;

					foreach (var direction in CS.GetMoveDirectionPoints(vertex))
					{
						if (!DirectionValidForConnectionLevel1(direction, out idB, out var neighborClusterId))
							continue;

						if (neighborClusterId == null)
						{
							pointA = vertex;
							pointB = direction;
						}
						else
						{
							if (_alreadyLinkedClusters.Contains(neighborClusterId))
								continue;

							pointA = _initiatorAxis;
							pointB = Clusters.GetClusterInfo(neighborClusterId).AxisPoint;
							_alreadyLinkedClusters.Add(neighborClusterId);
						}
						_allConnections.Add((idA, pointA, idB, pointB));
					}
				}
			}

			private void SuperClusterLevel3Connections(GridPoint start, CS.VectorsOrientation orientation)
			{
				Queue<GridPoint> vectors = new(CS.SuperClusterLevel3Vectors(orientation));
				GridPoint vector;

				CheckPositionForLongestConnection(start);

				vector = vectors.Dequeue();
				start += vector;
				//3x 1st, 2x 2nd, 3x 3rd, 2x 4th, 3x 5th (1-3-2-3-2-3)
				JigsawLoopLongestCheck(3, 2, ref start, ref vectors, ref vector);
			}
		}
	}
}