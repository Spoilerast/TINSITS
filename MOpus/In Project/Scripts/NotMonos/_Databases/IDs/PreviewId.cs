using System;
using System.Collections.Generic;
using System.Linq;

namespace NotMonos
{
	internal sealed class PreviewId : IEquatable<PreviewId>
	{
		private static readonly SortedDictionary<byte, PreviewId> _cache = new();
		private readonly byte _id;

		public PreviewId(byte value)
			=> _id = value;

		internal int Value
			=> _id;

		public override string ToString()
			=> $"[PID {_id}]";

		internal static void Clear()
			=> _cache.Clear();

		internal static PreviewId GetNewID()
		{
			byte value = _cache.LastOrDefault().Key;
			if (value == byte.MaxValue)
				throw new ArgumentOutOfRangeException("value", "Id is exceeds the limit");

			value++;

			PreviewId pid = new(value);
			_cache[value] = pid;
			return pid;
		}

		public bool Equals(PreviewId other)
			=> other._id == _id;
	}
}