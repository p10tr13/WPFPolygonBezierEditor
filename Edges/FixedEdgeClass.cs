using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Point = System.Windows.Point;

namespace GK_Proj_1.Edges
{
    public class FixedLenEdge : Edge
    {
        public FixedLenEdge(Point p1, Point p2) : base(p1, p2) { length = (p1 - p2).Length; }

        public double length { get; }

        public override bool MoveP1To(Point pt, int edgesCount)
        {

            Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
            p1 = pt;
            p2 = CalculateOtherPointsPosition(p1, p2);
            bool res = p1Edge.AdjustP2(0, edgesCount);
            if (!res)
            {
                p1 = oldp1;
                p2 = oldp2;
                return res;
            }
            res = p2Edge.AdjustP1(0, edgesCount);
            if (!res)
            {
                
            }
            return res;
        }

        public override bool AdjustP1(int ind, int maxRecCount)
        {
            if (p1Edge == null || p2Edge == null)
                return false;
            if (ind == maxRecCount)
                return false;

            Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
            p1 = p1Edge.p2;

            p2 = CalculateOtherPointsPosition(p1, p2);

            if (p1Edge.type == RelationType.Horizontal && p2Edge.type == RelationType.Horizontal)
            {
                if(length < Math.Abs(p1Edge.p2.Y - p2Edge.p1.Y))
                {
                    p2.X = p1.X;
                    if(p1Edge.p2.Y < p2Edge.p1.Y)
                        p2.Y = p1.Y + length;
                    else
                        p2.Y = p1.Y - length;
                }
                else
                {
                    double z = Math.Sqrt(Math.Pow(length, 2) - Math.Pow(p2Edge.p1.Y - p1.Y, 2));
                    if (p1.X - z - p2Edge.p1.X < p1.X + z - p2Edge.p1.X)
                        p2.X = p1.X - z;
                    else
                        p2.X = p1.X + z;
                    p2.Y = p2Edge.p1.Y;
                    if (maxRecCount - ind == 1)
                    {
                        double s = p2.X - p2Edge.p1.X;
                        p2.X = p2Edge.p1.X;
                        p1Edge.p2.X += s;
                        p1.X += s;
                        return true;
                    }
                }
            }

            bool res = p2Edge.AdjustP1(++ind, maxRecCount);
            if (!res)
            {
                p1 = oldp1;
                p2 = oldp2;
            }
            return res;
        }

        public override bool AdjustP2(int ind, int maxRecCount)
        {
            if (p1Edge == null || p2Edge == null)
                return false;
            if (ind == maxRecCount)
                return false;

            Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
            p2 = p2Edge.p1;

            p1 = CalculateOtherPointsPosition(p2, p1);

            if (p1Edge.type == RelationType.Horizontal && p2Edge.type == RelationType.Horizontal)
            {
                if (length < Math.Abs(p2Edge.p1.Y - p1Edge.p2.Y))
                {
                    p1.X = p2.X;
                    if (p2Edge.p1.Y < p1Edge.p2.Y)
                        p1.Y = p2.Y + length;
                    else
                        p1.Y = p2.Y - length;
                }
                else
                {
                    double z = Math.Sqrt(Math.Pow(length, 2) - Math.Pow(p1Edge.p2.Y - p2.Y, 2));
                    if (p2.X - z - p1Edge.p2.X > p2.X + z - p1Edge.p2.X)
                        p1.X = p2.X - z;
                    else
                        p1.X = p2.X + z;
                    p1.Y = p1Edge.p2.Y;

                    if(maxRecCount - ind == 1)
                    {
                        double s = p1.X - p1Edge.p2.X;
                        p1.X = p1Edge.p2.X;
                        p2Edge.p1.X -= s;
                        p2.X -= s;
                        return true;
                    }
                }
            }
            
            bool res = p1Edge.AdjustP2(++ind, maxRecCount);
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

        public override void Draw(DrawingContext dc)
        {
            base.Draw(dc);
            Point middle = GetMiddle();
            middle.Y -= 20;
            FormattedText ft = new FormattedText(Math.Round(length,2).ToString(), System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 20,
                Brushes.Brown, VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
            middle.X -= ft.Width / 2;
            dc.DrawText(ft, middle);
        }
    }
}
