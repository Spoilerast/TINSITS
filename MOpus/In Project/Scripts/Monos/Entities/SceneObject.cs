namespace Monos.Scene
{
	public abstract class SceneObject : UnityEngine.MonoBehaviour
	{   /*any kind of visible objects on scene*/
		public UnityEngine.Vector3 Position => transform.position;

		internal NotMonos.Databases.GridPoint PositionAsPoint => new(transform.position.x, transform.position.z);

		public virtual void DestroySceneObject()
			=> Destroy();

		internal void Activate()
			=> gameObject.SetActive(true);

		internal void Deactivate()
			=> gameObject.SetActive(false);

		internal void Move(NotMonos.Databases.GridPoint newPosition)
			=> transform.position = newPosition.ToVector3;

		protected void Destroy()
			=> Destroy(gameObject);
	}
}