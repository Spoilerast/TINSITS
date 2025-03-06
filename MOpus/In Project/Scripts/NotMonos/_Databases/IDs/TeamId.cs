using System;
using System.Collections.Generic;

namespace NotMonos
{
	internal sealed class TeamId : IEquatable<TeamId>, IEquatable<byte>
	{
		private static readonly SortedDictionary<byte, TeamId> _cache = new();

		private TeamId(byte value)
			=> Value = value;

		internal static TeamId NAT
			=> new(42);

		internal bool IsNAT
			=> _cache.ContainsKey(Value);

		internal byte Value { get; }

		public static bool operator !=(TeamId left, TeamId right)
			=> !left.Equals(right);

		public static bool operator ==(TeamId left, TeamId right)
			=> left.Equals(right);

		public bool Equals(TeamId other)
			=> Value == other.Value;

		public bool Equals(byte other)
			=> Value == other;

		public override bool Equals(object obj)
			=> obj is TeamId id && Equals(id);

		public override int GetHashCode()
			=> Value;

		public override string ToString()
			=> $"[TID {Value}]";

		internal static TeamId GetTeamId(byte id)
			=> _cache.ContainsKey(id)
				? _cache[id]
				: CreateId(id);

		private static TeamId CreateId(byte id)
		{
			_cache.Add(id, new(id));
			return _cache[id];
		}

		/*
				internal TeamId(int value)
				{
					if (value is < byte.MinValue or > byte.MaxValue)//todo
						throw new ArgumentOutOfRangeException("value");

					_id = (byte)value;
				}*/
		/*public static implicit operator TeamId(int v) => new(v);

		public static implicit operator byte(TeamId v) => v._id; //todo make validator*/
	}
}