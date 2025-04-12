#if UNITY_EDITOR
#define IN_EDITOR
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Monos;
using NotMonos.Databases;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Extensions.PeekLogger;
using static UnityEngine.Object;
using Object = UnityEngine.Object;

namespace Extensions
{
public interface ISearchable {}

public static class NotMonosExtensions
{
	internal static Vector2 ToVector2(this GridPoint p)
		=> new(p.X, p.Z);

	internal static Vector3 ToVector3(this GridPoint p)
		=> new(p.X, 0, p.Z);
}

public static class UnityExtensions
{
	public static void FindFirstVisualElement<T>(this UIDocument document,
												 out T visualElement)
		where T : VisualElement
	{
		if (!document)
			UIDocumentNullErrorLog();

		visualElement = document.rootVisualElement.Q<T>();
		if (visualElement == null)
			VisualElementNullErrorLog<T>();
	}

	public static void FindFirstVisualElement<Source, Result>(this Source sourceElement,
															  out Result visualElement)
		where Source : VisualElement
		where Result : VisualElement
	{
		if (sourceElement == null)
			VisualElementNullErrorLog<Result>();

		visualElement = sourceElement.Q<Result>();
		if (visualElement == null)
			VisualElementNullErrorLog<Result>();
	}

	public static void FindVisualElement<V>(this UIDocument document,
											in string name,
											out V visualElement)
		where V : VisualElement
	{
		if (!document)
			UIDocumentNullErrorLog();

		visualElement = document.rootVisualElement.Q<V>(name);
		if (visualElement == null)
			VisualElementNullErrorLog<V>();
	}

	public static void FindVisualElement<T, V>(this UIDocument document,
											   in T name,
											   out V visualElement)
		where V : VisualElement
		where T : Enum
	{
		if (!document)
			UIDocumentNullErrorLog();

		visualElement = document.rootVisualElement.Q<V>(name.ToString());
		if (visualElement == null)
			VisualElementNullErrorLog<V>();
	}

	public static void FindVisualElement<Source, Result>(this Source sourceElement,
														 in string name,
														 out Result visualElement)
		where Source : VisualElement
		where Result : VisualElement
	{
		if (sourceElement == null)
			VisualElementNullErrorLog<Result>();

		visualElement = sourceElement.Q<Result>(name);
		if (visualElement == null)
			VisualElementNullErrorLog<Result>();
	}

	public static void FindVisualElement<Source, EnumName, Result>(this Source sourceElement,
																   in EnumName name,
																   out Result visualElement)
		where Source : VisualElement
		where Result : VisualElement
		where EnumName : Enum
	{
		if (sourceElement == null)
			VisualElementNullErrorLog<Result>();

		visualElement = sourceElement.Q<Result>(name.ToString());
		if (visualElement == null)
			VisualElementNullErrorLog<Result>();
	}

	public static GameObject GameObjectNamed<TBehaviour>(this TBehaviour script, string name, out Transform transform)
		where TBehaviour : Behaviour
	{
		transform = null;
		if (!script){
			BehaviourNullErrorLog<TBehaviour>();
			return null;
		}

		GameObject gameObject =
#if IN_EDITOR
			new(name);
#else
				new();
#endif
		transform = gameObject.transform;
		return gameObject;
	}

	public static UIDocument GetUIDocument(this SceneUI sceneUI)
		=> sceneUI.GetComponent<UIDocument>();

	public static void HideElement(this VisualElement element)
		=> element.style.display = DisplayStyle.None;

	public static bool IsComponentNull<TBehaviour, UnityComponent>(this TBehaviour script, UnityComponent component)
		where TBehaviour : Behaviour
		where UnityComponent : Component
	{
		if (!script){
			BehaviourNullErrorLog<TBehaviour>();
			return true;
		}

		if (!component){
			EditorGUIUtility.PingObject(script);
			LogComponentNullError<UnityComponent>(script.gameObject.name);
			return true;
		}

		return false;
	}

	public static bool IsFieldValueNull<TBehaviour, FieldValue>(this TBehaviour script,
																FieldValue fieldValue)
		where TBehaviour : Behaviour
		where FieldValue : Object
	{
		if (!script){
			BehaviourNullErrorLog<TBehaviour>();
			return true;
		}

		if (!fieldValue){
			EditorGUIUtility.PingObject(script);
			LogFieldValueError<FieldValue>(script.gameObject.name);
			return true;
		}

		return false;
	}

