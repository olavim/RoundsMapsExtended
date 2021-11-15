using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;

namespace MapsExt.Geometry
{
	/* David Eberly, Triangulation by Ear Clipping, 2015
	 * https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
	 */
	public class Triangulator
	{
		private PolygonWrapper polygon;

		public Triangulator(Polygon polygon)
		{
			this.polygon = new PolygonWrapper(polygon);
		}

		public Mesh GetMesh()
		{
			if (!this.polygon.IsValid())
			{
				return null;
			}

			var verticesToClip = this.BuildVertexList();
			var triangles = new int[(verticesToClip.Count - 2) * 3];
			int triangleIndex = 0;

			while (verticesToClip.Count >= 3)
			{
				bool verticesRemoved = false;
				var node = verticesToClip.First;

				for (int i = 0; i < verticesToClip.Count; i++)
				{
					var prev = node.Previous ?? verticesToClip.Last;
					var next = node.Next ?? verticesToClip.First;

					// A convex vertex may be an ear
					if (node.Value.isConvex && this.IsEar(verticesToClip, node))
					{
						triangles[triangleIndex * 3 + 2] = prev.Value.index;
						triangles[triangleIndex * 3 + 1] = node.Value.index;
						triangles[triangleIndex * 3] = next.Value.index;
						triangleIndex++;

						verticesToClip.Remove(node);
						this.UpdateVertex(verticesToClip, prev);
						this.UpdateVertex(verticesToClip, next);

						verticesRemoved = true;
						break;
					}

					node = next;
				}

				if (!verticesRemoved)
				{
					return null;
				}
			}

			var mesh = new Mesh();
			mesh.vertices = this.polygon.vertices.Select(c => new Vector3((float) c.X, (float) c.Y, 0)).ToArray();
			mesh.triangles = triangles;

			return mesh;
		}

		private LinkedList<Vertex> BuildVertexList()
		{
			var vertices = new LinkedList<Vertex>();

			for (int i = 0; i < this.polygon.Exterior.vertices.Length; i++)
			{
				var vertex = new Vertex(i, this.polygon.Exterior.vertices[i]);
				vertices.AddLast(vertex);
			}

			for (var node = vertices.First; node != null; node = node.Next)
			{
				this.UpdateVertex(vertices, node);
			}

			for (int i = 0; i < this.polygon.Interiors.Length; i++)
			{
				var interior = this.polygon.Interiors[i];

				// Find the rightmost vertex of this interior and a vertex on the exterior that is visible from it
				var rightmostPoint = interior.vertices.Aggregate((a, b) => a.X > b.X ? a : b);
				var visibleVertex = this.FindMutuallyVisibleVertex(vertices, rightmostPoint);

				var visibleVertexNode = vertices.Find(visibleVertex);
				var currentVertexNode = visibleVertexNode;

				// Bridge the interior and exterior together
				int firstInteriorVertexIndex = Array.IndexOf(interior.vertices, rightmostPoint);
				for (int j = 0; j <= interior.vertices.Length; j++)
				{
					int interiorVertexIndex = (firstInteriorVertexIndex + j) % interior.vertices.Length;
					var vertex = new Vertex(interior.vertexOffset + interiorVertexIndex, interior.vertices[interiorVertexIndex]);
					currentVertexNode = vertices.AddAfter(currentVertexNode, vertex);
				}

				// The bridge point vertex must be duplicated
				var visibleVertexCopy = new Vertex(visibleVertex.index, visibleVertex.position);
				var lastNewVertexNode = vertices.AddAfter(currentVertexNode, visibleVertexCopy);

				for (var node = visibleVertexNode; node != (lastNewVertexNode.Next ?? vertices.First); node = (node.Next ?? vertices.First))
				{
					var next = node.Next ?? vertices.First;
					this.UpdateVertex(vertices, node);
				}
			}

			return vertices;
		}

		private Vertex FindMutuallyVisibleVertex(LinkedList<Vertex> vertices, Coordinate point)
		{
			var edges = new List<Edge>();
			for (var node = vertices.First; node != null; node = node.Next)
			{
				var next = node.Next ?? vertices.First;
				edges.Add(new Edge(node.Value, next.Value));
			}

			var ray = new LineSegment(point, new Coordinate(point.X + 1, point.Y));
			var intersectingEdges = edges
				.Select(e => new { edge = e, coord = e.IntersectLine(ray) })
				.Where(e => e.coord != null && e.coord.X > point.X)
				.ToList();

			intersectingEdges.Sort((a, b) => point.Distance(a.coord).CompareTo(point.Distance(b.coord)));

			var visibleEdge = intersectingEdges[0].edge;
			var intersectionPoint = intersectingEdges[0].coord;

			var candidateVertex = visibleEdge.first.position.X > visibleEdge.second.position.X ? visibleEdge.first : visibleEdge.second;
			var triangle = new Polygon(new LinearRing(new Coordinate[] { point, intersectionPoint, candidateVertex.position, point }));
			var locator = new IndexedPointInAreaLocator(triangle);

			var verticesInTriangle = vertices.Where(v => !v.isConvex && v != candidateVertex && locator.Locate(v.position) != Location.Exterior).ToList();

			return verticesInTriangle.Count == 0
				? candidateVertex
				: verticesInTriangle
						.Select(v => new { vertex = v, angle = Mathf.Abs((float) (v.position.Y - point.Y)) })
						.Aggregate((a, b) => a.angle < b.angle ? a : b)
						.vertex;
		}

