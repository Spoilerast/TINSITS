using System.Collections.Generic;
using Extensions;
using NotMonos.Databases;

namespace NotMonos.Previews
{
internal class DirectionInfo
{
	internal readonly GridPoint direction;
	internal readonly ClusterInfo[] infos;

	internal DirectionInfo(GridPoint direction, IEnumerable<ClusterInfo> infos)
	{
		(this.direction, this.infos) = (direction, infos.CastToArray());
	}

	internal DirectionInfo(GridPoint direction, ClusterInfo info)
	{
		this.direction = direction;
		infos = new[] { info };
	}
}
}