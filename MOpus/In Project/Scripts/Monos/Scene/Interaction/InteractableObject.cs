namespace Monos.Scene
{
public abstract class InteractableObject : SceneObject
{
	internal abstract void Interact();

	/*internal override void Interact()
	{
		switch (SceneGlobals.CurrentState)
		{
			*case SceneState.SpawnMode: // instatiate prism to available Nest
				return;

			*case SceneState.Default: // select Prism, attack Prism
				return;

			*case SceneState.PreviewMode: // pick Nests, Previews, attack Prism
				return;

			*case SceneState.SubPreviewMode: // pick only Previews
				return;

			default:
				return;
		}
	}*/
}
}