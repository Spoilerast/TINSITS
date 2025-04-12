using NotMonos.Processors;
using UnityEngine;

namespace Monos.Scene
{
internal sealed class PowerSource : SceneObject
{
	[SerializeField] private byte _team = 1;

	internal void Initialize() { LoadProcessor.AddPowerSourceOnGrid(_team, PositionAsPoint); }

	internal void Initialize(byte team)
	{
		_team = team;
		Initialize();
	}
}
}