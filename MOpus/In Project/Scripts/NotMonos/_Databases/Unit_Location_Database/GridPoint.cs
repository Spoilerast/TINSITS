using System;

namespace NotMonos.Databases
{
	internal enum Axis
	{ X, Z, Both }

	internal sealed class GridPoint : IEquatable<GridPoint>
	{
		private const float Epsilon = 1e-4f;

		internal GridPoint(in float x, in float z)
			=> (X, Z) = (x, z);

		internal GridPoint(in UnityEngine.Vector3 vector)
			=> (X, Z) = (vector.x, vector.z);

		public string ToRichString
			=> $"<color=orange><{X}, {Z}></color>";

		public UnityEngine.Vector2 ToVector2
					=> new(X, Z);

		public UnityEngine.Vector3 ToVector3
					=> new(X, 0, Z);

		public float X { get; }

		public float Z { get; }

		internal static GridPoint NAP
			=> new(float.NaN, float.NaN); //todo what if operator called for Point and Not-a-Point? (most likely exception)

		internal bool IsNAP
			=> float.IsNaN(X);

		public static bool AreFloatEquals(float a, float b)
			=> Math.Abs(a - b) < Epsilon;

		public static implicit operator bool(GridPoint v)
			=> v != null && !v.IsNAP;

		public static GridPoint operator -(GridPoint a, GridPoint b)
			=> new(a.X - b.X, a.Z - b.Z);

		public static GridPoint operator -(GridPoint a)
			=> new(-(a.X), -(a.Z));

		public static bool operator !=(GridPoint a, GridPoint b)
			=> !(a == b);

		public static GridPoint operator *(GridPoint a, float b)
			=> new(a.X * b, a.Z * b);

		public static GridPoint operator *(float b, GridPoint a)
			=> a * b;

		public static GridPoint operator +(GridPoint a, GridPoint b)
			=> new(a.X + b.X, a.Z + b.Z);

		public static bool operator ==(GridPoint a, GridPoint b)
			=> Math.Abs(a.X - b.X) < Epsilon
			&& Math.Abs(a.Z - b.Z) < Epsilon;

		public bool Equals(GridPoint other)
			=> this == other;

		public override bool Equals(object obj)
			=> obj is GridPoint other
			&& this == other;

		public override int GetHashCode()
		{
			if (X is float.NaN)
				return -1;

			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (X.GetHashCode() * 397) ^ Math.Sign(X);
				hash = hash * 23 + (Z.GetHashCode() * 397) ^ Math.Sign(Z);

				return hash;
			}
		}

		public override string ToString()
			=> $"<{X}, {Z}>";

		internal bool IsSameHeight(GridPoint point)
			=> AreFloatEquals(Z, point.Z);

		internal GridPoint ScaleOnAxis(Axis axis, float multiplier)
					=> axis switch
					{
						Axis.X => new(X * multiplier, Z),
						Axis.Z => new(X, Z * multiplier),
						Axis.Both => new(X * multiplier, Z * multiplier),
						_ => throw new NotImplementedException()
					};
	}
}