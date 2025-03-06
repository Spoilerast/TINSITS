using System.Collections.Generic;
using System.Linq;

namespace NotMonos.Databases
{
	internal static class Constants
	{
		#region Fields

		internal const float Step = 2f;
		private const int IntStep = (int)Step;
		internal static readonly GridPoint Forward = new(0, Step);
		internal static readonly GridPoint ForwardRight = new(Step, Step);
		internal static readonly GridPoint ForwardLeft = ForwardRight.ScaleOnAxis(Axis.X, -1f);
		internal static readonly GridPoint Back = Forward.ScaleOnAxis(Axis.Z, -1f);
		internal static readonly GridPoint BackLeft = ForwardRight.ScaleOnAxis(Axis.Both, -1f);
		internal static readonly GridPoint BackRight = ForwardLeft.ScaleOnAxis(Axis.Both, -1f);
		internal static readonly GridPoint ClusterForwardRight = new(Step, Step * 2);
		internal static readonly GridPoint ClusterBackLeft = ClusterForwardRight.ScaleOnAxis(Axis.Both, -1f);
		internal static readonly GridPoint ClusterBackRight = ClusterForwardRight.ScaleOnAxis(Axis.Z, -1f);
		internal static readonly GridPoint ClusterForwardLeft = ClusterForwardRight.ScaleOnAxis(Axis.X, -1f);
		internal static readonly GridPoint ClusterRight = new(2 * Step, 0);
		internal static readonly GridPoint ClusterLeft = ClusterRight.ScaleOnAxis(Axis.X, -1f);

		internal const string CurrentSaveVersion = "アッポーペン";

		#endregion Fields

		#region PointsEnumerables

		public static IEnumerable<GridPoint> BottomOrientationPoints()
		{
			yield return ForwardRight;
			yield return ForwardLeft;
			yield return Back;
		}

		public static IEnumerable<GridPoint> ClusterLevel2Vectors(VectorsOrientation orientation)
		{
			if (orientation is VectorsOrientation.Bottom)
			{
				yield return ClusterForwardRight;
				yield return ClusterRight;
				yield return ClusterRight;
				yield return ClusterBackRight;
				yield return ClusterBackLeft;
				yield return ClusterBackLeft;
				yield return ClusterLeft;
				yield return ClusterForwardLeft;
				yield break;
			}
			yield return ClusterForwardRight;
			yield return ClusterForwardRight;
			yield return ClusterRight;
			yield return ClusterBackRight;
			yield return ClusterBackRight;
			yield return ClusterBackLeft;
			yield return ClusterLeft;
			yield return ClusterLeft;
		}

		public static IEnumerable<GridPoint> ClusterPyramideVectors(VectorsOrientation orientation)
		{
			if (orientation is VectorsOrientation.Bottom)
			{
				yield return ClusterRight;
				yield return ClusterBackRight;
				yield return ClusterBackLeft;
				yield return ClusterLeft;
				yield return ClusterForwardLeft;
				yield break;
			}
			yield return ClusterForwardRight;
			yield return ClusterRight;
			yield return ClusterBackRight;
			yield return ClusterBackLeft;
			yield return ClusterLeft;
		}

		public static IEnumerable<GridPoint> GetClusterPoints()
		{
			yield return ClusterForwardRight;
			yield return ClusterRight;
			yield return ClusterBackRight;
			yield return ClusterBackLeft;
			yield return ClusterLeft;
			yield return ClusterForwardLeft;
		}

		public static IEnumerable<GridPoint> GetClusterPoints(GridPoint referencePoint)
			=> GetPointsRelativeTo(GetClusterPoints(), referencePoint);

		public static IEnumerable<GridPoint> GetPoints()
		{
			yield return Forward;
			yield return ForwardRight;
			yield return ForwardLeft;
			yield return Back;
			yield return BackRight;
			yield return BackLeft;
		}

		public static IEnumerable<GridPoint> GetPoints(GridPoint referencePoint)
			=> GetPointsRelativeTo(GetPoints(), referencePoint);

		public static IEnumerable<GridPoint> SuperClusterLevel3Vectors(VectorsOrientation orientation)
		{
			if (orientation is VectorsOrientation.Bottom)
			{
				foreach (var item in GetClusterPoints())
				{
					yield return item;
				}
				yield break;
			}
			yield return ClusterForwardLeft;
			yield return ClusterForwardRight;
			yield return ClusterRight;
			yield return ClusterBackRight;
			yield return ClusterBackLeft;
			yield return ClusterLeft;
		}

		public static IEnumerable<GridPoint> TopOrientationPoints()
		{
			yield return Forward;
			yield return BackRight;
			yield return BackLeft;
		}

		private static IEnumerable<GridPoint> BottomOrientationPoints(GridPoint referencePoint)
			=> GetPointsRelativeTo(BottomOrientationPoints(), referencePoint);

		private static IEnumerable<GridPoint> TopOrientationPoints(GridPoint referencePoint)
			=> GetPointsRelativeTo(TopOrientationPoints(), referencePoint);

		#endregion PointsEnumerables

		internal enum VectorsOrientation : byte
		{
			Top, Bottom
		}

		public static IEnumerable<GridPoint> GetMoveDirectionPoints(GridPoint referencePoint)
			=> IsTopOrientation(referencePoint)
				? TopOrientationPoints(referencePoint)
				: BottomOrientationPoints(referencePoint);

		public static bool IsTopOrientation(GridPoint vector)
			=> (int)(vector.Z * .5f) % IntStep != 0; //todo int step, need to test for float step

		public static VectorsOrientation GetVectorOrientation(GridPoint vector)
			=> IsTopOrientation(vector)
			? VectorsOrientation.Top
			: VectorsOrientation.Bottom;

		internal static Prev_Cluster_Side GetPreviewSide(GridPoint direction, GridPoint axisPoint)
		{
			GridPoint vector = axisPoint - direction;
			return vector switch //todo maybe do it in GridPoint?
			{
				GridPoint when vector == Forward => Prev_Cluster_Side.Forward, //'switch' syntax for runtime values
				GridPoint when vector == Back => Prev_Cluster_Side.Back,
				GridPoint when vector == BackLeft => Prev_Cluster_Side.BackLeft,
				GridPoint when vector == BackRight => Prev_Cluster_Side.BackRight,
				GridPoint when vector == ForwardLeft => Prev_Cluster_Side.ForwardLeft,
				GridPoint when vector == ForwardRight => Prev_Cluster_Side.ForwardRight,
				_ => throw new System.InvalidOperationException($"{axisPoint} - {direction} = {vector}")
			};
		}

		private static IEnumerable<GridPoint> GetPointsRelativeTo(IEnumerable<GridPoint> gridPoints, GridPoint referencePoint)
					=> from point in gridPoints
					   select point + referencePoint;
	}
}