using System;
using UnityEngine;

namespace NotMonos.SaveLoad
{
[Serializable]
public struct CameraData
{
	[SerializeField] public int posX;
	[SerializeField] public int posZ;

	public CameraData(Vector3 position)
	{
		(posX, posZ) =
			(ISerializedFloats.ToSerialized(position.x),
			 ISerializedFloats.ToSerialized(position.z));
	}

	internal readonly (float x, float z) GetData
		=> (ISerializedFloats.FromSerialized(posX),
			ISerializedFloats.FromSerialized(posZ));
}
}