using System;
using NotMonos.Databases;
using UnityEngine;

namespace NotMonos.SaveLoad
{
[Serializable]
public struct PowerSourceData
{
	[SerializeField] public byte team;
	[SerializeField] public int posX;
	[SerializeField] public int posZ;

	internal PowerSourceData(TeamId team, GridPoint position)
	{
		(this.team, posX, posZ) = (
			team.Value,
			ISerializedFloats.ToSerialized(position.X),
			ISerializedFloats.ToSerialized(position.Z));
	}

	public readonly override string ToString()
		=> $"ps in {posX}, {posZ}   t {team}";

	internal readonly (float x, float z, byte team) GetData
		=> (ISerializedFloats.FromSerialized(posX),
			ISerializedFloats.FromSerialized(posZ),
			team);
}
}