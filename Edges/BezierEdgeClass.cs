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
    }
}
