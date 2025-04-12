using System;
using System.Collections.Generic;
using System.Linq;

namespace NotMonos.Databases
{
internal static class Constants
{
	public static IEnumerable<GridPoint> GetMoveDirectionPoints(GridPoint referencePoint)
		=> IsTopOrientation(referencePoint)
			? TopOrientationPoints(referencePoint)
			: BottomOrientationPoints(referencePoint);

	//todo int step, need to test for float step
	public static VectorsOrientation GetVectorOrientation(GridPoint vector)
		=> IsTopOrientation(vector)
			? VectorsOrientation.Top
			: VectorsOrientation.Bottom;

	public static bool IsTopOrientation(GridPoint vector)
		=> (int)(vector.Z * .5f) % IntStep != 0;

	internal static HolderSide GetPreviewSide(GridPoint direction, GridPoint axisPoint)
	{
		GridPoint vector = axisPoint - direction;
		return vector switch //todo maybe do it in GridPoint?
		{
			not null when vector == Forward      => HolderSide.Forward, //'switch' syntax for runtime values
			not null when vector == Back         => HolderSide.Back,
			not null when vector == BackLeft     => HolderSide.BackLeft,
			not null when vector == BackRight    => HolderSide.BackRight,
			not null when vector == ForwardLeft  => HolderSide.ForwardLeft,
			not null when vector == ForwardRight => HolderSide.ForwardRight,
			_                                    => throw new InvalidOperationException($"{axisPoint} - {direction} = {vector}")
		};
	}

	private static IEnumerable<GridPoint> GetPointsRelativeTo(IEnumerable<GridPoint> gridPoints,
															  GridPoint referencePoint)
		=> from point in gridPoints
		   select point + referencePoint;

	internal enum VectorsOrientation : byte
	{
		Top, Bottom
	}

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

	internal static IEnumerable<GridPoint> BottomOrientationPoints()
	{
		yield return ForwardRight;
		yield return ForwardLeft;
		yield return Back;
	}

	internal static IEnumerable<GridPoint> ClusterLevel2Vectors(VectorsOrientation orientation)
	{
		if (orientation is VectorsOrientation.Bottom){
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

	internal static IEnumerable<GridPoint> ClusterPyramidVectors(VectorsOrientation orientation)
	{
		if (orientation is VectorsOrientation.Bottom){
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

	internal static IEnumerable<GridPoint> GetClusterPoints()
	{
		yield return ClusterForwardRight;
		yield return ClusterRight;
		yield return ClusterBackRight;
		yield return ClusterBackLeft;
		yield return ClusterLeft;
		yield return ClusterForwardLeft;
	}

	internal static IEnumerable<GridPoint> GetClusterPoints(GridPoint referencePoint)
		=> GetPointsRelativeTo(GetClusterPoints(), referencePoint);

	internal static IEnumerable<GridPoint> GetSortedClusterPoints(GridPoint superClusterAxis)
		=> GetPointsRelativeTo(GetSortedClusterPoints(), superClusterAxis)
			.Append(superClusterAxis);

	internal static IEnumerable<GridPoint> GetSortedClusterPoints() //todo maybe this is ClusterPoints?
	{
		yield return ClusterLeft;
		yield return ClusterForwardLeft;
		yield return ClusterForwardRight;
		yield return ClusterRight;
		yield return ClusterBackRight;
		yield return ClusterBackLeft;
	}

	internal static IEnumerable<GridPoint> GetPoints()
	{
		yield return Forward;
		yield return ForwardRight;
		yield return ForwardLeft;
		yield return Back;
		yield return BackRight;
		yield return BackLeft;
	}

	internal static IEnumerable<GridPoint> GetPoints(GridPoint referencePoint)
		=> GetPointsRelativeTo(GetPoints(), referencePoint);

	internal static IEnumerable<GridPoint> SuperClusterLevel3Vectors(VectorsOrientation orientation)
	{
		if (orientation is VectorsOrientation.Bottom){
			foreach (GridPoint item in GetClusterPoints())
				yield return item;

			yield break;
		}

		yield return ClusterForwardLeft;
		yield return ClusterForwardRight;
		yield return ClusterRight;
		yield return ClusterBackRight;
		yield return ClusterBackLeft;
		yield return ClusterLeft;
	}

	internal static IEnumerable<GridPoint> TopOrientationPoints()
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
}
}