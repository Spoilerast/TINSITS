using System.Collections.Generic;

namespace NotMonos.Databases
{
	internal sealed partial class ConnectionsDB
	{
		private sealed class NetNode
		{
			private readonly HashSet<ushort> _linksIds;

			public NetNode()
				=> _linksIds = new(6);

			internal IEnumerable<ushort> Links
				=> _linksIds;

			internal void Add(ushort linkId)
				=> _linksIds.Add(linkId);

			internal void RemoveLink(ushort linkId)
				=> _linksIds.Remove(linkId);
		}
	}
}