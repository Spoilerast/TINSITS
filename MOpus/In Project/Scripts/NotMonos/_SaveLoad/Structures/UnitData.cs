using System;
using UnityEngine;

namespace NotMonos.SaveLoad
{
[Serializable]
public struct UnitData
{
	[SerializeField] public ushort id;
	[SerializeField] public byte team;
	[SerializeField] public byte type;
	[SerializeField] public int coordX;
	[SerializeField] public int coordZ;
	[SerializeField] public int integrity;
	[SerializeField] public int capacity;
	[SerializeField] public int charge;
	[SerializeField] public int resistance;

	public UnitData(ushort unitId,
					byte teamId,
					byte type,
					float coordX,
					float coordZ,
					float integrity,
					float capacity,
					float charge,
					float resistance)
	{
		(id, team, this.type, this.coordX, this.coordZ, this.integrity, this.capacity, this.charge, this.resistance) = (
			unitId,
			teamId,
			type,
			ISerializedFloats.ToSerialized(coordX),
			ISerializedFloats.ToSerialized(coordZ),
			ISerializedFloats.ToSerialized(integrity),
			ISerializedFloats.ToSerialized(capacity),
			ISerializedFloats.ToSerialized(charge),
			ISerializedFloats.ToSerialized(resistance));
	}

	public readonly override string ToString()
		=> $"unit {id} in {coordX}, {coordZ}   t {team}";

	internal readonly void Deconstruct(out ushort id,
									   out byte teamId,
									   out byte type,
									   out float coordX,
									   out float coordZ,
									   out float integrity,
									   out float capacity,
									   out float charge,
									   out float resistance)
	{
		id = this.id;
		teamId = team;
		type = this.type;
		coordX = ISerializedFloats.FromSerialized(this.coordX);
		coordZ = ISerializedFloats.FromSerialized(this.coordZ);
		integrity = ISerializedFloats.FromSerialized(this.integrity);
		capacity = ISerializedFloats.FromSerialized(this.capacity);
		charge = ISerializedFloats.FromSerialized(this.charge);
		resistance = ISerializedFloats.FromSerialized(this.resistance);
	}
}
}