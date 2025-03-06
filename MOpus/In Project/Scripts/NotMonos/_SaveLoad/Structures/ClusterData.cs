using System;
using UnityEngine;

namespace NotMonos.SaveLoad
{
	[Serializable]
	public struct ClusterData
	{
		[SerializeField] private int _axis_pos_x;
		[SerializeField] private int _axis_pos_z;
		[SerializeField] private byte _type;
		[SerializeField] private ushort[] _ids;

		public ClusterData(byte type, float axis_x, float axis_z, ushort[] ids)
			=> (_ids, _type, _axis_pos_x, _axis_pos_z) =
			(ids,
			type,
			ISerializedFloats.ToSerialized(axis_x),
			ISerializedFloats.ToSerialized(axis_z));

		internal readonly (byte type, float x, float z, ushort[] ids) GetData
			=> (_type,
			ISerializedFloats.FromSerialized(_axis_pos_x),
			ISerializedFloats.FromSerialized(_axis_pos_z),
			_ids);
	}
}