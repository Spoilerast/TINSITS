using System;
using System.Collections.Generic;
using NotMonos.Databases;
using UnityEngine;

namespace NotMonos.SaveLoad
{
	[Serializable]
	public sealed class SaveData
	{
		[SerializeField] public string saveVersion;
		[SerializeField] public CameraData camera;
		[SerializeField] public List<PowerSourceData> powerSources;
		[SerializeField] public List<ClusterData> clusters;
		[SerializeField] public List<UnitData> units;

		public SaveData(
			CameraData camera,
			IEnumerable<PowerSourceData> powerSources,
			IEnumerable<ClusterData> clusters,
			IEnumerable<UnitData> unitDatas)
			=> (units, this.clusters, this.powerSources, this.camera, saveVersion)
			= (new(unitDatas), new(clusters), new(powerSources), camera, Constants.CurrentSaveVersion);

		internal SaveData(string fromJson)
			=> JsonUtility.FromJsonOverwrite(fromJson, this);

		public Vector3 CameraPosition()
		{
			var (x, z) = camera.GetData;
			return new(x, 0, z);
		}

		public IEnumerable<ClusterData> Clusters
			=> clusters;

		public IEnumerable<UnitData> Units
			=> units;

		public IEnumerable<PowerSourceData> PowerSources
			=> powerSources;

		public void LoadFromJson(string a_Json)
			=> JsonUtility.FromJsonOverwrite(a_Json, this);

		public string ToJson
			=> JsonUtility.ToJson(this, true);
	}
}