		private void UpdateVertex(LinkedList<Vertex> vertices, LinkedListNode<Vertex> node)
		{
			var prevNode = node.Previous ?? vertices.Last;
			var nextNode = node.Next ?? vertices.First;

			var prevPoint = prevNode.Value.position;
			var nextPoint = nextNode.Value.position;
			var point = node.Value.position;

			// A vertex is convex if the angle formed by it and its adjacent vertices is less than 180 degrees
			node.Value.isConvex = Orientation.Index(prevPoint, nextPoint, point) == OrientationIndex.Clockwise;
		}

		private bool IsEar(LinkedList<Vertex> vertices, LinkedListNode<Vertex> node)
		{
			var prevNode = node.Previous ?? vertices.Last;
			var nextNode = node.Next ?? vertices.First;

			var prevPoint = prevNode.Value.position;
			var nextPoint = nextNode.Value.position;
			var point = node.Value.position;

			// A vertex is an ear if no point is contained within the triangle formed by it and its adjacent vertices
			var triangle = new Polygon(new LinearRing(new Coordinate[] { prevPoint, point, nextPoint, prevPoint }));
			var locator = new IndexedPointInAreaLocator(triangle);
			return vertices.All(v => locator.Locate(v.position) != Location.Interior);
		}

		private class Vertex
		{
			public int index;
			public Coordinate position;
			public bool isConvex;

			public Vertex(int index, Coordinate position)
			{
				this.index = index;
				this.position = position;
			}
		}

		private class Edge
		{
			public Vertex first;
			public Vertex second;
			public LineSegment Segment => new LineSegment(this.first.position, this.second.position);

			public Edge(Vertex first, Vertex second)
			{
				this.first = first;
				this.second = second;
			}

			public Coordinate IntersectLine(LineSegment line)
			{
				var coord = this.Segment.LineIntersection(line);

				bool intersects =
					coord != null &&
					((this.first.position.Y <= coord.Y && this.second.position.Y >= coord.Y) ||
					(this.first.position.Y >= coord.Y && this.second.position.Y <= coord.Y));

				return intersects ? coord : null;
			}
		}

		private class PolygonHull
		{
			public readonly Coordinate[] vertices;
			public readonly int vertexOffset;

			public PolygonHull(LineString ring, int vertexOffset)
			{
				var coords = ring.Coordinates.ToList();
				coords.RemoveAt(coords.Count - 1);

				this.vertices = coords.ToArray();
				this.vertexOffset = vertexOffset;
			}
		}

		private class PolygonWrapper
		{
			public readonly PolygonHull[] hulls;
			public readonly Coordinate[] vertices;

			public PolygonHull Exterior => this.hulls[0];
			public PolygonHull[] Interiors => this.hulls.Skip(1).ToArray();

			public PolygonWrapper(Polygon polygon)
			{
				var exterior = polygon.ExteriorRing;
				var interiors = polygon.InteriorRings;

				if (!Orientation.IsCCW(exterior.Coordinates))
				{
					exterior = (LineString) exterior.Reverse();
				}

				for (int i = 0; i < interiors.Length; i++)
				{
					if (Orientation.IsCCW(interiors[i].Coordinates))
					{
						interiors[i] = (LineString) interiors[i].Reverse();
					}
				}

				var sortedInteriors = interiors.ToList();

				// Sort interiors by their max x-coordinates, descending order
				sortedInteriors.Sort((a, b) => b.Coordinates.Max(c => c.X).CompareTo(a.Coordinates.Max(c => c.X)));

				var hullList = new List<PolygonHull>();
				hullList.Add(new PolygonHull(exterior, 0));
				foreach (var interior in sortedInteriors)
				{
					var prevHull = hullList[hullList.Count - 1];
					hullList.Add(new PolygonHull(interior, prevHull.vertexOffset + prevHull.vertices.Length));
				}

				this.hulls = hullList.ToArray();
				this.vertices = this.hulls.SelectMany(h => h.vertices).ToArray();
			}

			public bool IsValid()
			{
				for (int i = 0; i < this.vertices.Length; i++)
				{
					for (int j = 0; j < this.vertices.Length; j++)
					{
						if (i != j && this.vertices[i].Distance(this.vertices[j]) < 0.001f)
						{
							return false;
						}
					}
				}

				return true;
			}
		}
	}
}
