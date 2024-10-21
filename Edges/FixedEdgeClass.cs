using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Point = System.Windows.Point;
using System.Windows.Documents;

namespace GK_Proj_1.Edges
{
    public class FixedLenEdge : Edge
    {
        public FixedLenEdge(Point p1, Point p2) : base(p1, p2) { length = (p1 - p2).Length; type = RelationType.FixedLen; }

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
            bool res = false;
            Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);

            if (p1Edge.type == RelationType.Vertical)
            {
                if (Math.Abs(p2.X - p1Edge.p2.X) < length)
                {
                    if (ind != 0)
                    {
                        double z = Math.Sqrt(Math.Pow(length, 2) - Math.Pow(p2.X - p1Edge.p2.X, 2));
                        p1.X = p1Edge.p2.X;
                        if (Math.Abs(p2.Y - z - p1Edge.p2.Y) < Math.Abs(p2.Y + z - p1Edge.p2.Y))
                            p1.Y = p2.Y - z;
                        else
                            p1.Y = p2.Y + z;
                        p1Edge.p2.Y = p1.Y;
                        res = true;
                        goto AdjustingCP1;
                    }
                }
                else
                {
                    if (p1.X < p1Edge.p2.X)
                        p2.X = p1Edge.p2.X - length;
                    else
                        p2.X = p1Edge.p2.X + length;
                    p1.X = p1Edge.p2.X;
                    p1.Y = p1Edge.p2.Y;
                    p2.Y = p1.Y;
                    goto Exit;
                }
            }

            if (p1Edge.type == RelationType.Horizontal)
            {
                if (length < Math.Abs(p2.Y - p1Edge.p2.Y))
                {
                    if (p1.Y < p1Edge.p2.Y)
                        p2.Y = p1Edge.p2.Y - length;
                    else
                        p2.Y = p1Edge.p2.Y + length;
                    p1.X = p1Edge.p2.X;
                    p1.Y = p1Edge.p2.Y;
                    p2.X = p1.X;
                    goto Exit;
                }
                else
                {
                    if (ind != 0)
                    {
                        double z = Math.Sqrt(Math.Pow(length, 2) - Math.Pow(p2.Y - p1Edge.p2.Y, 2));

                        if (Math.Abs(p2.X + z - p1Edge.p2.X) < Math.Abs(p2.X - z - p1Edge.p2.X))
                            p1Edge.p2.X = p2.X + z;
                        else
                            p1Edge.p2.X = p2.X - z;
                        p1.X = p1Edge.p2.X;
                        p1.Y = p1Edge.p2.Y;
                        res = true;
                        goto AdjustingCP1;
                    }
                }
            }

            p1 = p1Edge.p2;

            p2 = CalculateOtherPointsPosition(p1, p2);

        Exit:
            res = p2Edge.AdjustP1(++ind, maxRecCount);

        // W razie zmiany kierunku krawędzi sprawdzamy czy trzeba control point następnej zmienić
        AdjustingCP1:
            if (res && p2Edge.vertType != VertRelationType.G0)
                p2Edge.AdjustCP1(0, 0);

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
            bool res = false;

            if (p2Edge.type == RelationType.Vertical)
            {
                if (length > Math.Abs(p2Edge.p1.X - p1.X))
                {
                    if (ind != 0)
                    {
                        double z = Math.Sqrt(Math.Pow(length, 2) - Math.Pow(p2Edge.p1.X - p1.X, 2));
                        p2.X = p2Edge.p1.X;
                        if (Math.Abs(p1.Y - z - p2Edge.p1.Y) < Math.Abs(p1.Y + z - p2Edge.p1.Y))
                            p2.Y = p1.Y - z;
                        else
                            p2.Y = p1.Y + z;
                        p2Edge.p1.Y = p2.Y;
                        res = true;
                        goto AdjustingCP2;
                    }
                }
                else
                {
                    if (ind != 0)
                    {
                        if (p2.X < p2Edge.p1.X)
                            p1.X = p2Edge.p1.X - length;
                        else
                            p1.X = p2Edge.p1.X + length;
                        p2.X = p2Edge.p1.X;
                        p2.Y = p2Edge.p1.Y;
                        p1.Y = p2.Y;
                        goto Exit;
                    }
                }
            }

