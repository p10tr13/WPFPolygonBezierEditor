using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
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

        private List<(int x, int y)> pixels;

        public BezierEdge(Point pnt1, Point pnt2) : base(pnt1, pnt2)
        {
            p1c = new Point((2 * p1.X + 2 * p2.X - p2.Y + p1.Y) / 4, (2 * p1.Y + 2 * p2.Y + p2.X - p1.X) / 4);
            p2c = new Point((2 * p1.X + 2 * p2.X + p2.Y - p1.Y) / 4, (2 * p1.Y + 2 * p2.Y - p2.X + p1.X) / 4);
            type = RelationType.Bezier;
        }

        public override void Draw(DrawingContext dc)
        {
            pixels = DrawingAlgorithms.DrawBezierCurve(p1, p1c, p2c, p2, dc);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p1, p2);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p1, p1c);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p2, p2c);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p1c, p2c);
            dc.DrawEllipse(Var.VertColor, null, p1, Var.VertSize, Var.VertSize);
            dc.DrawEllipse(Var.VertColor, null, p1c, Var.VertSize, Var.VertSize);
            dc.DrawEllipse(Var.VertColor, null, p2c, Var.VertSize, Var.VertSize);
            DrawG(dc);
        }

        public override (Point p1, Point p2) GetCollinearPoints(int vert)
        {
            if (vert == 1)
                return (p1c, p1);
            else
                return (p2c, p2);   
        }

        public void DrawG(DrawingContext dc)
        {
            string s1, s2;
            if (p1Edge != null)
            {
                if (vertType == VertRelationType.G1)
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
                if (p2Edge.vertType == VertRelationType.G1)
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

        public static Point MovePointToBeCollinear(Point x1, Point x2, Point x3)
        {
            double dx = x2.X - x1.X;
            double dy = x2.Y - x1.Y;

            double t = ((x3.X - x1.X) * dx + (x3.Y - x1.Y) * dy) / (Math.Pow(dx, 2) + Math.Pow(dy, 2));

            return new Point(x1.X + dx * t, x1.Y + dy * t);
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
            else if (cpind == 2)
                return MoveCP2To(pt, edgesCount);
            return false;
        }

        public override void AdjustCP1()
        {
            (Point pt1, Point pt2) = p1Edge.GetCollinearPoints(2);
            p1c = MovePointToBeCollinear(pt1, pt2, p1c);
        }

        public override void AdjustCP2() 
        {
            (Point pt1, Point pt2) = p2Edge.GetCollinearPoints(1);
            p2c = MovePointToBeCollinear(pt1, pt2, p2c);
        }

        public bool MoveCP1To(Point pt, int edgesCount)
        {
            p1c.X = pt.X;
            p1c.Y = pt.Y;
            if (vertType == VertRelationType.G0)
                return true;
            (Point pt1, Point pt2) = p1Edge.GetCollinearPoints(2);
            if (IsCollinear(pt1, pt2, p1c))
                return true;
            return true;
        }

        public bool MoveCP2To(Point pt, int edgesCount)
        {
            p2c.X = pt.X;
            p2c.Y = pt.Y;
            if (p2Edge.vertType == VertRelationType.G0)
                return true;
            (Point pt1, Point pt2) = p2Edge.GetCollinearPoints(1);
            if (IsCollinear(pt1, pt2, p2c))
                return true;
            
            return true;
        }

        public override bool IsNearEdge(Point pt)
        {
            foreach ((int x, int y) in pixels)
            {
                if (Math.Sqrt(Math.Pow(x - pt.X, 2) + Math.Pow(y - pt.Y, 2)) < 10)
                    return true;
            }
            return false;
        }
    }
}
