using System.ComponentModel;
using System.Reflection;
using NotMonos;
using NotMonos.SaveLoad;

namespace UnityEngine;

public enum FindObjectsSortMode
{
	//     Don't sort the objects.
	None,

	//     Sort the objects by InstanceID in ascending order.
	InstanceID
}

public enum LineTextureMode
{
	//     Map the texture once along the entire length of the line.
	Stretch,

	//     Repeat the texture along the line, based on its length in world units. To set
	//     the tiling rate, use Material.SetTextureScale.
	Tile,

	//     Map the texture once along the entire length of the line, assuming all vertices
	//     are evenly spaced.
	DistributePerSegment,

	//     Repeat the texture along the line, repeating at a rate of once per line segment.
	//     To adjust the tiling rate, use Material.SetTextureScale.
	RepeatPerSegment,

	//     Trails do not change the texture coordinates of existing points when they add
	//     or remove points.
	Static
}

public enum Space
{
	//     World coordinate space, relative to the origin point (0,0,0) of the x-, y-, and
	//     z-axes of the scene.
	World,

	//     The local coordinate system of a GameObject relative to its parent, including
	//     orientation.
	Self
}

public struct Color
{
	internal static Color yellow;
}

public struct Quaternion
{
	internal static object Euler(float xRotationMax, float y, float z)
	{
		throw new NotImplementedException();
	}
}

public struct RaycastHit
{
	internal Transform transform;
}

public struct Vector2
{
	internal float x;
	internal float y;
}

public struct Vector3
{
	internal static int back;
	internal static int left;
	internal static object up;
	internal static object zero;
	internal float x;
	internal float y;
	internal float z;
}
public struct Vector3Int
{ }

public static class JsonUtility
{
	internal static void FromJsonOverwrite(string fromJson, SaveData saveData)
	{
		throw new NotImplementedException();
	}

	internal static string ToJson(SaveData saveData, bool v)
	{
		throw new NotImplementedException();
	}
}

public static class Time
{
	internal static float deltaTime;
}

public class Application
{
	internal static string persistentDataPath;
}

public class Awaitable
{
	internal static async Task NextFrameAsync()
	{
		throw new NotImplementedException();
	}

	internal static async Task WaitForSecondsAsync(float seconds)
	{
		throw new NotImplementedException();
	}
}

public class Behaviour : Component
{
	internal GameObject gameObject;
}

public sealed class Camera : Behaviour
{
	internal static Camera main;
	internal int fieldOfView;

	internal object ScreenPointToRay(Vector3 mousePosition)
	{
		throw new NotImplementedException();
	}
}

public class Collider : Component
{
	internal bool enabled;
}

public class Component : Object
{
	public Transform transform => null;
}

public sealed class ContextMenu : Attribute
{
	public readonly string menuItem;

	public readonly int priority;
	public readonly bool validate;

	public ContextMenu(string itemName)
		: this(itemName, isValidateFunction: false)
	{
	}

	public ContextMenu(string itemName, bool isValidateFunction)
		: this(itemName, isValidateFunction, 1000000)
	{
	}

	public ContextMenu(string itemName, bool isValidateFunction, int priority)
	{
		menuItem = itemName;
		validate = isValidateFunction;
		this.priority = priority;
	}
}

public class ContextMenuItemAttribute : PropertyAttribute
{
	//     The name of the function that should be called.
	public readonly string function;

	//     The name of the context menu item.
	public readonly string name;
	public ContextMenuItemAttribute(string name, string function)
	{
		this.name = name;
		this.function = function;
	}
}
public sealed class DelayedAttribute : PropertyAttribute
{
	//     Attribute used to make a float, int, or string variable in a script be delayed.
	public DelayedAttribute()
	{
	}
}

public sealed class DisallowMultipleComponent : Attribute
{
}

public sealed class GameObject : Object
{
	internal bool isStatic;
	internal string name;
	internal Transform? transform;

	public T AddComponent<T>() where T : Component
	{ throw new NotImplementedException(); }

	public T GetComponentInChildren<T>()
	{
		return default;
	}

	internal void SetActive(bool v)
	{
		throw new NotImplementedException();
	}

	internal bool TryGetComponent<Component>(out Component component) where Component : UnityEngine.Component
	{
		throw new NotImplementedException();
	}
}

public class GUI
{
	internal static Color color;
}

public class Handles
{
	internal static Color color;

