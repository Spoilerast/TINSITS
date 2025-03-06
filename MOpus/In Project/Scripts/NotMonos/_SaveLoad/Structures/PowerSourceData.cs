using System;
using NotMonos.Databases;
using UnityEngine;

namespace NotMonos.SaveLoad
{
	[Serializable]
	public struct PowerSourceData
	{
		[SerializeField] public byte _team;
		[SerializeField] public int _pos_x;
		[SerializeField] public int _pos_z;

		internal PowerSourceData(TeamId team, GridPoint position)
			=> (_team, _pos_x, _pos_z) = (
			team.Value,
			ISerializedFloats.ToSerialized(position.X),
			ISerializedFloats.ToSerialized(position.Z));

		public override readonly string ToString()
		{
			return $"ps in {_pos_x}, {_pos_z}   t {_team}";
		}

		internal readonly (float x, float z, byte team) GetData
			=> (ISerializedFloats.FromSerialized(_pos_x),
				ISerializedFloats.FromSerialized(_pos_z),
				_team);
	}
}