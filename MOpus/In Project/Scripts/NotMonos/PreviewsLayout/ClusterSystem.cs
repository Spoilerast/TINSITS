using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Databases;

namespace NotMonos.PreviewsLayout
{
	internal sealed class ClusterSystem
	{
		private readonly List<DirectionInfo> _directions = new(20);
		private readonly GridDB _grid = DataCenter.Grid;
		private readonly PropertiesDB _properties = DataCenter.Properties;
		private readonly UnitsDB _units = DataCenter.Units;

		private UnitId _initiatorId;
		private GridPoint _initiatorPoint;
		private const float Step = Constants.Step;
		private const int HexEnd = 5; //six cluster vectors can be iterated 5 times if we take N and N+1
									  
		internal static (GridPoint, IEnumerable<ClusterInfo>) SplitSuperClusterOnClusters(
			ClusterInfo clusterInfo,
			IEnumerable<GridPoint> except)
		{
			if (clusterInfo.ClusterType != ClusterType._7)
				throw new ArgumentException("it is not a super cluster", "clusterInfo");

			GridPoint superAxis = clusterInfo.AxisPoint;
			List<GridPoint> exceptList = except.ToList();
			List<ClusterInfo> infos = new(4);
			GridPoint direction1, direction2;
			GridPoint[] cluster;

			for (int i = 0; i <= HexEnd; i++)
			{
				if (i < HexEnd)
				{
					direction1 = clusterInfo.Positions[i];
					direction2 = clusterInfo.Positions[i + 1];
				}
				else
				{
					direction1 = clusterInfo.Positions[0];
					direction2 = clusterInfo.Positions[HexEnd];
				}

				if (exceptList.Contains(direction1) || exceptList.Contains(direction2))
					continue;

				cluster = new GridPoint[]{
						superAxis,
						direction1,
						direction2
						};

				GridPoint axis = FindAxis(cluster);
				//PeekLogger.LogTabTab("clust and axis");
				//PeekLogger.LogItems(cluster);
				//PeekLogger.Log(axis);
				ClusterInfo nfo = new(axis, ClusterType._3, cluster);
				infos.Add(nfo);
			}

			return (superAxis, infos);
		}

		internal void ClearAll()
			=> _directions.Clear();

		internal bool TryFindFormableClusters(
			UnitId unitId,
			GridPoint[] availableMoves,
			out IEnumerable<DirectionInfo> infos)
		{
			_initiatorId = unitId;
			_initiatorPoint = _grid.GetPoint(_initiatorId);

			IEnumerable<GridPoint> availableAndSelf = availableMoves.Append(_initiatorPoint);

			foreach (var direction in availableAndSelf)
				Check4Clusters(direction);

			infos = _directions;
			return _directions.Count > 0;
		}

		private static GridPoint FindAxis(GridPoint[] positions)
		{
			float x, z;
			GridPoint footA, footB, perpendicular;
			if (positions[0].IsSameHeight(positions[1]))
			{
				footA = positions[0];
				footB = positions[1];
				perpendicular = positions[2];
			}
			else if (positions[0].IsSameHeight(positions[2]))
			{
				footA = positions[0];
				footB = positions[2];
				perpendicular = positions[1];
			}
			else
			{
				footA = positions[1];
				footB = positions[2];
				perpendicular = positions[0];
			}
			x = MiddleOf2(footA.X, footB.X);
			z = MiddleOf2(footA.Z, perpendicular.Z);
			return new GridPoint(x, z);
		}

		private static float MiddleOf2(in float a, in float b)
			=> float.IsNegative(a - b)
			? a + Step
			: a - Step;

		private void Check4Clusters(GridPoint center)
		{
			List<ClusterInfo> clusterInfos = new(6); //maximum amount of clusters from center = 6
			GridPoint[] vectors = Constants.GetClusterPoints(center).ToArray();			
			GridPoint direction1, direction2;

			for (int i = 0; i <= HexEnd; i++)
			{
				if (i < HexEnd)
				{
					direction1 = vectors[i];
					direction2 = vectors[i + 1];
				}
				else
				{
					direction1 = vectors[0];
					direction2 = vectors[HexEnd];
				}

				if (TryCheckPairAndAxis(direction1, direction2, center, ref i, out ClusterInfo info))
					clusterInfos.Add(info);
			}

			if (clusterInfos.Count > 0)
				_directions.Add(new(center, clusterInfos.ToArray()));
		}

		private bool CheckDirectionForClusterPart(GridPoint direction, UnitId initiatorId)
				=> _grid.TryGetUnitId(direction, out UnitId directionUnitId)
				&& directionUnitId != initiatorId
				&& _units.IsNotClustered(directionUnitId)
				&& _properties.InSameTeam(initiatorId, directionUnitId);
				
		private bool TryCheckPairAndAxis(GridPoint direction1,
								   GridPoint direction2,
								   GridPoint center,
								   ref int loopCounter,
								   out ClusterInfo info)
		{
			//PeekLogger.LogParams(direction1.ToRichString, direction2.ToRichString, center);
			bool checkFirst = CheckDirectionForClusterPart(direction1, _initiatorId),
			checkNext = CheckDirectionForClusterPart(direction2, _initiatorId);
			if (!checkNext)
				loopCounter++; //skip next iteration

			if (checkFirst && checkNext)
			{
				GridPoint[] positions = new GridPoint[]{
					center,
					direction1,
					direction2
					};
				GridPoint axisPoint = FindAxis(positions);

				//PeekLogger.LogTabTab("clust and axis");
				//PeekLogger.LogItems(positions);
				//PeekLogger.Log(axisPoint);
				if (_grid.IsFreeOrSelf(axisPoint, _initiatorId))
				{
					info = new(axisPoint, ClusterType._3, positions);
					return true;
				}
			}

			info = default;
			return false;
		}
	}
}