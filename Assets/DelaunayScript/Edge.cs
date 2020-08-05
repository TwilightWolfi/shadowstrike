namespace DelaunayVoronoi
{
    public class Edge
    {
        public Point point1 { get; }
        public Point point2 { get; }

        public Edge(Point point1, Point point2)
        {
            this.point1 = point1;
            this.point2 = point2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var edge = obj as Edge;

            var samePoints = point1 == edge.point1 && point2 == edge.point2;
            var samePointsReversed = point1 == edge.point2 && point2 == edge.point1;
            return samePoints || samePointsReversed;
        }

        public override int GetHashCode()
        {
            int hCode = (int)point1.x ^ (int)point1.y ^ (int)point2.x ^ (int)point2.y;
            return hCode.GetHashCode();
        }
    }
}