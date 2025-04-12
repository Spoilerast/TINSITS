using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Monos.Scene;
using NotMonos.Databases;
using UnityEngine;

namespace NotMonos.Backstage
{
internal sealed class NestPuddle //like pool, but small. get it?
{
	private const int NestsCount = 3;
	private readonly Dictionary<Nest, Collider> _nests;

	public NestPuddle(Nest nestPrefab)
	{
		_nests = new(NestsCount);
		PuddleInitialize(nestPrefab);
	}

	public void PreviewsScrolled(ClusterStatus status)
	{
		if (status == ClusterStatus.NotClustered)
			EnableColliders();
		else
			DisableColliders();
	}

	internal void DisableAll()
	{
		foreach (Nest item in _nests.Keys)
			item.Deactivate();

		EnableColliders();
	}

	internal void EnableNests(IEnumerable<GridPoint> positions)
	{
		PeekLogger.LogName();
		GridPoint[] positionsArray = positions.CastToArray();
		int end = positionsArray.Length;
		Nest nest;
		for (var i = 0; i < end; i++){
			nest = _nests.Keys.ElementAt(i);
			nest.Move(positionsArray[i]);
			nest.Activate();
		}
	}

	internal void SubscribeOnPickedTo(Action<GridPoint> action)
	{
		foreach (Nest item in _nests.Keys)
			item.OnPickedMove += action;
	}

	private void DisableColliders()
	{
		foreach (Collider item in _nests.Values)
			item.enabled = false;
	}

	private void EnableColliders()
	{
		foreach (Collider item in _nests.Values)
			item.enabled = true;
	}

	private void PuddleInitialize(Nest nestPrefab)
	{
		nestPrefab.GameObjectNamed("nests", out Transform parent);
		Nest nest;
		for (var i = 1; i <= NestsCount; i++){
			nest = UnityExtensions.InstantiateWithParent(nestPrefab, parent);
			_nests.Add(nest, nest.GetComponent<Collider>());
			if (SceneGlobals.InEditor)
				nest.name = $"Nest {i}";
			nest.Deactivate();
		}
	}
}
}