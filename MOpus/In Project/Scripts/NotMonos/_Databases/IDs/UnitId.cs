using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace NotMonos
{
	internal sealed class UnitId : IEquatable<UnitId>, IEquatable<ushort>//, IComparable<UnitId>
	{
		internal const int MaxValue = ushort.MaxValue - 1;
		private static readonly SortedDictionary<ushort, UnitId> _cache = new();
		private static readonly Stack<ushort> _removedUnitIds = new();

		private UnitId(ushort value)
			=> Value = value;

		private UnitId()
			=> Value = 0;

		internal static IEnumerable<UnitId> GetAllIds
			=> _cache.Values;

		internal static UnitId PowerSource
			=> new(ushort.MaxValue);

		internal static UnitId Zero
			=> new();

		internal bool IsPowerSource
			=> Value == ushort.MaxValue;

		internal bool IsZero
			=> Value == 0;

		internal string ToRichString
			=> IsPowerSource
			? "<color=blue>[PS]</color>"
			: $"<color=#05ffc5>[UID <b>{Value}</b>]</color>";

		internal ushort Value { get; }

		public static implicit operator bool(UnitId v)
			=> v != null;

		public static implicit operator ushort(UnitId v)
			=> v.Value;

		public bool Equals(UnitId other)
			=> Value == other.Value;

		public bool Equals(ushort other)
			=> Value == other;

		public override int GetHashCode()
			=> Value;

		public override string ToString()
			=> IsPowerSource
			? "[PS]"
			: $"[UID  {Value}]";

		internal static void ClearIds()
		{
			_cache.Clear();
			_removedUnitIds.Clear();
		}

		internal static UnitId Find(ushort id)
			=> !_cache.ContainsKey(id)
				? throw new ArgumentException("There is no such id in Unit ids", "id")
				: _cache[id];

		internal static UnitId GetNewID()
		{
			ushort value;
			if (_removedUnitIds.Count == 0)
			{
				value = _cache.LastOrDefault().Key;
				if (value == MaxValue)
					throw new ArgumentOutOfRangeException("value", "Id is exceeds the limit");

				value++;
			}
			else
				value = _removedUnitIds.Pop();

			UnitId id = new(value);
			_cache[value] = id;
			return id;
		}

		internal static UnitId LoadID(ushort id)
		{
			if (_cache.ContainsKey(id)) //cache must be cleared before, so this is never happens in theory
				throw new ArgumentException("This id is already exists");

			if (id == 0)
				throw new ArgumentException("Id cannot be zero");

			UnitId uid = new(id);
			_cache[id] = uid;
			return uid;
		}

		internal static void RemoveID(UnitId unit_Id) => unit_Id.RemoveThisId();

		internal void RemoveThisId()
		{
			PeekLogger.LogMessage("i remove " + Value);
			_removedUnitIds.Push(Value);
			_ = _cache.Remove(Value);
		}
	}
}