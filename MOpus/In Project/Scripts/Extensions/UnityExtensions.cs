#if UNITY_EDITOR
#define IN_EDITOR
#else
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Monos;
using NotMonos.Databases;
using UnityEngine;
using UnityEngine.UIElements;
using static Extensions.PeekLogger;
using static UnityEngine.Object;

namespace Extensions
{
	public interface ISearchable
	{ }

	public static class UnityExtensions
	{
		public static void FindFirstVisualElement<V>(
			this UnityEngine.UIElements.UIDocument document,
			out V visualElement)
			where V : VisualElement
		{
			if (!document)
				UIDocumentNullErrorLog();

			visualElement = document.rootVisualElement.Q<V>();
			if (visualElement == null)
				VisualElementNullErrorLog<V>();
		}

		public static void FindFirstVisualElement<VSource, VResult>(
			this VSource sourceElement,
			out VResult visualElement)
			where VSource : VisualElement
			where VResult : VisualElement
		{
			if (sourceElement == null)
				VisualElementNullErrorLog<VResult>();

			visualElement = sourceElement.Q<VResult>();
			if (visualElement == null)
				VisualElementNullErrorLog<VResult>();
		}

		public static void FindVisualElement<V>(
			this UIDocument document,
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

		public static void FindVisualElement<V, T>(
			this UIDocument document,
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

		public static void FindVisualElement<VSource, VResult>(
			this VSource sourceElement,
			in string name,
			out VResult visualElement)
			where VSource : VisualElement
			where VResult : VisualElement
		{
			if (sourceElement == null)
				VisualElementNullErrorLog<VResult>();

			visualElement = sourceElement.Q<VResult>(name);
			if (visualElement == null)
				VisualElementNullErrorLog<VResult>();
		}

		public static void FindVisualElement<VSource, EnumName, VResult>(
			this VSource sourceElement,
			in EnumName name,
			out VResult visualElement)
			where VSource : VisualElement
			where VResult : VisualElement
			where EnumName : Enum
		{
			if (sourceElement == null)
				VisualElementNullErrorLog<VResult>();

			visualElement = sourceElement.Q<VResult>(name.ToString());
			if (visualElement == null)
				VisualElementNullErrorLog<VResult>();
		}

		public static GameObject GameObjectNamed<Behaviour>
			(this Behaviour script, string name, out Transform transform)
		where Behaviour : UnityEngine.Behaviour
		{
			transform = null;
			if (!script)
			{
				BehaviourNullErrorLog<Behaviour>();
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

		public static bool IsComponentNull<Behaviour, UnityComponent>
			(this Behaviour script, UnityComponent component)
		where Behaviour : UnityEngine.Behaviour
		where UnityComponent : Component
		{
			if (!script)
			{
				BehaviourNullErrorLog<Behaviour>();
				return true;
			}

			if (!component)
			{
				UnityEditor.EditorGUIUtility.PingObject(script);
				LogComponentNullError<UnityComponent>(script.gameObject.name);
				return true;
			}
			return false;
		}

		public static bool IsFieldValueNull<Behaviour, FieldValue>
										(this Behaviour script,
		FieldValue fieldValue)
		where Behaviour : UnityEngine.Behaviour
		where FieldValue : UnityEngine.Object
		{
			if (!script)
			{
				BehaviourNullErrorLog<Behaviour>();
				return true;
			}

			if (!fieldValue)
			{
				UnityEditor.EditorGUIUtility.PingObject(script);
				LogFieldValueError<FieldValue>(script.gameObject.name);
				return true;
			}
			return false;
		}

		public static VisualElement RegisterCallbackElement<TEventType>(
			this VisualElement element,
			EventCallback<TEventType> callback,
			TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
			where TEventType : EventBase<TEventType>, new()
		{
			if (element == null)
				VisualElementNullErrorLog<VisualElement>();

			element.RegisterCallback(callback, useTrickleDown);
			return element;
		}

		public static VisualElement RegisterCallbackElement<TEventType, TUserArgsType>(
			this VisualElement element,
			EventCallback<TEventType, TUserArgsType> callback,
			TUserArgsType userArgs,
			TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
			where TEventType : EventBase<TEventType>, new()
		{
			if (element == null)
				VisualElementNullErrorLog<VisualElement>();

			element.RegisterCallback(callback, userArgs, useTrickleDown);
			return element;
		}

		[Conditional("UNITY_EDITOR")]//todo redo via preprocessor
		public static void SetName<Behaviour>
			(this Behaviour script, object name)
			where Behaviour : UnityEngine.Behaviour
		{
			if (!script)
			{
				BehaviourNullErrorLog<Behaviour>();
				return;
			}

			GameObject g_o_ = script.gameObject;
			if (!g_o_)
			{
				ObjectNullErrorLog();
				return;
			}

			g_o_.name = name.ToString();
		}

		public static bool TryFindAllInterfaces<Behaviour, Interface>
			(this Behaviour script,
			out IEnumerable<Interface> interfaces)
			where Behaviour : UnityEngine.Behaviour
			where Interface : ISearchable
		{
			interfaces = Enumerable.Empty<Interface>();
			if (!script)
			{
				BehaviourNullErrorLog<Behaviour>();
				return false;
			}

			IQueryable<MonoBehaviour> activeBehaviours =
				FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
				.Where(mb => mb.isActiveAndEnabled)
				.AsQueryable();

			IQueryable<Interface> q_interfaces = activeBehaviours.OfType<Interface>();
			if (!interfaces.Any())
			{
				InterfaceNotFoundErrorLog<Interface>(script.gameObject.name, typeof(Behaviour).ToString());
				return false;
			}

			interfaces = q_interfaces;
			return true;
		}

		public static bool TryFindObject<T>(out T var)
			where T : UnityEngine.Object
		{
			var = FindFirstObjectByType<T>();
			return var == null
				? throw new NullReferenceException($"Cannot find {nameof(T)}")
				: true;
		}

		public static bool TryFindObjectIfNull<T>(ref T var)
			where T : UnityEngine.Object
		{
			if (var)
				return true;

			var = FindFirstObjectByType<T>();
			return var == null
				? throw new NullReferenceException($"Cannot find {nameof(T)}")
				: true;
		}

		public static bool TryFindSingleInterface<Behaviour, Interface>
					(this Behaviour script,
			out Interface @interface)
			where Behaviour : UnityEngine.Behaviour
			where Interface : ISearchable
		{
			@interface = default;
			if (!script)
			{
				BehaviourNullErrorLog<Behaviour>();
				return false;
			}

			IQueryable<MonoBehaviour> activeBehaviours =
				FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
				.Where(mb => mb.enabled)
				.AsQueryable();

			IQueryable<Interface> interfaces = activeBehaviours.OfType<Interface>();
			if (!interfaces.Any())
			{
				InterfaceNotFoundErrorLog<Interface>(script.gameObject.name, typeof(Behaviour).ToString());
				return false;
			}

			@interface = interfaces.Single();
			return !Equals(@interface, default(Interface));
		}

		public static bool TryFindSingleInterfaceIfNull<Behaviour, Interface>(
			this Behaviour script,
			ref Interface @interface)
			where Behaviour : UnityEngine.Behaviour
			where Interface : ISearchable
		{
			if (@interface is not null)
				return true;

			return TryFindSingleInterface(script, out @interface);
		}

		public static bool TryGetComponent_InAttachedParentalChild<Behaviour, UnityComponent>
			(this Behaviour script,
			out UnityComponent componentExpectedInParentalChild)
			where Behaviour : UnityEngine.Behaviour
			where UnityComponent : Component
		{
			componentExpectedInParentalChild = null;
			GameObject g_o_;

			if (!script)
			{
				BehaviourNullErrorLog<Behaviour>();
				return false;
			}

			g_o_ = script.gameObject;
			if (!g_o_)
			{
				ObjectNullErrorLog();
				return false;
			}

			componentExpectedInParentalChild = g_o_.GetComponentInChildren<UnityComponent>();
			if (!componentExpectedInParentalChild)
			{
				ChildrenComponentNullErrorLog<UnityComponent>(g_o_.name);
				return false;
			}

			return true;
		}

		public static bool TryGetComponentIfNull<Behaviour, Component>(
											this Behaviour script,
			ref Component component)
			where Behaviour : UnityEngine.Behaviour
			where Component : UnityEngine.Component
		{
			if (component)
				return true;

			if (!script)
			{
				BehaviourNullErrorLog<Behaviour>();
				return false;
			}

			GameObject g_o_ = script.gameObject;
			if (!g_o_)
			{
				ObjectNullErrorLog();
				return false;
			}

			return g_o_.TryGetComponent(out component);
		}

		public static VisualElement UnregisterCallbackElement<TEventType>(
																	this VisualElement element,
			EventCallback<TEventType> callback,
			TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
			where TEventType : EventBase<TEventType>, new()
		{
			if (element == null)
				VisualElementNullErrorLog<VisualElement>();

			element.UnregisterCallback(callback, useTrickleDown);
			return element;
		}

		public static VisualElement UnregisterCallbackElement<TEventType, TUserArgsType>(
			this VisualElement element,
			EventCallback<TEventType, TUserArgsType> callback,
			TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
			where TEventType : EventBase<TEventType>, new()
		{
			if (element == null)
				VisualElementNullErrorLog<VisualElement>();

			element.UnregisterCallback(callback, useTrickleDown);
			return element;
		}

		internal static T InstantiateXZ<T>(this SceneSystem _, T original, float xPosition, float zPosition)
			where T : UnityEngine.Object
			=> Instantiate(original, new Vector3(xPosition, 0, zPosition), default);


		internal static bool TryInstantiate<T>(T original, GridPoint position, out T instantiated)
			where T : UnityEngine.Object
		{
			instantiated = null;
			if (LogWarningForReturn(position, "Position is null or not valid"))
				return false;
			if (LogWarningForReturn(original, $"{typeof(T)} is null or not valid"))
				return false;

			instantiated = Instantiate(original, position.ToVector3, default);
			return true;
		}

		internal static bool TryInstantiate<T>(this SceneSystem _, T original, GridPoint position, out T instantiated)
			where T : UnityEngine.Object
			=> TryInstantiate(original, position, out instantiated);

		//todo need global remake for error logging to make it more generic

		[Conditional("UNITY_EDITOR")] //todo perhaps Conditional attribute have no reason anymore
		private static void BehaviourNullErrorLog<Behaviour>()
			where Behaviour : UnityEngine.Behaviour
			=> LogError($"{typeof(Behaviour)} was null");

		[Conditional("UNITY_EDITOR")]
		private static void ChildrenComponentNullErrorLog<UnityComponent>(in string parentName)
			where UnityComponent : Component
			=> LogError($"Component {typeof(UnityComponent)} not found in children of parent GameObject ({parentName})");

		[Conditional("UNITY_EDITOR")]
		private static void InterfaceNotFoundErrorLog<Interface>(string gameObjectName, string scriptName)
			where Interface : ISearchable
			=> LogError($"[{gameObjectName}, {scriptName}]: Interface of type {typeof(Interface)} not found");

		[Conditional("UNITY_EDITOR")]
		private static void LogComponentNullError<UnityComponent>(string gameObjectName)
			where UnityComponent : Component
			=> LogError($"[{gameObjectName}]: Component {typeof(UnityComponent)} is not set (null)");

		private static void LogFieldValueError<FieldValue>(string gameObjectName)
			where FieldValue : UnityEngine.Object
			=> LogError($"[{gameObjectName}]: Field value type of {typeof(FieldValue)} is not set (null)");

		[Conditional("UNITY_EDITOR")]
		private static void ObjectNullErrorLog()
			=> LogError("Expected parent GameObject is null");

		private static void UIDocumentNullErrorLog()
			=> LogError($"UIDocument was null");

		private static void VisualElementNullErrorLog<VResult>()
			where VResult : VisualElement
			=> LogError($"{typeof(VResult)} was null");
	}
}