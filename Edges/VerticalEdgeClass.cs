using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace GK_Proj_1.Edges
{
    public class VerticalEdge : Edge
    {
        public VerticalEdge(Point p1, Point p2) : base(p1, new Point(p1.X, p2.Y)) { base.type = RelationType.Vertical; }

        public override bool AdjustP1()
        {
            if (p1Edge == null)
                return false;
            if (p1.X == p1Edge.p2.X)
            {
                p1 = p1Edge.p2;
                return true;
            }

            switch (p2Edge.type)
            {
                case RelationType.FixedLen:
                    {
                        return true;
                    }
                default:
                    {
                        Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
                        bool res = false;
                        p1 = p1Edge.p2;
                        p2 = new Point(p1.X, p2.Y);
                        if (p2Edge != null)
                            res = p2Edge.AdjustP1();
                        if (!res)
                        {
                            p1 = oldp1;
                            p2 = oldp2;
                        }
                        return res;
                    }
            }
        }

        public override bool AdjustP2()
        {
            if (p2Edge == null)
                return false;
            if (p1.X == p2Edge.p1.X)
            {
                p2 = p2Edge.p1;
                return true;
            }

            switch (p1Edge.type)
            {
                case RelationType.FixedLen:
                    {
                        return true;
                    }
                default:
                    {
                        Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
                        bool res = false;
                        p2 = p2Edge.p1;
                        p1 = new Point(p2.X, p1.Y);
                        if (p1Edge != null)
                            res = p1Edge.AdjustP2();
                        if (!res)
                        {
                            p1 = oldp1;
                            p2 = oldp2;
                        }
                        return res;
                    }
            }
        }

        public override bool MoveP1To(Point pt)
        {
            if ((pt - p2).Length <= 2)
                return false;
            Point oldp1 = new Point(base.p1.X, base.p1.Y), oldp2 = new Point(base.p2.X, base.p2.Y);
            base.p1 = pt;
            base.p2.X = pt.X;
            bool res = base.p1Edge.AdjustP2();
            if (!res)
            {
                base.p1 = oldp1;
                base.p2 = oldp2;
                return res;
            }
            return base.p2Edge.AdjustP1();
        }
    }
}
