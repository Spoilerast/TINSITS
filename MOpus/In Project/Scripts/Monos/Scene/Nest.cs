using System;
using Extensions;
using NotMonos;
using NotMonos.Databases;
using UnityEngine;

namespace Monos.Scene
{
	public sealed class Nest : InteractableObject
	{
		internal string name;

		internal event Action<GridPoint> OnPickedMove;

		internal event Action<GridPoint> OnPickedSpawn;

		/*internal void UnsubscribePicked()
		{
			OnPickedSpawn = null;
			OnPickedMove = null;
		}*/

		public override void DestroySceneObject()
		{
			OnPickedMove = null;
			OnPickedSpawn = null;
			Destroy();
		}

		internal override void Interact()
		{
			if (SceneGlobals.CurrentState == SceneState.SpawnMode) // instatiate prism to available Nest
				OnPickedSpawn.SafeInvoke(PositionAsPoint);
			else if (SceneGlobals.CurrentState == SceneState.PreviewMode) // pick Nests
				OnPickedMove.SafeInvoke(PositionAsPoint);//todo maybe block invokation for previews scroll
		}
	}
}