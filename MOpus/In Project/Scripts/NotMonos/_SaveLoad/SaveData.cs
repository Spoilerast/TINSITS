using System;
using System.Collections.Generic;
using Extensions;
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

	public SaveData(CameraData camera,
					IEnumerable<PowerSourceData> powerSources,
					IEnumerable<ClusterData> clusters,
					IEnumerable<UnitData> unitData)
	{
		(units, this.clusters, this.powerSources, this.camera, saveVersion)
			= (new(unitData), new(clusters), new(powerSources), camera, Constants.CurrentSaveVersion);
	}

	internal SaveData(string fromJson) { JsonUtility.FromJsonOverwrite(fromJson, this); }

	public IEnumerable<ClusterData> Clusters => clusters;

	public IEnumerable<UnitData> Units => units;

	public IEnumerable<PowerSourceData> PowerSources => powerSources;

	public string ToJson => JsonUtility.ToJson(this, true);

	public Vector3 CameraPosition()
	{
		(float x, float z) = camera.GetData;
		return new(x, 0, z);
	}

	public override string ToString()
	{
		saveVersion.LogThis();
		CameraPosition().LogThis();
		powerSources.LogItemsThis();
		clusters.LogItemsThis();
		units.LogItemsThis();
		return "";
	}

	//public void LoadFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
}
}