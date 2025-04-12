using System;
using UnityEngine;

namespace NotMonos.SaveLoad
{
[Serializable]
public struct ClusterData
{
	[SerializeField] public int axisPosX;
	[SerializeField] public int axisPosZ;
	[SerializeField] public byte type;
	[SerializeField] public ushort[] ids;

	public ClusterData(byte type, float axisX, float axisZ, ushort[] ids)
	{
		(this.ids, this.type, axisPosX, axisPosZ) =
			(ids,
			 type,
			 ISerializedFloats.ToSerialized(axisX),
			 ISerializedFloats.ToSerialized(axisZ));
	}

	internal readonly (byte type, float x, float z, ushort[] ids) GetData
		=> (type,
			ISerializedFloats.FromSerialized(axisPosX),
			ISerializedFloats.FromSerialized(axisPosZ),
			ids);
}
}