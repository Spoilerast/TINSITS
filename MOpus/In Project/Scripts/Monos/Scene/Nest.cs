using System;
using Extensions;
using NotMonos;
using NotMonos.Backstage;
using NotMonos.Databases;
using UnityEngine;

namespace Monos.Scene
{
[RequireComponent(typeof(SphereCollider))]
public sealed class Nest : InteractableObject
{
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
		switch (SceneGlobals.CurrentState){
			// instantiate prism to available Nest
			case SceneState.SpawnMode:
				OnPickedSpawn.SafeInvoke(PositionAsPoint);
				return;
			// pick Nests
			case SceneState.PreviewMode:
				OnPickedMove.SafeInvoke(PositionAsPoint); //todo maybe block invocation for previews scroll
				return;
		}
	}
}
}