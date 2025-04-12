using Extensions;
using NotMonos.Databases;
using UnityEngine;

namespace Monos.Scene
{
public abstract class SceneObject : MonoBehaviour
{ /*any kind of visible objects on scene*/
	protected Vector3 Position => transform.position;

	internal GridPoint PositionAsPoint => new(transform.position.x, transform.position.z);

	public virtual void DestroySceneObject()
		=> Destroy();

	internal void Activate()
		=> gameObject.SetActive(true);

	internal void Deactivate()
		=> gameObject.SetActive(false);

	internal void Move(GridPoint newPosition)
		=> transform.position = newPosition.ToVector3();

	protected void Destroy()
		=> Destroy(gameObject);
}
}