using UnityEngine;
using UnboundLib;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Operation.Union;
using Sebastian.Geometry;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace MapsExt.UI
{
	public class SmoothLineRenderer : MonoBehaviour
	{
		public int cornerVertexCount = 12;
		public float lineWidth = 0.2f;

		private GeometryFactory geometryFactory;
		private MeshRenderer renderer;

		public void Awake()
		{
			this.renderer = this.gameObject.GetOrAddComponent<MeshRenderer>();
			this.renderer.material = new Material(Shader.Find("Sprites/Default"));
			this.renderer.material.color = new Color(1f, 1f, 1f, 0.01f);

			this.gameObject.GetOrAddComponent<MeshFilter>();
			this.geometryFactory = new GeometryFactory(PrecisionModel.FloatingSingle.Value);
		}

		public void SetPositions(List<Vector3> points)
		{
			var filter = this.gameObject.GetComponent<MeshFilter>();

			if (points.Count < 2)
			{
				filter.mesh = null;
				return;
			}

			var circles = points.Select(this.CreateCircle).ToList();
			var hull = this.CreateConcaveHull(circles);

			var shapes = new List<Shape>();

			var unionExterior = new Shape();
			unionExterior.points = hull.ExteriorRing.Coordinates.Select(c => new Vector3((float) c.X, 0, (float) c.Y)).ToList();
			unionExterior.points.RemoveAt(unionExterior.points.Count - 1);
			shapes.Add(unionExterior);

			for (int i = 0; i < hull.NumInteriorRings; i++)
			{
				var shape = new Shape();
				shape.points = hull.InteriorRings[i].Coordinates.Select(c => new Vector3((float) c.X, 0, (float) c.Y)).ToList();
				shape.points.RemoveAt(shape.points.Count - 1);
				shapes.Add(shape);
			}

			filter.mesh = new CompositeShape(shapes).GetMesh();
			filter.mesh.vertices = filter.mesh.vertices.Select(v => new Vector3(v.x, v.z, 0)).ToArray();
		}

		private Polygon CreateCircle(Vector3 pos)
		{
			var coords = new List<Coordinate>();
			var widthVertex = new Vector3(0, this.lineWidth / 2f, 0);
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
			var lines = new List<Geometry>();

			for (int i = 0; i < shapes.Count - 1; i++)
			{
				var hull = ConvexHull.Create(new Polygon[] { shapes[i], shapes[i + 1] });
				lines.Add(hull);
			}

			return (Polygon) CascadedPolygonUnion.Union(lines);
		}
	}
}
