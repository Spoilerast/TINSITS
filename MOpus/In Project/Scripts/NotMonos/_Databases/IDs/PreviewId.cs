using System;
using System.Collections.Generic;
using System.Linq;

namespace NotMonos
{
internal sealed class PreviewId : IEquatable<PreviewId>
{
	private static readonly SortedDictionary<byte, PreviewId> _cache = new();
	private readonly byte _id;

	private PreviewId(byte value) { _id = value; }

	public bool Equals(PreviewId other)
		=> other!._id == _id; //potential null reference

	public override string ToString()
		=> $"[PID {_id}]";

	internal static void ClearCache() { _cache.Clear(); }

	internal static PreviewId GetNewID()
	{
		byte value = _cache.LastOrDefault().Key;
		if (value == byte.MaxValue)
			throw new IndexOutOfRangeException($"Id is exceeds the limit ({value})");

		value++;

		PreviewId pid = new(value);
		_cache[value] = pid;
		return pid;
	}
}
}