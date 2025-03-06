namespace Inputs
{
	internal sealed class InputsWrapper : Extensions.LazySingletonWrapperOf<InputActions>
	{
		private InputsWrapper()
		{ }

		public static InputActions Actions => Instance;
	}
}