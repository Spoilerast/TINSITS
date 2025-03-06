using System;
using System.Collections.Generic;
using System.Linq;
using Monos.Scene;
using NotMonos.Databases;
using UnityEngine;

namespace NotMonos.PreviewsLayout
{
	internal sealed class NestPuddle //like pool, but small. get it?
	{
		private readonly Dictionary<Nest, Collider> _nests;

		internal NestPuddle(Nest[] nests)
		{
			_nests = new(3);
			int i = 0;
			foreach (var item in nests)
			{
				item.name = $"Nest {++i}";
				_nests.Add(item, item.GetComponent<Collider>());
			}
			DisableAll();
		}

		internal void DisableAll()
		{
			foreach (var item in _nests.Keys)
				item.Deactivate();

			EnableColliders();
		}

		internal void DisableColliders()
		{
			foreach (var item in _nests.Values)
				item.enabled = false;
		}

		internal void EnableColliders()
		{
			foreach (var item in _nests.Values)
				item.enabled = true;
		}

		internal void EnableNests(IEnumerable<GridPoint> positions)
		{
			var positionsArray = positions.ToArray();
			int end = positionsArray.Length;
			Nest nest;
			for (int i = 0; i < end; i++)
			{
				nest = _nests.Keys.ElementAt(i);
				nest.Move(positionsArray[i]);
				nest.Activate();
			}
		}

		internal void SubscribeOnPickedTo(Action<GridPoint> action)
		{
			foreach (var item in _nests.Keys)
				item.OnPickedMove += action;
		}
	}
}