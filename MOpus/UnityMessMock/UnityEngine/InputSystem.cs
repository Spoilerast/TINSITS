namespace UnityEngine.InputSystem;

public sealed class InputAction
{
	internal bool inProgress;

	public struct CallbackContext
	{ }

	public event Action<CallbackContext> performed
	{
		add => throw new NotImplementedException();
		remove => throw new NotImplementedException();
	}

	public event Action<CallbackContext> started
	{
		add => throw new NotImplementedException();
		remove => throw new NotImplementedException();
	}
	
}
public class PlayerInput : MonoBehaviour
{
	internal Action<PlayerInput> onControlsChanged;
}
