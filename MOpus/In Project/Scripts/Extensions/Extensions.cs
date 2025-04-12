using System;
using System.Collections.Generic;

namespace Extensions
{
public static class EnumerableExtensions
{
	public static bool AllExcept<TSource>(this IEnumerable<TSource> collection, Func<TSource, bool> predicate, TSource exceptItem)
	{
		foreach (TSource item in collection){
			if (EqualityComparer<TSource>.Default.Equals(item, exceptItem))
				continue;

			if (!predicate(item))
				return false;
		}

		return true;
	}

	public static bool AnyExcept<TSource>(this IEnumerable<TSource> collection, Func<TSource, bool> predicate, TSource exceptItem)
	{
		foreach (TSource item in collection){
			if (EqualityComparer<TSource>.Default.Equals(item, exceptItem))
				continue;

			if (predicate(item))
				return true;
		}

		return false;
	}

	public static IEnumerable<TSource> AppendItem<TSource>(this TSource source, TSource value)
		=> Enumerable.Empty<TSource>().Append(source).Append(value);

	public static IEnumerable<TSource> AsIEnumerable<TSource>(this TSource source)
		=> Enumerable.Empty<TSource>().Append(source);

	public static TSource[] CastToArray<TSource>(this IEnumerable<TSource> source)
		=> source as TSource[] ?? source.ToArray();

	public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> collection, params TSource[] exceptItems)
	{
		bool isIncluded;
		foreach (TSource item in collection){
			isIncluded = true;
			foreach (TSource except in exceptItems)
				if (EqualityComparer<TSource>.Default.Equals(item, except)){
					isIncluded = false;
					break;
				}

			if (isIncluded)
				yield return item;
		}
	}

	public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> collection, TSource exceptItem)
	{
		foreach (TSource item in collection){
			if (EqualityComparer<TSource>.Default.Equals(item, exceptItem))
				continue;

			yield return item;
		}
	}

	public static void ForEach<TSource>(this IEnumerable<TSource> collection, Action<TSource> action)
	{
		foreach (TSource item in collection)
			action(item);
	}

	public static void ForEachExcept<TSource>(this IEnumerable<TSource> collection, Action<TSource> action, TSource exceptItem)
	{
		foreach (TSource item in collection){
			if (EqualityComparer<TSource>.Default.Equals(item, exceptItem))
				continue;

			action(item);
		}
	}

	public static string JoinItemsToString<TSource>(this IEnumerable<TSource> collection)
		//todo make more generic or remove
		=> string.Join(", ", collection);

	public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
											   Func<TSource, TKey> keySelector)
		=> MinOrMaxBy(source, keySelector, false);

	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
											   Func<TSource, TKey> keySelector)
		=> MinOrMaxBy(source, keySelector, true);

	public static IEnumerable<(TKey, TValue)> PairsToTuples<TKey, TValue>(this Dictionary<TKey, TValue> dict)
		=> dict.Select(pair => (pair.Key, pair.Value));

	private static TSource MinOrMaxBy<TSource, TKey>(this IEnumerable<TSource> source,
													 Func<TSource, TKey> keySelector,
													 bool isMin)
	{ //method from later version of .NET (modified)
		if (source is null)
			throw new ArgumentNullException(nameof(source));

		if (keySelector is null)
			throw new ArgumentNullException(nameof(keySelector));

		using IEnumerator<TSource> e = source.GetEnumerator();

		if (!e.MoveNext())
			throw new InvalidOperationException("Sequence contains no elements");

		IComparer<TKey> comparer = Comparer<TKey>.Default;
		TSource value = e.Current,
				firstValue,
				nextValue;
		TKey key = keySelector(value),
			 nextKey;
		if (default(TKey) is null){
			if (key is null){
				firstValue = value;
				do{
					if (!e.MoveNext()) // All keys are null, surface the first element.
						return firstValue;

					value = e.Current;
					key = keySelector(value);
				}
				while (key is null);
			}

			while (e.MoveNext()){
				nextValue = e.Current;
				nextKey = keySelector(nextValue);
				if (nextKey is null || !Compare())
					continue;
				key = nextKey;
				value = nextValue;
			}
		}
		else{
			while (e.MoveNext()){
				nextValue = e.Current;
				nextKey = keySelector(nextValue);
				if (!Compare())
					continue;
				key = nextKey;
				value = nextValue;
			}
		}

		return value;

		bool Compare()
		{
			return isMin
				? comparer.Compare(nextKey, key) < 0
				: comparer.Compare(nextKey, key) > 0;
		}
	}
}

public static class ActionExtensions
{
	public static void SafeInvoke(this Action action) { action?.Invoke(); }

	public static void SafeInvoke<T>(this Action<T> action, T arg) { action?.Invoke(arg); }

	public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2) { action?.Invoke(arg1, arg2); }
}

public class LazySingletonWrapperOf<TClass>
	where TClass : class, new()
{
	private static readonly Lazy<LazySingletonWrapperOf<TClass>> _instance
		= new(() => new());

	private readonly Lazy<TClass> _class = new(() => new());

	protected static TClass Instance => _instance.Value._class.Value;
}
}