using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace NotMonos
{
internal sealed class ClusterId : IEquatable<ClusterId>
{
	private const ushort MaxValue = ushort.MaxValue;
	private static readonly SortedDictionary<ushort, ClusterId> _cache = new();
	private static readonly Stack<ushort> _removedClusterIds = new();
	private readonly ushort _id;

	private ClusterId() { _id = 0; }

	private ClusterId(ushort value) { _id = value; }

	internal static ClusterId NotInCluster {
		get {
			if (_cache.TryGetValue(0, out ClusterId id))
				return id;

			id = new();
			_cache.Add(0, id);
			return id;
		}
	}

	internal bool IsNotInCluster => _id == 0;

	public bool Equals(ClusterId other)
		=> _id == other!._id; //potential null reference

	public override int GetHashCode()
		=> _id;

	public static explicit operator ushort(ClusterId v)
		=> v._id;

	public override string ToString()
		=> $"[CID {_id}]";

	internal static void ClearIds()
	{
		_cache.Clear();
		_removedClusterIds.Clear();
	}

	internal static ClusterId GetNewID()
	{
		ushort value;
		if (_removedClusterIds.Count == 0){
			value = _cache.LastOrDefault().Key;
			if (value == MaxValue)
				throw new IndexOutOfRangeException($"Id is exceeds the limit ({value})");

			value++;
		}
		else{
			value = _removedClusterIds.Pop();
		}

		ClusterId id = new(value);
		_cache[value] = id;
		return id;
	}

	internal void RemoveThisId()
	{
		PeekLogger.LogMessage("i remove cluster " + this);
		_removedClusterIds.Push(_id);
		_ = _cache.Remove(_id);
	}
}
}