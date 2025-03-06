using Monos.Scene;

namespace NotMonos.Entities
{
	internal sealed class Unit
	{
		internal Unit(Prism prism)
		{
			Prism = prism;
			Declusterize();
		}

		internal ClusterId ClusterId { get; private set; }

		internal Prism Prism { get; }

		internal ClusterStatus Status { get; private set; } 

		internal void Declusterize()
			=> (ClusterId, Status) = (ClusterId.NotInCluster, ClusterStatus.NotClustered);

		internal void Clusterize(ClusterId clusterId, ClusterStatus status)
			=> (ClusterId, Status) = (clusterId, status);
	}
}