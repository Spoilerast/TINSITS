using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Monos.EditorSystems
{
	public sealed class RoundGrid : EditorSystem //dis code is too old for all dis s..
	{
		//public bool turnOnTooltips;
		//public bool reloadTooltips = false;
		public bool turnOnGrid;

		public bool turnOnGridCoords;

		[Range(.5f, 8f)]
		public float gridStep = 2;

		[Range(1, 60)]
		public int gridRadiusX = 10;

		[Range(1, 60)]
		public int gridRadiusZ = 10;

		private (int x, int z) _gridRadius;
		private float _gridStep;
		public Vector3Int gridOffset = new(10, 0, 11);
		private IEnumerable<Vector3> _vecs;
		private IEnumerable<Vector3> _posnts;

		private void OnDrawGizmos()
		{
			/*if (turnOnTooltips)
            {
                _vecs ??= GetVectors;
                //GUI.color = Color.black;
                //GUI.contentColor = Color.cyan;
                foreach (var i in _vecs)
                {
                    Vector3 j = i;
                    j.y = .5f;
                    Vector3 k = i;
                    k.x += .5f;
                    k.z += -.5f;
                    Handles.color = Color.cyan;
                    Handles.DrawLine(j, k);
                    Handles.Label(k, $"{i.x:f2}, {i.z:f2}");
                }
            }
            if (reloadTooltips)
            {
                _vecs = null;
                _vecs = FindObjectOfType<Ghost>() is not null
                    ? GetVectors.Append(FindObjectOfType<Ghost>().gameObject.transform.position)
                    : GetVectors;
                reloadTooltips = false;
            }*/
			if (turnOnGrid)
			{
				DrawGrid();
			}
		}

		private void DrawGrid()
		{
			_posnts ??= GetGridPositions();
			if (_gridRadius.x != gridRadiusX || _gridRadius.z != gridRadiusZ || _gridStep != gridStep)
			{ _posnts = GetGridPositions(); }

			GUI.color = Color.yellow;
			Handles.color = new Color(.3f, .3f, .3f, 1);
			foreach (var i in _posnts)
			{
				Vector3 ii = i + gridOffset;
				Handles.DrawWireDisc(ii, Vector3.up, .5f);
				if (turnOnGridCoords)
					Handles.Label(ii, $"{ii.x:f2}, {ii.z:f2}");
			}
		}

		private List<Vector3> GetGridPositions()
		{
			_gridRadius = (gridRadiusX, gridRadiusZ);
			_gridStep = gridStep;
			//step are multiplied on 2
			float halfStep = gridStep;
			float quarterStep = gridStep * .5f;
			List<Vector3> positions = new();

			foreach (Vector3 center in GetCenters4Grid(halfStep))
			{
				foreach (Vector3 item in MakeHexagon(center, halfStep, quarterStep))
				{
					if (!positions.Contains(item))
					{ positions.Add(item); }
				}
			}
			return positions;
		}

		private List<Vector3> GetCenters4Grid(float half)
		{
			List<Vector3> list = new();
			bool swap = false;
			float step = gridStep * 2;
			float borderZ, borderX;
			borderX = gridRadiusX * gridStep;
			borderZ = gridRadiusZ * gridStep;
			for (float z = 0; z <= borderZ; z += step)
			{
				for (float x = 0; x <= borderX; x += step)
				{
					list.Add(new Vector3(
						swap ? x - borderX + half : x - borderX,
						gridOffset.y,
						z - borderZ));
				}
				swap = !swap;
			}
			return list;
		}

		private static IEnumerable<Vector3> MakeHexagon(Vector3 center, float half, float quarter)
		{
			yield return new Vector3(center.x + half, center.y, center.z + quarter);
			yield return new Vector3(center.x + half, center.y, center.z - quarter);
			yield return new Vector3(center.x - half, center.y, center.z + quarter);
			yield return new Vector3(center.x - half, center.y, center.z - quarter);
			yield return new Vector3(center.x, center.y, center.z + half + quarter);
			yield return new Vector3(center.x, center.y, center.z - (half + quarter));
		}

		//private IEnumerable<Vector3> GetVectors
		//    => from p in FindObjectsOfType<Prism>()
		//       select p.Position;
	}
}