namespace Monos.Scene
{
	internal sealed class PowerSource : SceneObject
	{
		[UnityEngine.SerializeField] private byte _team = 1;

		internal void Initialize() => NotMonos.Processors.
				SceneObjectsToEntitiesProcessor.AddPowerSourceOnGrid(_team, PositionAsPoint);

		internal void Initialize(byte team)
		{
			_team = team;
			Initialize();
		}
	}
}