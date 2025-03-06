namespace NotMonos.Databases
{
	internal sealed class PrismProperties //move to PropertiesDB and make private?
	{
		internal PrismProperties(byte tid, byte type, float integrity, float capacity, float charge, float resistance)
		=> (Team, Type, Integrity, Capacity, Charge, Resistance) =
		(TeamId.GetTeamId(tid), (PrismType)type, integrity, capacity, charge, resistance);

		internal PrismProperties()
		{ }

		internal TeamId Team { get; set; } = TeamId.GetTeamId(1); //todo remove initialization

		internal PrismType Type { get; set; }

		//all properties orders must follow of I-Ka-Cha-Re rule
		internal float Integrity { get; set; } //todo generic class property? (for Observer)
												//or just use https://github.com/NickKhalow/EventValue
		internal float Capacity { get; set; }

		internal float Charge { get; set; }

		internal float Resistance { get; set; }

		public override string ToString()
		{
			return $"pp {Team},   tp{Type} i{Integrity} ca{Capacity} ch{Charge} r{Resistance}";
		}

		internal void Deconstruct(out TeamId teamId,
			out PrismType prismType,
			out float integrity,
			out float capacity,
			out float charge,
			out float resistance
			)
		{
			teamId = Team;
			prismType = Type;
			integrity = Integrity;
			capacity = Capacity;
			charge = Charge;
			resistance = Resistance;
		}
	}
}