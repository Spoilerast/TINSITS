using System;
using System.Collections.Generic;

namespace Extensions
{
	public static class Extensions
	{
		public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> collection, TSource exceptItem)
		{
			foreach (TSource item in collection)
				if (!item.Equals(exceptItem))
					yield return item;
		}

		public static void ForEach<TSource>(this IEnumerable<TSource> collection, Action<TSource> action)
		{
			foreach (TSource item in collection)
				action(item);
		}

		public static string JoinItemsToString<TSource>(this IEnumerable<TSource> collection)//todo make more generic or remove
			=> string.Join(", ", collection);

		public static void SafeInvoke(this Action action)
			=> action?.Invoke();

		public static void SafeInvoke<T>(this Action<T> action, T arg)
			=> action?.Invoke(arg);

		public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
			=> action?.Invoke(arg1, arg2);
	}

	public class LazySingletonWrapperOf<Class>
	where Class : class, new()
	{
		private static readonly Lazy<LazySingletonWrapperOf<Class>> _instance
			= new(() => new LazySingletonWrapperOf<Class>());

		private readonly Lazy<Class> _class = new(() => new Class());

		public static Class Instance => _instance.Value._class.Value;
	}
}