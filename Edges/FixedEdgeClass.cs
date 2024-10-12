using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace GK_Proj_1.Edges
{
    public class FixedLenEdge : Edge
    {
        public FixedLenEdge(Point p1, Point p2) : base(p1, p2) { length = (p1 - p2).Length; }

        public double length { get; }

        public override bool MoveP1To(Point pt)
        {
            Point oldp1 = new Point(base.p1.X, base.p1.Y), oldp2 = new Point(base.p2.X, base.p2.Y);
            p1 = pt;
            p2 = CalculateOtherPointsPosition(p1, p2);
            bool res = p1Edge.AdjustP2();
            if (!res)
            {
                p1 = oldp1;
                p2 = oldp2;
                return res;
            }
            return p2Edge.AdjustP1();
        }

        public override bool AdjustP1()
        {
            if (p1Edge == null || p2Edge == null)
                return false;

            Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
            p1 = p1Edge.p2;

            p2 = CalculateOtherPointsPosition(p1, p2);

            bool res = p2Edge.AdjustP1();
            if (!res)
            {
                p1 = oldp1;
                p2 = oldp2;
            }
            return res;
        }

        public override bool AdjustP2()
        {
            if (p1Edge == null || p2Edge == null)
                return false;

            Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
            p2 = p2Edge.p1;

            p1 = CalculateOtherPointsPosition(p2, p1);

            bool res = p1Edge.AdjustP2();
            if (!res)
            {
                p1 = oldp1;
                p2 = oldp2;
            }
            return res;
        }

        private Point CalculateOtherPointsPosition(Point p1, Point p2)
        {
            Point res = new Point();
            double dist = (p2 - p1).Length;

            //Wektor jednostkowy do przesunięcia
            double unitX = (p2.X - p1.X) / dist;
            double unitY = (p2.Y - p1.Y) / dist;

            //Przesunięcie
            res.X = p1.X + unitX * length;
            res.Y = p1.Y + unitY * length;

            return res;
        }
    }
}
