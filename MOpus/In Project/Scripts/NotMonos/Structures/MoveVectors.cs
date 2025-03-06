using System;
using System.Collections.Generic;
using UnityEngine;

namespace NotMonos
{
	[Obsolete("Old version of NotMonos.Databases.Constants used before migrating to Unity 6")]
	public readonly struct MoveVectors
	{
		#region Header

		/// <summary> atom of distance </summary>
		public const float STEP = 2f;

		/// <summary> vector (0, 0, -step) </summary>
		public static readonly Vector3 back = new(0, 0, -STEP);

		/// <summary> vector (-step, 0, -step) </summary>
		public static readonly Vector3 backLeft = new(-STEP, 0, -STEP);

		/// <summary> vector (step, 0, -step) </summary>
		public static readonly Vector3 backRight = new(STEP, 0, -STEP);

		/// <summary> vector (-step, 0, step) </summary>
		public static readonly Vector3 forwardLeft = new(-STEP, 0, STEP);

		/// <summary> vector (step, 0, step) </summary>
		public static readonly Vector3 forwardRight = new(STEP, 0, STEP);

		/// <summary> vector (0, 0, step) </summary>
		public static readonly Vector3 forward = new(0, 0, STEP);

		/// <summary> vector (step, 0, 2 * step) </summary>
		public static readonly Vector3 clusterForwardRight = new(STEP, 0, 2 * STEP);

		/// <summary> vector (2 * step, 0, 0) </summary>
		public static readonly Vector3 clusterRight = new(2 * STEP, 0, 0);

		/// <summary> vector (step, 0, -2 * step) </summary>
		public static readonly Vector3 cLusterBackRight = new(STEP, 0, -2 * STEP);

		public static readonly Vector3 cLusterBackLeft = Vector3.Scale(cLusterBackRight, new Vector3(-1, 0, 1));
		public static readonly Vector3 cLusterLeft = Vector3.Scale(clusterRight, new Vector3(-1, 0, 1));
		public static readonly Vector3 cLusterForwardLeft = Vector3.Scale(clusterForwardRight, new Vector3(-1, 0, 1));

		#endregion Header

		public static IEnumerable<Vector3> GetClusterVectors()
		{
			yield return clusterForwardRight;
			yield return clusterRight;
			yield return cLusterBackRight;
			yield return cLusterBackLeft;
			yield return cLusterLeft;
			yield return cLusterForwardLeft;
		}

		public static IEnumerable<Vector3> GetClusterVectors(Vector3 add)
		{
			yield return clusterForwardRight + add;
			yield return clusterRight + add;
			yield return cLusterBackRight + add;
			yield return cLusterBackLeft + add;
			yield return cLusterLeft + add;
			yield return cLusterForwardLeft + add;
		}

		//public static IEnumerable<Vector3> GetDirectionVectors(float i)
		//    => i % 1.5f == 0 ? GetRiseVectors() : GetFallVectors();

		public static bool RiseCondition(Vector3 vector) => (int)(vector.z * .5f) % 2 != 0; //TODO: magic 2

		private static bool RiseCondition(ref float z) => (int)(z * .5f) % 2 != 0;

		public static IEnumerable<Vector3> GetDirectionVectors(Vector3 add)
		{
			//Debug.Log($"{add}\t z {add.z} |\t% {add.z%1.5f} \t=={add.z % 1.5f == 0}");
			//maybe make enum and choose it by formula
			return RiseCondition(ref add.z)
				? GetRiseVectors(add)
				: GetFallVectors(add);
		}

		public static IEnumerable<Vector3> GetFallVectors()
		{
			yield return forwardRight;
			yield return forwardLeft;
			yield return back;
		}

		public static IEnumerable<Vector3> GetFallVectors(Vector3 add)
		{
			yield return forwardRight + add;
			yield return forwardLeft + add;
			yield return back + add;
		}

		public static IEnumerable<Vector3> GetRiseVectors()
		{
			yield return forward;
			yield return backRight;
			yield return backLeft;
		}

		public static IEnumerable<Vector3> GetRiseVectors(Vector3 add)
		{
			yield return forward + add;
			yield return backRight + add;
			yield return backLeft + add;
		}

		public static IEnumerable<Vector3> GetVectors()
		{
			yield return forward;
			yield return forwardRight;
			yield return forwardLeft;
			yield return back;
			yield return backRight;
			yield return backLeft;
		}

		public static IEnumerable<Vector3> GetVectors(Vector3 add)
		{
			yield return forward + add;
			yield return forwardRight + add;
			yield return forwardLeft + add;
			yield return back + add;
			yield return backRight + add;
			yield return backLeft + add;
		}

		internal static Prev_Cluster_Side GetPreviewSide(Vector3 vertex, Vector3 axis)
		{
			Vector3 vector = axis - vertex;

			//Debug.Log($"{axis} - {vertex} = {vector}");
			if (vector == forward)
			{
				return Prev_Cluster_Side.Forward;
			}
			else if (vector == back)
			{
				return Prev_Cluster_Side.Back;
			}
			else if (vector == backLeft)
			{
				return Prev_Cluster_Side.BackLeft;
			}
			else if (vector == backRight)
			{
				return Prev_Cluster_Side.BackRight;
			}
			else if (vector == forwardLeft)
			{
				return Prev_Cluster_Side.ForwardLeft;
			}
			else if (vector == forwardRight)
			{
				return Prev_Cluster_Side.ForwardRight;
			}
			else
				throw new System.InvalidOperationException($"{axis} - {vertex} = {vector}");
		}
	}
}