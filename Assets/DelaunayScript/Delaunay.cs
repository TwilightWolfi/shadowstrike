using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DelaunayVoronoi
{
    public class DelaunayTriangulator
    {	
		private double MaxX { get; set; }
		private double MaxY { get; set; }
		private Point point0;
		private Point point1;
		private Point point2;
		private Point point3;
		private List<Point> points;
		private Triangle tri1;
		private Triangle tri2;
		private List<Triangle> border;
		
	
		public DelaunayTriangulator (double MaxX, double MaxY)
		{
			this.MaxX = MaxX;
			this.MaxY = MaxY;
			this.point0 = new Point(0, 0);
			this.point1 = new Point(0, MaxY);
			this.point2 = new Point(MaxX, MaxY);
			this.point3 = new Point(MaxX, 0);
			this.points = new List<Point>() { point0, point1, point2, point3 };
			this.tri1 = new Triangle(point0, point1, point2);
			this.tri2 = new Triangle(point0, point2, point3);
			this.border = new List<Triangle>() { tri1, tri2 };
		}
		
        public IEnumerable<Triangle> BowyerWatson(List<Point> pointsInput)
        {
            var triangulation = new HashSet<Triangle>(border);
            List<Point> mirrorOfPoints = pointsInput;
            foreach (var point in mirrorOfPoints)
            {
                var badTriangles = FindBadTriangles(point, triangulation);
                var polygon = FindHoleBoundaries(badTriangles);

                foreach (var triangle in badTriangles)
                {
                    foreach (var vertex in triangle.Vertices)
                    {
                        vertex.AdjacentTriangles.Remove(triangle);
                    }
                }
                triangulation.RemoveWhere(o => badTriangles.Contains(o));

                foreach (var edge in polygon.Where(possibleEdge => possibleEdge.point1 != point && possibleEdge.point2 != point))
                {
                    var triangle = new Triangle(point, edge.point1, edge.point2);
                    triangulation.Add(triangle);
                }
            }
            //triangulation.Remove(border[0]);
			//triangulation.Remove(border[1]); //since the border is necessary for calculations or some shit *WHY NOT JUST DO AWAY WITH IT AFTER CALCULATIONS ARE DONE **WHAT WAS I THINKING***
            List<Triangle> triangulation_butnotfucked = new List<Triangle>();
            foreach(Triangle itsprobablyfucked in triangulation)
            {
                bool comparisonResult = true;
                for(int i = 0; i<itsprobablyfucked.Vertices.Length; i++)
                {
                    if(itsprobablyfucked.Vertices[i] == point0 || itsprobablyfucked.Vertices[i] == point1 || itsprobablyfucked.Vertices[i] == point2 || itsprobablyfucked.Vertices[i] == point3)
                    {
                        comparisonResult = false;
                    }
                }
                if(comparisonResult)
                {
                    triangulation_butnotfucked.Add(itsprobablyfucked);
                }
            }
            return triangulation_butnotfucked;
        }

        private List<Edge> FindHoleBoundaries(ISet<Triangle> badTriangles)
        {
            var edges = new List<Edge>();
            foreach (var triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            var grouped = edges.GroupBy(o => o);
            var boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
            return boundaryEdges.ToList();
        }

        private ISet<Triangle> FindBadTriangles(Point point, HashSet<Triangle> triangles)
        {
            var badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
            return new HashSet<Triangle>(badTriangles);
        }
    }
}