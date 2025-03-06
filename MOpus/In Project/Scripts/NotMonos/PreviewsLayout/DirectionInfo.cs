using NotMonos.Databases;

namespace NotMonos.PreviewsLayout
{
	internal class DirectionInfo
	{
		internal readonly GridPoint direction;
		internal readonly ClusterInfo[] infos;

		internal DirectionInfo(GridPoint direction, params ClusterInfo[] infos)
			=> (this.direction, this.infos) = (direction, infos);
	}
}