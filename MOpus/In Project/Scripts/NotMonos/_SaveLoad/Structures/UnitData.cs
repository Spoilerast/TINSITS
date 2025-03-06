using System;
using UnityEngine;

namespace NotMonos.SaveLoad
{
	[Serializable]
	public struct UnitData
	{
		[SerializeField] private ushort _id;
		[SerializeField] private byte _team;
		[SerializeField] private byte _type;
		[SerializeField] private int _coord_x;
		[SerializeField] private int _coord_z;
		[SerializeField] private int _integrity;
		[SerializeField] private int _capacity;
		[SerializeField] private int _charge;
		[SerializeField] private int _resistance;

		public UnitData(
			ushort unitId,
			byte teamId,
			byte type,
			float coord_x,
			float coord_z,
			float integrity,
			float capacity,
			float charge,
			float resistance)
			=> (_id, _team, _type, _coord_x, _coord_z, _integrity, _capacity, _charge, _resistance) = (
			unitId,
			teamId,
			type,
			ISerializedFloats.ToSerialized(coord_x),
			ISerializedFloats.ToSerialized(coord_z),
			ISerializedFloats.ToSerialized(integrity),
			ISerializedFloats.ToSerialized(capacity),
			ISerializedFloats.ToSerialized(charge),
			ISerializedFloats.ToSerialized(resistance));

		public override readonly string ToString()
		{
			return $"unit {_id} in {_coord_x}, {_coord_z}   t {_team}";
		}

		internal readonly void Deconstruct(
			out ushort id,
			out byte teamId,
			out byte type,
			out float coord_x,
			out float coord_z,
			out float integrity,
			out float capacity,
			out float charge,
			out float resistance)
		{
			id = _id;
			teamId = _team;
			type = _type;
			coord_x = ISerializedFloats.FromSerialized(_coord_x);
			coord_z = ISerializedFloats.FromSerialized(_coord_z);
			integrity = ISerializedFloats.FromSerialized(_integrity);
			capacity = ISerializedFloats.FromSerialized(_capacity);
			charge = ISerializedFloats.FromSerialized(_charge);
			resistance = ISerializedFloats.FromSerialized(_resistance);
		}
	}
}