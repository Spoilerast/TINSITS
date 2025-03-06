using Extensions;
using NotMonos;
using NotMonos.Databases;
using NotMonos.Processors;
using System;

namespace Monos.Scene
{
	public abstract class InteractableObject : SceneObject
	{
		internal abstract void Interact();

		/*internal override void Interact() - interaction workflow
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