	public static VisualElement RegisterCallbackElement<TEventType>(this VisualElement element,
																	EventCallback<TEventType> callback,
																	TrickleDown useTrickleDown
																		= TrickleDown.NoTrickleDown)
		where TEventType : EventBase<TEventType>, new()
	{
		if (element == null)
			VisualElementNullErrorLog<VisualElement>();

		element!.RegisterCallback(callback, useTrickleDown);
		return element;
	}

	public static VisualElement RegisterCallbackElement<TEventType, TUserArgsType>(this VisualElement element,
																				   EventCallback<TEventType, TUserArgsType> callback,
																				   TUserArgsType userArgs,
																				   TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
		where TEventType : EventBase<TEventType>, new()
	{
		if (element == null)
			VisualElementNullErrorLog<VisualElement>();

		element!.RegisterCallback(callback, userArgs, useTrickleDown);
		return element;
	}

	public static void ShowElement(this VisualElement element)
		=> element.style.display = DisplayStyle.Flex;

	public static Vector3 ToGridVector(this Vector2 v)
		=> new(v.x, 0f, v.y);

	public static bool TryFindAllInterfaces<TBehaviour, TInterface>(this TBehaviour script,
																	out IEnumerable<TInterface> interfaces)
		where TBehaviour : Behaviour
		where TInterface : ISearchable
	{
		interfaces = Enumerable.Empty<TInterface>();
		if (!script){
			BehaviourNullErrorLog<TBehaviour>();
			return false;
		}

		IQueryable<MonoBehaviour> activeBehaviours =
			FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
				.Where(mb => mb.isActiveAndEnabled)
				.AsQueryable();

		IQueryable<TInterface> q_interfaces = activeBehaviours.OfType<TInterface>();
		if (!interfaces.Any()){
			InterfaceNotFoundErrorLog<TInterface>(script.gameObject.name, typeof(TBehaviour).ToString());
			return false;
		}

		interfaces = q_interfaces;
		return true;
	}

	public static bool TryFindObject<T>(out T var)
		where T : Object
	{
		var = FindFirstObjectByType<T>();
		return var == null
			? throw new NullReferenceException($"Cannot find {nameof(T)}")
			: true;
	}

	public static bool TryFindObjectIfNull<T>(ref T var)
		where T : Object
	{
		if (var)
			return true;

		var = FindFirstObjectByType<T>();
		return var == null
			? throw new NullReferenceException($"Cannot find {nameof(T)}")
			: true;
	}

	public static bool TryFindSingleInterface<TBehaviour, TInterface>(this TBehaviour script,
																	  out TInterface @interface)
		where TBehaviour : Behaviour
		where TInterface : ISearchable
	{
		@interface = default;
		if (!script){
			BehaviourNullErrorLog<TBehaviour>();
			return false;
		}

		IQueryable<MonoBehaviour> activeBehaviours =
			FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
				.Where(mb => mb.enabled)
				.AsQueryable();

		IQueryable<TInterface> interfaces = activeBehaviours.OfType<TInterface>();
		if (!interfaces.Any()){
			InterfaceNotFoundErrorLog<TInterface>(script.gameObject.name, typeof(TBehaviour).ToString());
			return false;
		}

		@interface = interfaces.Single();
		return !Equals(@interface, default(TInterface));
	}

	public static bool TryFindSingleInterfaceIfNull<TBehaviour, TInterface>(this TBehaviour script,
																			ref TInterface @interface)
		where TBehaviour : Behaviour
		where TInterface : ISearchable
	{
		if (@interface is not null)
			return true;

		return TryFindSingleInterface(script, out @interface);
	}

	public static bool TryGetComponent_InAttachedParentalChild<TBehaviour, TUnityComponent>(this TBehaviour script,
																							out TUnityComponent
																								componentExpectedInParentalChild)
		where TBehaviour : Behaviour
		where TUnityComponent : Component
	{
		componentExpectedInParentalChild = null;

		if (!script){
			BehaviourNullErrorLog<TBehaviour>();
			return false;
		}

		GameObject gameObject = script.gameObject;
		if (!gameObject){
			ObjectNullErrorLog();
			return false;
		}

		componentExpectedInParentalChild = gameObject.GetComponentInChildren<TUnityComponent>();
		if (!componentExpectedInParentalChild){
			ChildrenComponentNullErrorLog<TUnityComponent>(gameObject.name);
			return false;
		}

		return true;
	}

	public static bool TryGetComponentIfNull<TBehaviour, TComponent>(this TBehaviour script,
																	 ref TComponent component)
		where TBehaviour : Behaviour
		where TComponent : Component
	{
		if (component)
			return true;

		if (!script){
			BehaviourNullErrorLog<TBehaviour>();
			return false;
		}

		GameObject gameObject = script.gameObject;
		if (gameObject)
			return gameObject.TryGetComponent(out component);

		ObjectNullErrorLog();
		return false;
	}

	public static VisualElement UnregisterCallbackElement<TEventType>(this VisualElement element,
																	  EventCallback<TEventType> callback,
																	  TrickleDown useTrickleDown
																		  = TrickleDown.NoTrickleDown)
		where TEventType : EventBase<TEventType>, new()
	{
		if (element == null)
			VisualElementNullErrorLog<VisualElement>();

		element!.UnregisterCallback(callback, useTrickleDown);
		return element;
	}

	public static VisualElement UnregisterCallbackElement<TEventType, TUserArgsType>(this VisualElement element,
																					 EventCallback<TEventType, TUserArgsType> callback,
																					 TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
		where TEventType : EventBase<TEventType>, new()
	{
		if (element == null)
			VisualElementNullErrorLog<VisualElement>();

		element!.UnregisterCallback(callback, useTrickleDown);
		return element;
	}

	internal static T InstantiateWithParent<T>(T original, Transform parent)
		where T : Object
		=> Instantiate(original, parent);

	internal static T InstantiateXZ<T>(this SceneSystem _, T original, float xPosition, float zPosition)
		where T : Object
		=> Instantiate(original, new Vector3(xPosition, 0, zPosition), default);

	internal static bool TryInstantiate<T>(T original, GridPoint position, out T instantiated)
		where T : Object
	{
		instantiated = null;
		if (LogWarningForReturn(position, "Position is null or not valid"))
			return false;

		if (LogWarningForReturn(original, $"{typeof(T)} is null or not valid"))
			return false;

		instantiated = Instantiate(original, position.ToVector3(), default);
		return true;
	}

	internal static bool TryInstantiate<T>(this SceneSystem _, T original, GridPoint position, out T instantiated)
		where T : Object
		=> TryInstantiate(original, position, out instantiated);

	//todo need global remake for error logging to make it more generic

	[Conditional("UNITY_EDITOR")] //todo perhaps Conditional attribute have no reason anymore
	private static void BehaviourNullErrorLog<TBehaviour>()
		where TBehaviour : Behaviour
	{
		LogError($"{typeof(TBehaviour)} was null");
	}

	[Conditional("UNITY_EDITOR")]
	private static void ChildrenComponentNullErrorLog<TUnityComponent>(in string parentName)
		where TUnityComponent : Component
	{
		LogError($"Component {typeof(TUnityComponent)} not found in children of parent GameObject ({parentName})");
	}

	[Conditional("UNITY_EDITOR")]
	private static void InterfaceNotFoundErrorLog<TInterface>(string gameObjectName, string scriptName)
		where TInterface : ISearchable
	{
		LogError($"[{gameObjectName}, {scriptName}]: Interface of type {typeof(TInterface)} not found");
	}

	[Conditional("UNITY_EDITOR")]
	private static void LogComponentNullError<TUnityComponent>(string gameObjectName)
		where TUnityComponent : Component
	{
		LogError($"[{gameObjectName}]: Component {typeof(TUnityComponent)} is not set (null)");
	}

	private static void LogFieldValueError<FieldValue>(string gameObjectName)
		where FieldValue : Object
	{
		LogError($"[{gameObjectName}]: Field value type of {typeof(FieldValue)} is not set (null)");
	}

	[Conditional("UNITY_EDITOR")]
	private static void ObjectNullErrorLog() { LogError("Expected parent GameObject is null"); }

	private static void UIDocumentNullErrorLog() { LogError("UIDocument was null"); }

	private static void VisualElementNullErrorLog<VResult>()
		where VResult : VisualElement
	{
		LogError($"{typeof(VResult)} was null");
	}
}
}