            if (p2Edge.type == RelationType.Horizontal)
            {
                if (length < Math.Abs(p1.Y - p2Edge.p1.Y))
                {
                    if(ind != 0)
                    {
                        if (p2Edge.p1.Y > p2.Y)
                            p1.Y = p2Edge.p1.Y - length;
                        else
                            p1.Y = p2Edge.p1.Y + length;
                        p2.X = p2Edge.p1.X;
                        p2.Y = p2Edge.p1.Y;
                        p1.X = p2.X;
                        goto Exit;
                    }
                }
                else
                {
                    if (ind != 0)
                    {
                        double z = Math.Sqrt(Math.Pow(length, 2) - Math.Pow(p2Edge.p1.Y - p1.Y, 2));
                        if (Math.Abs(p1.X - z - p2Edge.p1.X) > Math.Abs(p1.X + z - p2Edge.p1.X))
                            p2Edge.p1.X = p1.X + z;
                        else
                            p2Edge.p1.X = p1.X - z;
                        p2.X = p2Edge.p1.X;
                        p2.Y = p2Edge.p1.Y;
                        res = true;
                        goto AdjustingCP2;
                    }
                }
            }

            p2 = p2Edge.p1;

            p1 = CalculateOtherPointsPosition(p2, p1);

        Exit:
            res = p1Edge.AdjustP2(++ind, maxRecCount);
        AdjustingCP2:
            if (res && vertType != VertRelationType.G0)
                p1Edge.AdjustCP2(0, 0);
            if (!res)
            {
                p1 = oldp1;
                p2 = oldp2;
            }
            return res;
        }

        // Dopasowywujemy pozycję p2, tak aby był w odpowiedniej odległosci od p1 i ją zwracamy
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

        public override bool MakeCollinearToP1(int ind, int edgesCount)
        {
            bool res = false;
            Point p1old = new Point(p1.X, p1.Y), p2old = new Point(p2.X, p2.Y);
            (Point p1s, Point p2s) = p1Edge.GetCollinearPoints(2);

            double dx = p2s.X - p1s.X;
            double dy = p2s.Y - p1s.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double uX = dx / len;
            double uY = dy / len;
            p2.X = p1.X + uX * length;
            p2.Y = p1.Y + uY * length;

            res = p2Edge.AdjustP1(++ind, edgesCount);

            if (!res)
            {
                p1 = p1old;
                p2 = p2old;
            }

            return res;
        }

        public override bool MakeCollinearToP2(int ind, int edgesCount)
        {
            bool res = false;
            Point p1old = new Point(p1.X, p1.Y), p2old = new Point(p2.X, p2.Y);
            (Point p1s, Point p2s) = p2Edge.GetCollinearPoints(1);

            double dx = p2s.X - p1s.X;
            double dy = p2s.Y - p1s.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double uX = dx / len;
            double uY = dy / len;
            p1.X = p2.X + uX * length;
            p1.Y = p2.Y + uY * length;

            res = p1Edge.AdjustP2(++ind, edgesCount);

            if (!res)
            {
                p1 = p1old;
                p2 = p2old;
            }

            return res;
        }

        public override void Draw(DrawingContext dc)
        {
            base.Draw(dc);
            Point middle = Geometry.GetMiddle(p1, p2);
            middle.Y -= 20;
            FormattedText ft = new FormattedText(Math.Round(length, 2).ToString(), System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 20,
                Brushes.Brown, VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
            middle.X -= ft.Width / 2;
            dc.DrawText(ft, middle);
        }
    }
}
