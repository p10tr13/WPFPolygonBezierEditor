using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p1c, p2);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p2, p2c);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p2c, p1);
            dc.DrawLine(new Pen(Brushes.LightGray, 2), p1c, p2c);
            dc.DrawEllipse(Var.VertColor, null, p1, Var.VertSize, Var.VertSize);
            dc.DrawEllipse(Var.VertColor, null, p1c, Var.VertSize, Var.VertSize);
            dc.DrawEllipse(Var.VertColor, null, p2c, Var.VertSize, Var.VertSize);
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
