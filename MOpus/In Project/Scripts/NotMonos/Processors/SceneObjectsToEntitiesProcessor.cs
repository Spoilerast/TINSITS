using NotMonos.Databases;

namespace NotMonos.Processors
{
	internal sealed class SceneObjectsToEntitiesProcessor : Processor
	{
		private SceneObjectsToEntitiesProcessor()
		{ }

		internal static void AddPowerSourceOnGrid(byte team, GridPoint point)
		{
			TeamId teamId = TeamId.GetTeamId(team);
			Grid.AddPowerSource(teamId, point);
		}

		internal static UnitId AddPrism(Monos.Scene.Prism prism)
		{
			GridPoint position = prism.PositionAsPoint;
			UnitId unitId = UnitId.GetNewID();
			Units.AddUnit(unitId, prism);
			Grid.AddOnGrid(unitId, position);
			var (team, type, i, ca, cha, re) = prism;
			PrismProperties properties = new(team, type, i, ca, cha, re);
			Properties.Add(unitId, properties);

			return unitId;
		}
	}
}