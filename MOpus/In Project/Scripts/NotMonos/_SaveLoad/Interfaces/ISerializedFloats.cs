namespace NotMonos.SaveLoad
{
	internal interface ISerializedFloats //todo maybe better name
	{
		static float FromSerialized(in int i)
			=> i / 1000f;

		static int ToSerialized(in float i)
			=> (int)(i * 1000);
	}
}