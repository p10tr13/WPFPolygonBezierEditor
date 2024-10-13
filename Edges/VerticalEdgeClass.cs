using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Point = System.Windows.Point;

namespace GK_Proj_1.Edges
{
    public class VerticalEdge : Edge
    {
        public VerticalEdge(Point p1, Point p2) : base(p1, new Point(p1.X, p2.Y)) { type = RelationType.Vertical; }

        public override bool AdjustP1(int ind, int maxRecCount)
        {
            if (p1Edge == null)
                return false;
            if (p1.X == p1Edge.p2.X)
            {
                p1 = p1Edge.p2;
                return true;
            }
            if (ind == maxRecCount)
                return false;

            switch (p2Edge.type)
            {
                case RelationType.Bezier:
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
                            res = p2Edge.AdjustP1(++ind, maxRecCount);
                        if (!res)
                        {
                            p1 = oldp1;
                            p2 = oldp2;
                        }
                        return res;
                    }
            }
        }

        public override bool AdjustP2(int ind, int maxRecCount)
        {
            if (p2Edge == null)
                return false;
            if (p1.X == p2Edge.p1.X)
            {
                p2 = p2Edge.p1;
                return true;
            }
            if (ind == maxRecCount)
                return false;

            switch (p1Edge.type)
            {
                case RelationType.Bezier:
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
                            res = p1Edge.AdjustP2(++ind, maxRecCount);
                        if (!res)
                        {
                            p1 = oldp1;
                            p2 = oldp2;
                        }
                        return res;
                    }
            }
        }

        public override bool MoveP1To(Point pt, int edgesCount)
        {
            if ((pt - p2).Length <= 2)
                return false;
            Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
            p1 = pt;
            p2.X = pt.X;
            bool res = p1Edge.AdjustP2(0, edgesCount);
            if (!res)
            {
                p1 = oldp1;
                p2 = oldp2;
                return res;
            }
            return p2Edge.AdjustP1(1, edgesCount);
        }

        public override void Draw(DrawingContext dc)
        {
            base.Draw(dc);
            Point middle = GetMiddle();
            middle.X -= 20;
            FormattedText ft = new FormattedText("|", System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 20,
                Brushes.Brown, VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
            middle.Y -= ft.Height/2;
            dc.DrawText(ft, middle);
        }
    }
}
