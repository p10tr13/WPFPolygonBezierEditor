using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace GK_Proj_1.Edges
{
    public class BezierEdge : Edge
    {
        public Point p1c, p2c;

        public BezierEdge(System.Windows.Point pnt1, System.Windows.Point pnt2) : base(pnt1, pnt2)
        {
            p1c = new Point(p1.X + 50, p1.Y + 50);
            p2c = new Point(p2.X - 50, p2.Y - 50);
            type = RelationType.Bezier;
        }

        public override void Draw(DrawingContext dc)
        {
            DrawingAlgorithms.DrawBezierCurve(p1, p1c, p2c, p2, dc);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p1, p2);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p1, p1c);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p2, p2c);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p1c, p2c);
            dc.DrawEllipse(Var.VertColor, null, p1, Var.VertSize, Var.VertSize);
            dc.DrawEllipse(Var.VertColor, null, p1c, Var.VertSize, Var.VertSize);
            dc.DrawEllipse(Var.VertColor, null, p2c, Var.VertSize, Var.VertSize);
            DrawG(dc);
        }

        public void DrawG(DrawingContext dc)
        {
            string s1, s2;
            if (p1Edge != null)
            {
                if (IsCollinear(p1Edge.p1, p1Edge.p2, p1c))
                    s1 = "G1";
                else
                    s1 = "G0";

                FormattedText ft = new FormattedText(s1, System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Arial"), 15,
                        Brushes.Brown, VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
                dc.DrawText(ft, new Point(p1Edge.p2.X + 10, p1Edge.p2.Y + 10));
            }

            if (p2Edge != null)
            {
                if (IsCollinear(p2c, p2Edge.p1, p2Edge.p2))
                    s2 = "G1";
                else
                    s2 = "G0";

                FormattedText ft = new FormattedText(s2, System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Arial"), 15,
                        Brushes.Brown, VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
                dc.DrawText(ft, new Point(p2Edge.p1.X + 10, p2Edge.p1.Y + 10));
            }
        }

        public static bool IsCollinear(Point x1, Point x2, Point x3)
        {
            return Math.Abs((x2.X - x1.X) * (x3.Y - x1.Y) - (x2.Y - x1.Y) * (x3.X - x1.X)) < Var.Eps;
        }

        public override bool IsNearControlPoint(Point pt, out int indc)
        {
            if ((p1c - pt).Length < 10)
            {
                indc = 1;
                return true;
            }
            if ((p2c - pt).Length < 10)
            {
                indc = 2;
                return true;
            }
            indc = -1;
            return false;
        }

        public override bool MoveCPTo(Point pt, int cpind, int edgesCount)
        {
            if (cpind == 1)
                return MoveCP1To(pt, edgesCount);
            else if(cpind == 2)
                return MoveCP2To(pt, edgesCount);
            return false;
        }

        public bool MoveCP1To(Point pt, int edgesCount)
        {
            if(vertType == VertRelationType.Regular)
            {
                p1c.X = pt.X;
                p1c.Y = pt.Y;
                return true;
            }

            p1c.X = pt.X;
            p1c.Y = pt.Y;
            return true;
        }

        public bool MoveCP2To(Point pt, int edgesCount)
        {
            if (vertType == VertRelationType.Regular)
            {
                p2c.X = pt.X;
                p2c.Y = pt.Y;
                return true;
            }

            p2c.X = pt.X;
            p2c.Y = pt.Y;
            return true;
        }
    }
}
