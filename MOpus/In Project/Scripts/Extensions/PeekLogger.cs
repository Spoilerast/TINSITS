#if UNITY_EDITOR
#else
#define LOGGING_DISABLED
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Extensions
{
	public static class PeekLogger
	//todo make common abstract class. make heritor UnityLogger and move all this code there (for compatibility with other engines)
	{
		private const string CollectionSeparator = "§";

		private const string TupleSeparator = "¥";

		internal static void ClearLog()
			=> Assembly.GetAssembly(typeof(UnityEditor.Editor))
				.GetType("UnityEditor.LogEntries")
				.GetMethod("Clear")
				.Invoke(null, null);

		internal static void ExitPlaymode()
			=> UnityEditor.EditorApplication.ExitPlaymode();

		internal static void Log(object o)
		{
#if LOGGING_DISABLED
			return;
#endif
			UnityEngine.Debug.Log(o);
		}

		internal static void Log<T>(in string message, IEnumerable<T> collection)
		{
#if LOGGING_DISABLED
			return;
#endif
			LogItems(message, collection);
		}

		internal static void LogEmptyMethod()
		{
#if LOGGING_DISABLED
			return;
#endif
			string objectString = "Method is empty!";
			CallerInfo callerInfo = GetCallerInfo(ref objectString);
			StringBuilder builder = CallerInfoBuilder(callerInfo);
			LogUnity(builder);
		}

		internal static void LogError(object o)
		{
#if LOGGING_DISABLED
			return;
#endif
			UnityEngine.Debug.LogError(o);
		}

		internal static void LogItems<T>(IEnumerable<T> collection)
		{
#if LOGGING_DISABLED
			return;
#endif
			LogItemsBuilder(collection, null);
		}

		internal static void LogItems<T>(in string message, IEnumerable<T> collection)
		{
#if LOGGING_DISABLED
			return;
#endif
			StringBuilder stringBuilder = new StringBuilder();
			_ = stringBuilder.Append(message);
			_ = stringBuilder.Append('\t');
			LogItems(stringBuilder, collection);
		}

		internal static void LogItemsThis<T>(this IEnumerable<T> collection) => LogItems(collection);

		internal static void LogItemsVarious(params object[] items)
		{
#if LOGGING_DISABLED
			return;
#endif
			if (items.Length == 0)
				return;

			StringBuilder builder = new();
			foreach (var item in items)
			{
				string typeName = item.GetType().Name;
				_ = builder.Append($"<color=red>{typeName}</color> <color=green>[</color>{item}<color=green>]</color> ");
			}
			LogUnity(builder);
		}

		internal static void LogMessage(object o)
		{
#if LOGGING_DISABLED
			return;
#endif
			UnityEngine.Debug.Log(o);
		}

		internal static void LogMessageObjectsTab(object o, params object[] objects)
		{
#if LOGGING_DISABLED
			return;
#endif
			LogMessageObjects($"\t\t{o}", objects);
		}

		internal static void LogMessageObjectsTabTab(object o, params object[] objects)
		{
#if LOGGING_DISABLED
			return;
#endif
			LogMessageObjects($"\t>\t\t{o}", objects);
		}

		internal static void LogMessageTab(object o)
		{
#if LOGGING_DISABLED
			return;
#endif
			LogUnityWithPrefix("\t", ref o);
		}

		internal static void LogMessageTabTab(object o)
		{
#if LOGGING_DISABLED
			return;
#endif
			LogUnityWithPrefix("\t>\t\t", ref o);
		}

		internal static void LogName(object message = null)
		{
#if LOGGING_DISABLED
			return;
#endif
			string objectString = message?.ToString();
			CallerInfo callerInfo = GetCallerInfo(ref objectString);
			StringBuilder builder = CallerInfoBuilder(callerInfo);
			LogUnity(builder);
		}

		internal static void LogNameCollection<T>(IEnumerable<T> collection)
		{
#if LOGGING_DISABLED
			return;
#endif
			string s = null;
			CallerInfo callerInfo = GetCallerInfo(ref s);
			StringBuilder builder = CallerInfoBuilder(callerInfo);
			LogItems(builder, collection);
		}

		internal static void LogParams(params object[] parameters)
		//todo for now it works right only if params equals called method parameteters. redo or remove
		{
			if (parameters.Length == 0)
				return;

			MethodBase method = new StackFrame(1, true).GetMethod();
			ParameterInfo[] parameterInfos = method.GetParameters();
			StringBuilder builder = new();

			for (int i = 0; i < parameters.Length; i++)
			{
				_ = builder.Append($"{parameterInfos[i].Name}: {parameters[i]}");
				if (i < parameters.Length - 1)
					_ = builder.Append("<color=green><b>,</b></color> ");
			}

			LogUnity(builder);
		}

		internal static void LogParamsWithTypes(params object[] parameters)
		//todo for now it works right only if params equals called method parameteters. redo or remove
		{
			if (parameters.Length == 0)
				return;

			MethodBase method = new StackFrame(1, true).GetMethod();
			ParameterInfo[] parameterInfos = method.GetParameters();
			StringBuilder builder = new();

			for (int i = 0; i < parameters.Length; i++)
			{
				_ = builder
					.Append($"<color=red>{parameterInfos[i].ParameterType}</color> {parameterInfos[i].Name}: {parameters[i]}");
				if (i < parameters.Length - 1)
					_ = builder.Append("<color=green><b>,</b></color> ");
			}

			UnityEngine.Debug.Log(builder.ToString());
		}

		internal static void LogPause(object o)
		{
			UnityEngine.Debug.Break();
			UnityEngine.Debug.Log(o);
		}

		internal static void LogTab(object v)
			=> LogMessageTab(v);

		internal static void LogTabTab(object v)
			=> LogMessageTabTab(v);

		internal static void LogTabTabThis(this object obj)
			=> LogTabTab(obj);

		internal static void LogTabThis(this object obj)
			=> LogTab(obj);

		internal static void LogThis(this object obj)
			=> Log(obj);

		internal static void LogWarning(object v)
		{
#if LOGGING_DISABLED
			return;
#endif
			UnityEngine.Debug.LogWarning(v);
		}

		/// <summary>
		/// Checks condition which is important to be True for right code execution.
		/// If "returnCondition" is True returns False. Else - returns True and push warning message to Log.
		/// </summary>
		/// <param name="returnCondition">Need to be True to move next, False to break execution (early exit)</param>
		/// <param name="warningMessage">Message what is wrong, will be sent to Log</param>
		/// <returns>True when you need return from method. False when code can procced.</returns>
		internal static bool LogWarningForReturn(bool returnCondition, string warningMessage)
		{
#if LOGGING_DISABLED
			return !returnCondition;
#endif
			if (returnCondition)
				return false;

			UnityEngine.Debug.LogWarning(warningMessage);
			return true;
		}

		internal static void Pause()
			=> UnityEngine.Debug.Break();

		private static void AppendBuilderItem<T>(T item, StringBuilder stringBuilder)
		{
			if (item is null || EqualityComparer<T>.Default.Equals(item, default))
			{
				_ = stringBuilder.Append($"default({typeof(T).Name})");
				return;
			}

			Type itemType = item.GetType();

			if (itemType == typeof(string))
			{
				_ = stringBuilder.Append(string.IsNullOrEmpty(item as string)
					? "\"\""
					: $"{item}");
				return;
			}

			if (item is IEnumerable<T> nestedCollection)
			{
				AppendCollection(nestedCollection, stringBuilder);
				return;
			}

			if (IsTuple(itemType))
			{
				AppendTuple(item, stringBuilder);
				return;
			}

			_ = stringBuilder.Append($"{item}");
		}

		private static void AppendCollection<T>(IEnumerable<T> collection, StringBuilder stringBuilder)
		{
			Type collectionType = collection.GetType();
			string typeName = collectionType.GetGenericArguments().Length > 0
				? collectionType.GetGenericArguments()[0].Name
				: collectionType.Name[..^2];

			_ = stringBuilder.Append($"<color=red>{typeName}</color> <color=green>[</color> ");

			foreach (T item in collection)
			{
				AppendBuilderItem(item, stringBuilder);
				_ = stringBuilder.Append(CollectionSeparator);
			}

			_ = stringBuilder
							.Remove(stringBuilder.Length - CollectionSeparator.Length, CollectionSeparator.Length)
							.Replace(CollectionSeparator, "<b><color=green>,</b></color> ")
							.Append(" <color=green>]</color>");
		}

		private static void AppendTuple(object tuple, StringBuilder stringBuilder)
		{
			var members = tuple.GetType().IsValueType
				? tuple.GetType().GetFields().Cast<MemberInfo>()
				: tuple.GetType().GetProperties().Cast<MemberInfo>();

			_ = stringBuilder.Append($"<color=purple>[</color> ");

			foreach (var member in members)
			{
				object value = member switch
				{
					FieldInfo field => field.GetValue(tuple),
					PropertyInfo property => property.GetValue(tuple),
					_ => null
				};

				AppendBuilderItem(value, stringBuilder);
				_ = stringBuilder.Append(TupleSeparator);
			}
			_ = stringBuilder
							.Remove(stringBuilder.Length - TupleSeparator.Length, TupleSeparator.Length)
							.Replace(TupleSeparator, "<b><color=purple>,</b></color> ")
							.Append($" <color=purple>]</color>");
		}

		private static StringBuilder CallerInfoBuilder(CallerInfo info)
		{
			StringBuilder builder = new();
			_ = builder
				.Append(info.ClassName)
				.Append(" called ")
				.Append(info.MethodName)
				.Append(". ");

			if (info.Message != null)
				_ = builder.Append(info.Message);

			return builder;
		}

		private static CallerInfo GetCallerInfo(ref string message)
		{
			const int maxFramesToCheck = 5;
			var stackTrace = new StackTrace(1, false);
			int loopLimit = Math.Min(stackTrace.FrameCount, maxFramesToCheck);

			for (int i = 0; i < loopLimit; i++)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				MethodBase method = frame?.GetMethod();
				if (method?.DeclaringType == null)
					continue;
				if (method.DeclaringType == typeof(PeekLogger))
					continue;

				string className = method.DeclaringType?.Name ?? "UnknownClass";
				string methodName = method.Name;
				return new(
					ref className,
					ref methodName,
					ref message
				);
			}
			return default;
		}

		private static bool IsTuple(Type type)
			=> type.IsGenericType &&
			   (type.Name.StartsWith("ValueTuple") || type.Name.StartsWith("Tuple"));

		private static void LogItems<T>(StringBuilder stringBuilder, IEnumerable<T> collection)
		{
			stringBuilder ??= new();

			_ = stringBuilder.Append('\t');
			LogItemsBuilder(collection, stringBuilder);
		}

		private static void LogItemsBuilder<T>(IEnumerable<T> collection, StringBuilder stringBuilder)
		{
			if (LogWarningForReturn(collection.Any(), "empty collection"))
				return;

			if (collection.Count() == 1)
			{
				Log(collection.First());
				return;
			}

			stringBuilder ??= new();

			AppendCollection(collection, stringBuilder);

			LogUnity(stringBuilder);
		}

		private static void LogMessageObjects(string message, params object[] objects)
		{
			if (objects.Length == 0)
			{
				message += " !no objects here!";
				LogMessage(message);
				return;
			}

			StringBuilder builder = new();
			MessageWithObjects(builder, ref message, ref objects);
			UnityEngine.Debug.Log(builder.ToString());
		}

		private static void LogUnity(StringBuilder stringBuilder)
			=> UnityEngine.Debug.Log(stringBuilder.ToString());

		private static void LogUnityWithPrefix(in string prefix, ref object o)
			=> UnityEngine.Debug.Log(prefix + o);

		private static void MessageWithObjects(StringBuilder builder, ref string message, ref object[] objects)
		{
			_ = builder.Append(message)
				.Append(". ");
			int end = objects.Length - 1;
			for (int i = 0; i <= end; i++)
			{
				object item = objects[i];
				_ = builder.Append(item);
				if (i != end)
				{
					_ = builder.Append("<color=green><b>,</b></color> ");
				}
			}
		}

		internal readonly struct CallerInfo
		{
			public readonly string ClassName;
			public readonly string Message;
			public readonly string MethodName;

			public CallerInfo(ref string className, ref string methodName, ref string message)
				=> (ClassName, MethodName, Message) = (className, methodName, message);
		}

		/*private static StringBuilder LogName(MethodBase method, object message)
		{
			string className = method.DeclaringType.Name,
				methodName = method.Name;
			StringBuilder builder = new();
			builder.Append(className);
			builder.Append(" called ");
			builder.Append(methodName);
			builder.Append(". ");

			if (message != null)
				builder.Append(message);

			return builder;
		}*/
	}
}