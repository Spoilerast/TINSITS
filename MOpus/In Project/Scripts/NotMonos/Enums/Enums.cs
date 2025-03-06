namespace NotMonos
{
	internal enum ClusterStatus : byte
	{ NotClustered, Clustered, SuperClustered }

	internal enum ClusterType : byte
	{ _3, _7 }

	internal enum Connectability : byte
	{ Unconnectable, Connectable, PowerSourceConnection }

	internal enum Prev_Cluster_Side : byte //todo rename
	{ NotASide, Forward, Back, BackRight, ForwardRight, BackLeft, ForwardLeft }

	internal enum PreviewType : byte
	{ Create, Destroy }

	internal enum PrismType : byte
	{ _L, _R, _C }

	internal enum SceneState : byte
	{ Error, Default, SpawnMode, PreviewMode, SubPreviewMode, NotInteractable }
}