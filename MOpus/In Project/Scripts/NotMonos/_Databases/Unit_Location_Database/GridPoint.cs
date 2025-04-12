using System;

namespace NotMonos.Databases
{
internal enum Axis
{
	X, Z, Both
}

internal sealed class GridPoint : IEquatable<GridPoint>, IComparable<GridPoint>, IRichString
{
	private const float Epsilon = 1e-4f;

	internal GridPoint(in float x, in float z) { (X, Z) = (x, z); }

/*		public UnityEngine.Vector2 ToVector2
					=> new(X, Z);*/

	/*	public UnityEngine.Vector3 ToVector3
					=> new(X, 0, Z);*/

	public float X { get; }

	public float Z { get; }

	internal static GridPoint NAP
		=> new(float.NaN, float.NaN); //todo what if operator called for Point and Not-a-Point? (most likely exception)

	internal bool IsNAP => float.IsNaN(X);

	public int CompareTo(GridPoint other)
	{
		if (other is null)
			return 1;

		if (this == other)
			return 0;

		int xCompare = X.CompareTo(other.X);
		return xCompare == 0
			? Z.CompareTo(other.Z)
			: xCompare;
	}

	public bool Equals(GridPoint other)
		=> this == other;

	public string ToRichString => $"<color=orange><{X}, {Z}></color>";

	public static bool AreFloatEquals(float a, float b)
		=> Math.Abs(a - b) < Epsilon;

	public override bool Equals(object obj)
		=> obj is GridPoint other
		   && this == other;

	public override int GetHashCode()
	{
		if (X is float.NaN)
			return -1;

		return HashCode.Combine(X.GetHashCode(), Z.GetHashCode());
	}

	public static bool IsReachable(GridPoint a, GridPoint b)
		=> Math.Abs(Math.Abs(a.Z - b.Z) - Constants.Step) < Epsilon;

	public static GridPoint operator+(GridPoint a, GridPoint b)
		=> new(a.X + b.X, a.Z + b.Z);

	public static bool operator==(GridPoint a, GridPoint b)
		=> Math.Abs(a!.X - b!.X) < Epsilon
		   && Math.Abs(a.Z - b.Z) < Epsilon;

	public static implicit operator bool(GridPoint v)
		=> v is not null;

	public static bool operator!=(GridPoint a, GridPoint b)
		=> !(a == b);

	public static GridPoint operator*(GridPoint a, float b)
		=> new(a.X * b, a.Z * b);

	public static GridPoint operator*(float b, GridPoint a)
		=> a * b;

	public static GridPoint operator-(GridPoint a, GridPoint b)
		=> new(a.X - b.X, a.Z - b.Z);

	public static GridPoint operator-(GridPoint a)
		=> new(-a.X, -a.Z);

	public override string ToString()
		=> X is float.NaN
			? "NAP"
			: $"<{X}, {Z}>";

	internal bool IsSameHeight(GridPoint point)
		=> AreFloatEquals(Z, point.Z);

	internal GridPoint ScaleOnAxis(Axis axis, float multiplier)
	{
		return axis switch {
			Axis.X    => new(X * multiplier, Z),
			Axis.Z    => new(X, Z * multiplier),
			Axis.Both => new(X * multiplier, Z * multiplier),
			_         => throw new NotImplementedException()
		};
	}
}
}