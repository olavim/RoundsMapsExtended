using UnityEngine;
using UnboundLib;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Operation.Union;
using MapsExt.Geometry;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace MapsExt.Editor.UI
{
	public class SmoothLineRenderer : MonoBehaviour
	{
		public int cornerVertexCount = 8;
		public float lineWidth = 0.1f;
		public MeshRenderer Renderer { get; private set; }

		private GeometryFactory geometryFactory;

		protected virtual void Awake()
		{
			this.Renderer = this.gameObject.GetOrAddComponent<MeshRenderer>();
			this.Renderer.material = new Material(Shader.Find("Sprites/Default"))
			{
				color = new Color(1f, 1f, 1f, 0.1f)
			};

			this.gameObject.GetOrAddComponent<MeshFilter>();
			this.geometryFactory = new GeometryFactory(PrecisionModel.FloatingSingle.Value);
		}

		public void SetPositions(List<Vector3> points)
		{
			var filter = this.gameObject.GetComponent<MeshFilter>();
			filter.mesh = null;

			if (points.Count < 2)
			{
				return;
			}

			Mesh newMesh = null;

			float width = this.lineWidth;
			while (newMesh == null && width >= 0.05f)
			{
				var circles = points.ConvertAll(p => this.CreateCircle(p, width));
				var hull = this.CreateConcaveHull(circles);
				var triangulator = new Triangulator(hull);
				newMesh = triangulator.GetMesh();
				width -= 0.01f;
			}

			filter.mesh = newMesh;
		}

		private Polygon CreateCircle(Vector3 pos, float width)
		{
			var coords = new List<Coordinate>();
			var widthVertex = new Vector3(0, width / 2f, 0);
			float anglePerVertex = 360f / this.cornerVertexCount;

			for (int j = 0; j < this.cornerVertexCount; j++)
			{
				var rotation = Quaternion.Euler(0, 0, j * anglePerVertex);
				var p = pos + (rotation * widthVertex);
				coords.Add(new Coordinate(p.x, p.y));
			}

			coords.Add(coords[0]);

			var shell = new LinearRing(coords.ToArray());
			return new Polygon(shell, this.geometryFactory);
		}

		private Polygon CreateConcaveHull(List<Polygon> shapes)
		{
			var lines = new List<NetTopologySuite.Geometries.Geometry>();

			for (int i = 0; i < shapes.Count - 1; i++)
			{
				var hull = ConvexHull.Create(new Polygon[] { shapes[i], shapes[i + 1] });
				lines.Add(hull);
			}

			return (Polygon) CascadedPolygonUnion.Union(lines);
		}
	}
}