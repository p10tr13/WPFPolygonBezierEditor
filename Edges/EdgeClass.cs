using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Point = System.Windows.Point;


namespace GK_Proj_1.Edges
{
    public class Edge
    {
        public RelationType type { get; set; }
        public Edge? p1Edge, p2Edge;
        public Point p1;
        public Point p2;

        public Edge(Point pnt1, Point pnt2)
        {
            p1 = pnt1;
            p2 = pnt2;
            type = RelationType.Regular;
        }

        public void Draw(DrawingContext dc)
        {
            Pen pen = new Pen(Brushes.CadetBlue, 3);
            dc.DrawLine(pen, p1, p2);
            dc.DrawEllipse(Brushes.Purple, null, p1, 4, 4);
        }

        public bool IsNearP1Vert(Point pt)
        {
            if ((p1 - pt).Length < 10)
            {
                return true;
            }
            return false;
        }

        public bool IsNearP2Vert(Point pt)
        {
            if ((p2 - pt).Length < 10)
            {
                return true;
            }
            return false;
        }

        public Point ClosestPointOnEdge(Point pt)
        {
            double LLS = Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2);
            if (LLS == 0.0)
                return new Point(-1, -1);
            double t = ((pt.X - p1.X) * (p2.X - p1.X) + (pt.Y - p1.Y) * (p2.Y - p1.Y)) / LLS;
            t = Math.Max(0, Math.Min(1, t));
            double closeX = p1.X + t * (p2.X - p1.X);
            double closeY = p1.Y + t * (p2.Y - p1.Y);
            return new Point(closeX, closeY);
        }

        public bool IsNearEdge(Point pt)
        {
            Point closestpt = ClosestPointOnEdge(pt);
            if (closestpt.X == -1 && closestpt.Y == -1)
                return false;
            return (closestpt - pt).Length < 10;
        }

        public virtual bool AdjustP1()
        {
            if (p1Edge == null)
                return false;
            p1 = p1Edge.p2;
            return true;
        }

        public virtual bool AdjustP2()
        {
            if (p2Edge == null)
                return false;
            p2 = p2Edge.p1;
            return true;
        }

        public virtual bool MoveP1To(Point pt)
        {
            p1 = pt;
            return p1Edge.AdjustP2();
        }
    }
}
