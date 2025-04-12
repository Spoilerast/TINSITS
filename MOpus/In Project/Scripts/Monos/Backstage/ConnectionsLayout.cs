using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Backstage;
using NotMonos.Databases;
using NotMonos.Previews;
using UnityEngine;

namespace Monos.Systems
{
internal sealed class ConnectionsLayout : SceneSystem
{
	[SerializeField] private Color[] _colors;
	[SerializeField] private Material _linkMaterial;

	private readonly Queue<GameObject> _links = new(50);
	private Transform _linksParent;

	private void OnEnable()
	{
		if (!_linksParent)
			this.GameObjectNamed("links", out _linksParent);
	}

	internal void DestroyLinks()
	{
		while (_links.Count > 0)
			Destroy(_links.Dequeue());
	}

	[ContextMenu("make links")] //todo: remove?
	internal void MakeLinks()
	{
		PeekLogger.LogName();
		DestroyLinks();
		List<(bool, IEnumerable<(GridPoint, GridPoint)>)> all = DataCenter.Connections
																		  .GetAllNetsLinks().ToList();

		for (var i = 0; i < all.Count; i++){
			bool connected = all[i].Item1;
			foreach ((GridPoint first, GridPoint second) in all[i].Item2)
				MakeAndPlaceLink(i, connected, first, second);
		}
	}

	internal void SubscribeOnMove(PreviewsController previews)
		=> previews.UnitMoved += MakeLinks;

	private void MakeAndPlaceLink(int colorIndex, bool connected, GridPoint first, GridPoint second)
	{
		GameObject obj = new();
		var line = obj.AddComponent<LineRenderer>();

		line.startColor = line.endColor = _colors[colorIndex];
		line.textureMode = connected
			? LineTextureMode.Stretch
			: LineTextureMode.Tile;
		line.SetPosition(0, first.ToVector3());
		line.SetPosition(1, second.ToVector3());
		line.material = _linkMaterial;
		line.startWidth = 0.4f;

		if (SceneGlobals.InEditor)
			obj.name = $"{first}--{second}";

		obj.transform.SetParent(_linksParent);

		_links.Enqueue(obj);

		//=> string.Format("({0:f1} , {1:f1})<=>({2:f1} , {3:f1})", first.X, first.Z, second.X, second.Z);
	}

	[ContextMenu("Generate colors")]
	private void RandColors()
	{
		_colors = new Color[20];
		for (var i = 0; i < _colors.Length; i++)
			_colors[i] = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
	}
}
}