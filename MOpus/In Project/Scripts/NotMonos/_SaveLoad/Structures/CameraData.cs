using System;
using UnityEngine;

namespace NotMonos.SaveLoad
{
	[Serializable]
	public struct CameraData
	{
		[SerializeField] private int _pos_x;
		[SerializeField] private int _pos_z;

		public CameraData(Vector3 position)
			=> (_pos_x, _pos_z) =
			(ISerializedFloats.ToSerialized(position.x),
			ISerializedFloats.ToSerialized(position.z));

		internal readonly (float x, float z) GetData
			=> (ISerializedFloats.FromSerialized(_pos_x),
			ISerializedFloats.FromSerialized(_pos_z));
	}
}