	internal static void DrawWireDisc(Vector3 ii, object up, float v)
	{
		throw new NotImplementedException();
	}

	internal static void Label(Vector3 ii, string v)
	{
		throw new NotImplementedException();
	}
}

public class HeaderAttribute : PropertyAttribute
{
	//     The header text.
	public readonly string header;

	//     Add a header above some fields in the Inspector.
	//   header:
	//     The header text.
	public HeaderAttribute(string header)
	{
		this.header = header;
	}
}

public class Input
{
	internal static Vector3 mousePosition;

	internal static Vector3 GetAxisRaw(string v)
	{
		throw new NotImplementedException();
	}
}

public sealed class LineRenderer
{ }

public class Material
{ }

public class MeshCollider : Collider
{
	internal bool enabled;
}

public class MeshRenderer
{
	internal object material;
}

public class MonoBehaviour : Behaviour
{
	public bool enabled => true;
	public bool isActiveAndEnabled => true;
}
public class Object
{
	internal string name;

	public static void Destroy(Object obj)
	{ }
	public static T FindFirstObjectByType<T>() where T : Object
	{
		throw new NotImplementedException();
	}

	public static T[] FindObjectsByType<T>(FindObjectsSortMode sortMode) where T : Object
	{
		throw new NotImplementedException();
	}

	public static implicit operator bool(Object v) => v != null;

	public static T Instantiate<T>(T original, Vector3 vector3, object value) where T : UnityEngine.Object
	{
		throw new NotImplementedException();
	}

	public static T Instantiate<T>(T original, Transform parent) where T : Object
	{
		throw new NotImplementedException();
	}

	public static T Instantiate<T>(T original) where T : Object
	{ throw new NotImplementedException(); }

	public T GetComponent<T>()
	{ throw new NotImplementedException(); }
}

public class Physics
{
	internal static bool Raycast(object v, out RaycastHit hit, float maxDistance)
	{
		throw new NotImplementedException();
	}
}

public abstract class PropertyAttribute : Attribute
{ }

public sealed class RangeAttribute : PropertyAttribute
{
	public readonly float max;
	public readonly float min;

	//     Attribute used to make a float or int variable in a script be restricted to a
	//     specific range.
	//   min:
	//     The minimum allowed value.
	//   max:
	//     The maximum allowed value.
	public RangeAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}

public sealed class RequireComponent : Attribute
{
	public Type m_Type0;

	public Type m_Type1;

	public Type m_Type2;

	public RequireComponent(Type requiredComponent)
	{
		m_Type0 = requiredComponent;
	}

	public RequireComponent(Type requiredComponent, Type requiredComponent2)
	{
		m_Type0 = requiredComponent;
		m_Type1 = requiredComponent2;
	}

	public RequireComponent(Type requiredComponent, Type requiredComponent2, Type requiredComponent3)
	{
		m_Type0 = requiredComponent;
		m_Type1 = requiredComponent2;
		m_Type2 = requiredComponent3;
	}
}

public class Rigidbody : Component
{ }

public sealed class Screen
{
	internal static int height;
	internal static int width;
}

public class SelectionBaseAttribute : Attribute
{
}

public sealed class SerializeField : Attribute
{
}

public class SpaceAttribute : PropertyAttribute
{ }

public class SphereCollider : Collider
{ }

public class TooltipAttribute : PropertyAttribute
{
	//     The tooltip text.
	public readonly string tooltip;

	//     Specify a tooltip for a field.
	//
	//     The tooltip text.
	public TooltipAttribute(string tooltip)
	{
		this.tooltip = tooltip;
	}
}

public class Transform : Component
{
	internal GameObject gameObject;
	internal Vector3 position;
	internal object rotation;

	public void SetParent(Transform p)
	{ }

	internal void Rotate(float rotationValue, int v1, int v2)
	{
		throw new NotImplementedException();
	}

	internal void SetPositionAndRotation(object zero, object value)
	{
		throw new NotImplementedException();
	}

	internal void Translate(Vector3 newPosition, Space self)
	{
		throw new NotImplementedException();
	}
}

internal class Debug
{
	internal static void Break()
	{
		throw new NotImplementedException();
	}

	internal static void Log(object o)
	{
		throw new NotImplementedException();
	}

	internal static void LogError(object o)
	{
		throw new NotImplementedException();
	}

	internal static void LogWarning(object v)
	{
		throw new NotImplementedException();
	}
}