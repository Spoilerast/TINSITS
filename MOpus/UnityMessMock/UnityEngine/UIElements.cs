using System.Diagnostics.Tracing;
using Extensions;

namespace UnityEngine.UIElements;

public delegate void EventCallback<in TEventType>(TEventType evt);

public enum TrickleDown
{
	//     The event handler should be executed during the AtTarget and BubbleUp phases.
	NoTrickleDown,

	//     The event handler should be executed during the AtTarget and TrickleDown phases.
	TrickleDown
}

public static class UQueryExtensions
{
	public static T Q<T>(this VisualElement e, string name = null, string className = null) where T : VisualElement
	{ throw new NotImplementedException(); }
}

public class Button : VisualElement
{
	internal string name;

	internal void Focus()
	{
		throw new NotImplementedException();
	}
}

public sealed class ClickEvent
{
	internal Button currentTarget;
}

public class FloatField : VisualElement
{
	internal float value;
}

public class FocusEvent
{
	internal Button currentTarget;
}

public class Foldout : VisualElement
{
	internal void Focus()
	{
		throw new NotImplementedException();
	}
}

public class GroupBox : VisualElement
{
	internal bool visible;
}

public class IntegerField : VisualElement
{
	internal ushort value;
}

public class Label : VisualElement
{
	internal string text;
}

public class MouseEnterEvent
{ }

public class MouseLeaveEvent
{ }

public class NavigationSubmitEvent
{ }

public class RadioButtonGroup : VisualElement
{
	internal int value;
}

public class UIDocument
{
	internal VisualElement rootVisualElement;

	public static implicit operator bool(UIDocument v) => v != null;
}

public class VisualElement
{
	internal void RegisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown) where TEventType : EventBase<TEventType>, new()
	{
		throw new NotImplementedException();
	}

	internal void RegisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TUserArgsType? userArgs, TrickleDown useTrickleDown) where TEventType : EventBase<TEventType>, new()
	{
		throw new NotImplementedException();
	}

	internal void UnregisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown) where TEventType : EventBase<TEventType>, new()
	{
		throw new NotImplementedException();
	}
}

public abstract class EventBase<T>
{ }

public class VisualTreeAsset:Object { }