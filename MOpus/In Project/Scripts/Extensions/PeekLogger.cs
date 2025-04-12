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
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Extensions
{
public static class PeekLogger
	//todo make common abstract class. make heritor UnityLogger and move all this code there (for compatibility with other engines)
{
	//private const string CollectionSeparator = "§";
	//private const string TupleSeparator = "¥";
	private const string CollectionSeparator = "<b><color=green>,</b></color> ";
	private const string CollectionStart = "<color=green>[</color> ";
	private const string CollectionEnd = " <color=green>]</color>";
	private const string TupleSeparator = "<b><color=purple>,</b></color> ";
	private const string TupleStart = "<color=purple>[</color> ";
	private const string TupleEnd = " <color=purple>]</color>";
	private const string ColorStartPrefix = "<color=";
	private const string ColorStartSuffix = ">";
	private const string ColorEnd = "</color> ";

	private const string TypesSeparator = ", ";

	internal static void ClearLog()
	{
		Assembly.GetAssembly(typeof(Editor))?
			.GetType("UnityEditor.LogEntries")?
			.GetMethod("Clear")?
			.Invoke(null, null);
	}

	internal static void ExitPlaymode() { EditorApplication.ExitPlaymode(); }

	internal static void Log(object o)
	{
#if LOGGING_DISABLED
			return;
#endif
		Debug.Log(o);
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
		var objectString = "Method is empty!";
		CallerInfo callerInfo = GetCallerInfo(ref objectString);
		StringBuilder builder = CallerInfoBuilder(callerInfo);
		LogUnity(builder);
	}

	internal static void LogError(object o)
	{
#if LOGGING_DISABLED
			return;
#endif
		Debug.LogError(o);
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
		var stringBuilder = new StringBuilder();
		_ = stringBuilder.Append(message);
		LogItems(stringBuilder, collection);
	}

	internal static void LogItemsTabThis<T>(this IEnumerable<T> collection) { LogItems("\t", collection); }

	internal static void LogItemsThis<T>(this IEnumerable<T> collection) { LogItems(collection); }

	internal static void LogItemsVarious(params object[] items)
	{
#if LOGGING_DISABLED
			return;
#endif
		if (items.Length == 0)
			return;

		StringBuilder builder = new();
		foreach (object item in items){ //todo rework
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
		Debug.Log(o);
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
		var objectString = message?.ToString();
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
		//todo for now it works right only if params equals called method parameters. redo or remove
	{
		if (parameters.Length == 0)
			return;

		MethodBase method = new StackFrame(1, true).GetMethod();
		ParameterInfo[] parameterInfos = method.GetParameters();
		StringBuilder builder = new();

		for (var i = 0; i < parameters.Length; i++){
			_ = builder.Append($"{parameterInfos[i].Name}: {parameters[i]}");
			if (i < parameters.Length - 1)
				_ = builder.Append("<color=green><b>,</b></color> ");
		}

		LogUnity(builder);
	}

	internal static void LogParamsWithTypes(params object[] parameters)
		//todo for now it works right only if params equals called method parameters. redo or remove
	{
		if (parameters.Length == 0)
			return;

		MethodBase method = new StackFrame(1, true).GetMethod();
		ParameterInfo[] parameterInfos = method.GetParameters();
		StringBuilder builder = new();

		for (var i = 0; i < parameters.Length; i++){
			_ = builder
				.Append($"<color=red>{parameterInfos[i].ParameterType}</color> {parameterInfos[i].Name}: {parameters[i]}");
			if (i < parameters.Length - 1)
				_ = builder.Append("<color=green><b>,</b></color> ");
		}

		Debug.Log(builder.ToString());
	}

	internal static void LogPause(object o)
	{
		Debug.Break();
		Debug.Log(o);
	}

	internal static void LogTab(object v) { LogMessageTab(v); }

	internal static void LogTabTab(object v) { LogMessageTabTab(v); }

	internal static void LogTabTabThis(this object obj) { LogTabTab(obj); }

	internal static void LogTabThis(this object obj) { LogTab(obj); }

	internal static void LogThis(this object obj) { Log(obj); }

	internal static void LogWarning(object v)
	{
#if LOGGING_DISABLED
			return;
#endif
		Debug.LogWarning(v);
	}

	/// <summary>
	///     Checks condition which is important to be True for right code execution.
	///     If "returnCondition" is True returns False. Else - returns True and push warning message to Log.
	/// </summary>
	/// <param name="returnCondition" >Need to be True to move next, False to break execution (early exit)</param>
	/// <param name="warningMessage" >Message what is wrong, will be sent to Log</param>
	/// <returns>True when you need return from method. False when code can proceed.</returns>
	internal static bool LogWarningForReturn(bool returnCondition, string warningMessage)
	{
#if LOGGING_DISABLED
			return !returnCondition;
#endif
		if (returnCondition)
			return false;

		Debug.LogWarning(warningMessage);
		return true;
	}

	internal static void Pause() { Debug.Break(); }

	private static void AppendBuilderItem<T>(T item, StringBuilder stringBuilder, Type itemType = null, bool isCollectionElement = true)
	{
		itemType ??= item.GetType();

		if (item is null || EqualityComparer<T>.Default.Equals(item, default)){
			_ = stringBuilder.Append($"default({itemType.Name})");
			return;
		}

		if (itemType == typeof(string)){
			_ = stringBuilder.Append(string.IsNullOrEmpty(item as string)
										 ? "\"\""
										 : item);
			return;
		}

		if (item is IEnumerable<T> nestedCollection){
			AppendCollection(null, nestedCollection, stringBuilder, itemType);
			return;
		}

		if (IsTuple(itemType)){
			AppendTuple(itemType, item, stringBuilder, isCollectionElement);
			return;
		}

		_ = stringBuilder.Append(item);
	}

	private static void AppendCollection<T>(StringBuilder typeNames,
											IEnumerable<T> collection,
											StringBuilder stringBuilder,
											Type genericType)
	{ //todo think about merging two StringBuilders

		// ReSharper disable once PossibleMultipleEnumeration
		typeNames ??= GetCollectionTypeNames(collection, out genericType);
		WrapWithColor(stringBuilder, Colors.red, typeNames);
		// ReSharper disable once PossibleMultipleEnumeration
		StringBuilder itemsCollection = CreateRichCollection(collection, CollectionSeparator, genericType);
		_ = stringBuilder
			.Append(CollectionStart)
			.Append(itemsCollection)
			.Append(CollectionEnd);
	}

	private static void AppendTuple<T>(Type itemType, T item, StringBuilder stringBuilder, bool isCollectionElement)
	{
		if (!isCollectionElement){
			IEnumerable<string> types = itemType.GenericTypeArguments
												.Where(x => !x.IsArray)
												.Select(x => x.Name);
			StringBuilder tupleTypes = CreateSimpleCollection(types, TypesSeparator);
			WrapWithColor(stringBuilder, Colors.orange, tupleTypes);
		}

		IEnumerable<object> values = itemType.GetFields().Select(field => field.GetValue(item));
		StringBuilder valuesCollection = CreateRichCollection(values, TupleSeparator);

		_ = stringBuilder.Append(TupleStart)
						 .Append(valuesCollection)
						 .Append(TupleEnd);
	}

	private static StringBuilder BuildNames(Type collectionType, Type genericType)
	{
		StringBuilder builder = new();
		IEnumerable<string> typeNames = genericType.GenericTypeArguments.Select(x => x.Name).ToArray();
		bool isHaveTypeNames = typeNames.Any();

		if (!collectionType.IsNested){
			if (collectionType.GenericTypeArguments.Length == 0){
				if (!isHaveTypeNames)
					builder.Append(collectionType.Name.Replace("[]", ""));
				else
					builder.Append(CreateSimpleCollection(typeNames, TypesSeparator));
			}
			else{
				if (!isHaveTypeNames)
					typeNames = collectionType.GenericTypeArguments.Select(x => x.Name);

				builder.Append(CreateSimpleCollection(typeNames, TypesSeparator));
			}

			return builder;
		}

		if (genericType.GenericTypeArguments.Length == 0)
			return builder.Append(genericType.Name);

		StringBuilder builtCollectionStream = CreateSimpleCollection(typeNames, TypesSeparator);
		string mainName = collectionType.Name;
		if (mainName.Contains('`'))
			return builder.Append(builtCollectionStream);

		int index = mainName.IndexOf('>');
		if (index >= 1)
			mainName = mainName[1..index];

		builder.Append(mainName)
			   .Append(" => ")
			   .Append(builtCollectionStream);
		return builder;
	}

	private static StringBuilder CallerInfoBuilder(CallerInfo info)
	{
		StringBuilder builder = new();
		_ = builder
			.Append(info.className)
			.Append(" called ")
			.Append(info.methodName)
			.Append(". ");

		if (info.message != null)
			_ = builder.Append(info.message);

		return builder;
	}

	private static StringBuilder CreateRichCollection<T>(IEnumerable<T> collection, string separator, Type genericType = null)
	{
		int separatorSize = separator.Length;
		StringBuilder builder = new();
		foreach (T item in collection){
			AppendBuilderItem(item, builder, genericType);
			builder.Append(separator);
		}

		builder.Remove(builder.Length - separatorSize, separatorSize);
		return builder;
	}

	private static StringBuilder CreateSimpleCollection<T>(IEnumerable<T> collection, string separator)
	{
		int separatorSize = separator.Length;
		StringBuilder builder = new();
		foreach (T item in collection)
			builder.Append(item)
				   .Append(separator);

		builder.Remove(builder.Length - separatorSize, separatorSize);
		return builder;
	}

	private static CallerInfo GetCallerInfo(ref string message)
	{
		const int maxFramesToCheck = 5;
		var stackTrace = new StackTrace(1, false);
		int loopLimit = Math.Min(stackTrace.FrameCount, maxFramesToCheck);

		for (var i = 0; i < loopLimit; i++){
			StackFrame frame = stackTrace.GetFrame(i);
			MethodBase method = frame?.GetMethod();
			if (method?.DeclaringType == null)
				continue;

			if (method.DeclaringType == typeof(PeekLogger))
				continue;

			string className = method.DeclaringType?.Name ?? "UnknownClass";
			string methodName = method.Name;
			return new(ref className,
					   ref methodName,
					   ref message);
		}

		return default;
	}

	private static StringBuilder GetCollectionTypeNames<T>(IEnumerable<T> collection, out Type genericType)
	{
		Type collectionType = collection.GetType();
		genericType = typeof(T);

		return BuildNames(collectionType, genericType);
	}

	private static bool IsTuple(Type type)
		=> type.IsGenericType && (type.Name.StartsWith("ValueTuple") || type.Name.StartsWith("Tuple"));

	private static void LogItems<T>(StringBuilder stringBuilder, IEnumerable<T> collection)
	{
		stringBuilder ??= new();
		_ = stringBuilder.Append('\t');
		LogItemsBuilder(collection, stringBuilder);
	}

	private static void LogItemsBuilder<T>(IEnumerable<T> collection, StringBuilder stringBuilder)
	{
		if (LogWarningForReturn(collection != null, $"{stringBuilder} collection was null"))
			return;

		if (LogWarningForReturn(collection.Any(), $"{stringBuilder} empty collection"))
			return;

		StringBuilder typeNames = GetCollectionTypeNames(collection, out Type genericType);
		T[] enumerable = collection.CastToArray();

		stringBuilder ??= new();

		if (enumerable.Length == 1){
			AppendBuilderItem(enumerable[0], stringBuilder, genericType, false);
			LogUnity(stringBuilder);
			return;
		}

		AppendCollection(typeNames, enumerable, stringBuilder, genericType);
		LogUnity(stringBuilder);
	}

	private static void LogMessageObjects(string message, params object[] objects)
	{
		if (objects.Length == 0){
			message += " !no objects here!";
			LogMessage(message);
			return;
		}

		StringBuilder builder = new();
		MessageWithObjects(builder, ref message, ref objects);
		Debug.Log(builder.ToString());
	}

	private static void LogUnity(StringBuilder stringBuilder) { Debug.Log(stringBuilder.ToString()); }

	private static void LogUnityWithPrefix(in string prefix, ref object o) { Debug.Log(prefix + o); }

	private static void MessageWithObjects(StringBuilder builder, ref string message, ref object[] objects)
	{
		_ = builder.Append(message)
				   .Append(". ");
		int end = objects.Length - 1;
		for (var i = 0; i <= end; i++){
			object item = objects[i];
			_ = builder.Append(item);
			if (i != end)
				_ = builder.Append("<color=green><b>,</b></color> ");
		}
	}

	private static void WrapWithColor(StringBuilder mainBuilder, Colors color, StringBuilder appendBuilder)
	{
		mainBuilder.Append(ColorStartPrefix)
				   .Append(color) //.ToString())
				   .Append(ColorStartSuffix)
				   .Append(appendBuilder)
				   .Append(ColorEnd);
	}

	private enum Colors
	{        // ReSharper disable once InconsistentNaming
		red, // ReSharper disable once InconsistentNaming
		orange
	}

	private readonly struct CallerInfo
	{
		public readonly string className;
		public readonly string message;
		public readonly string methodName;

		public CallerInfo(ref string className, ref string methodName, ref string message)
		{
			(this.className, this.methodName, this.message) = (className, methodName, message);
		}
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