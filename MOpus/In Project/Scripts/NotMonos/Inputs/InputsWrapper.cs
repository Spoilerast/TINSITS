using Extensions;

namespace Inputs
{
internal sealed class InputsWrapper : LazySingletonWrapperOf<InputActions>
{
	private InputsWrapper() {}

	public static InputActions Actions => Instance;
}
}