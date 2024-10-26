using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace GK_Proj_1.Edges
{
    public class BezierEdge : Edge
    {
        public Point p1c, p2c; // Punkty kontrolne

        private List<(int x, int y)> pixels; // Lista pikseli, w których rysujemy krzywą

        public BezierEdge(Point pnt1, Point pnt2) : base(pnt1, pnt2)
        {
            // Wybrane ich początkowe pozycje to romb (nie ma to teraz znazenia, bo i tak zmieniamy ich pozycje dla G1)
            p1c = new Point((2 * p1.X + 2 * p2.X - p2.Y + p1.Y) / 4, (2 * p1.Y + 2 * p2.Y + p2.X - p1.X) / 4);
            p2c = new Point((2 * p1.X + 2 * p2.X - p2.Y + p1.Y) / 4, (2 * p1.Y + 2 * p2.Y - p2.X + p1.X) / 4);
            type = RelationType.Bezier;
        }

        public override void Draw(DrawingContext dc)
        {
            switch (Var.BezierDrawingStyle)
            {
                case BezierDrawingStyle.Rasterisation:
                    {
                        pixels = DrawingAlgorithms.DrawBezierCurve(p1, p1c, p2c, p2, dc);
                        break;
                    }
                case BezierDrawingStyle.Easy:
                    {
                        pixels = DrawingAlgorithms.DrawBezierCurve(p1, p1c, p2c, p2, dc, 100);
                        break;
                    }
            }
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

        // Funkcja wypisuje odpowiednie G0/G1/C1 przy wierzchołku
        public void DrawG(DrawingContext dc)
        {
            string s1, s2;
            if (p1Edge != null)
            {
                if (vertType == VertRelationType.G1)
                    s1 = "G1";
                else if (vertType == VertRelationType.C1)
                    s1 = "C1";
                else
                    s1 = "G0";

                FormattedText ft = new FormattedText(s1, System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Arial"), 15,
                        Brushes.Brown, VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
                dc.DrawText(ft, new Point(p1.X + 10, p1.Y + 10));
            }

            if (p2Edge != null && p2Edge.type != RelationType.Bezier)
            {
                if (p2Edge.vertType == VertRelationType.G1)
                    s2 = "G1";
                else if (p2Edge.vertType == VertRelationType.C1)
                    s2 = "C1";
                else
                    s2 = "G0";

                FormattedText ft = new FormattedText(s2, System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Arial"), 15,
                        Brushes.Brown, VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
                dc.DrawText(ft, new Point(p2Edge.p1.X + 10, p2Edge.p1.Y + 10));
            }
        }

        // Zwracamy czy punkt jest bliskoo punktu kontrolnego oraz informację którego
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

        // Przesuwamy punkt kontrolny o danym indeksie do pt
        public override bool MoveCPTo(Point pt, int cpind, int edgesCount)
        {
            if (cpind == 1)
                return MoveCP1To(pt, edgesCount);
            else if (cpind == 2)
                return MoveCP2To(pt, edgesCount);
            return false;
        }

        public override bool MoveP1To(Point pt, int edgesCount)
        {
            if (p1Edge == null || p2Edge == null)
                return false;
            bool res = false;
            Point p1old = p1;
            p1 = pt;
            if (p2Edge.vertType == VertRelationType.G1)
                p2Edge.AdjustCP1(0, 0);
            res = p1Edge.AdjustP2(0, edgesCount);
            if (res && vertType == VertRelationType.G1)
                AdjustCP1(p1.X - p1old.X, p1.Y - p1old.Y);
            return res;
        }

        public override void AdjustCP1(double dx, double dy)
        {
            (Point pt1, Point pt2) = p1Edge.GetCollinearPoints(2);
            p1c.X += dx;
            p1c.Y += dy;
            if (vertType == VertRelationType.G1)
            {
                p1c = Geometry.MovePointToBeCollinear(pt1, pt2, p1c);
                if (Geometry.OnTheSameSideofP2(pt1, pt2, p1c))
                {
                    dx = pt2.X - p1c.X;
                    dy = pt2.Y - p1c.Y;
                    p1c.X += 2 * dx;
                    p1c.Y += 2 * dy;
                }
            }
            else if (vertType == VertRelationType.C1)
            {
                p1c = Geometry.MovePointToBeCollinear(pt1, pt2, p1c);
                dx = pt2.X - pt1.X;
                dy = pt2.Y - pt1.Y;
                if (p1Edge.type == RelationType.Bezier) { p1c.X = pt2.X + dx; p1c.Y = pt2.Y + dy; return; }
                p1c.X = pt2.X + dx/3;
                p1c.Y = pt2.Y + dy/3;
            }
        }

        public override void AdjustCP2(double dx, double dy)
        {
            (Point pt1, Point pt2) = p2Edge.GetCollinearPoints(1);
            p2c.X += dx;
            p2c.Y += dy;
            if (p2Edge.vertType == VertRelationType.G1)
            {
                p2c = Geometry.MovePointToBeCollinear(pt1, pt2, p2c);
                if (Geometry.OnTheSameSideofP2(pt1, pt2, p2c))
                {
                    dx = pt2.X - p2c.X;
                    dy = pt2.Y - p2c.Y;
                    p2c.X += 2 * dx;
                    p2c.Y += 2 * dy;
                }
            }
            else if (p2Edge.vertType == VertRelationType.C1)
            {
                p2c = Geometry.MovePointToBeCollinear(pt1, pt2, p2c);
                dx = pt2.X - pt1.X;
                dy = pt2.Y - pt1.Y;
                if (p2Edge.type == RelationType.Bezier) { p2c.X = pt2.X + dx; p2c.Y = pt2.Y + dy; return; }
                p2c.X = pt2.X + dx/3;
                p2c.Y = pt2.Y + dy/3;
            }
        }

        public override bool AdjustP1(int ind, int maxRecCount)
        {
            if (p1Edge == null)
                return false;
            double dx = p1Edge.p2.X - p1.X, dy = p1Edge.p2.Y - p1.Y;
            p1 = p1Edge.p2;
            AdjustCP1(dx, dy);
            return true;
        }

        public override bool AdjustP2(int ind, int maxRecCount)
        {
            if (p2Edge == null)
                return false;
            double dx = p2Edge.p1.X - p2.X, dy = p2Edge.p1.Y - p2.Y;
            p2 = p2Edge.p1;
            AdjustCP2(dx, dy);
            return true;
        }

        // Zmieniamy miejsce punktu kontrolnego p1c
        public bool MoveCP1To(Point pt, int edgesCount)
        {
            Point p1cold = p1c;
            p1c.X = pt.X;
            p1c.Y = pt.Y;
            if (vertType == VertRelationType.G0)
                return true;
            (Point pt1, Point pt2) = p1Edge.GetCollinearPoints(2);
            if ((Geometry.IsCollinear(pt1, pt2, p1c) && vertType == VertRelationType.G1) || (p1c - p1).Length < 2)
                return true;
            bool res = false;
            switch (p1Edge.type)
            {
                case RelationType.Horizontal:
                    {
                        p1.Y = pt.Y;
                        if (vertType == VertRelationType.G1)
                            res = p1Edge.AdjustP2(1, edgesCount);
                        else
                        {
                            p1Edge.p2 = p1;
                            double dx = p1.X - pt.X;
                            double dy = p1.Y - pt.Y;
                            Point p1Ep1old = new Point(p1Edge.p1.X, p1Edge.p1.Y);
                            p1Edge.p1 = new Point(pt.X + 4 * dx,pt.Y + 4 * dy);
                            res = p1Edge.p1Edge.AdjustP2(2, edgesCount);
                            if (!res) p1Edge.p1 = p1Ep1old;
                        }
                        break;
                    }
                case RelationType.Vertical:
                    {
                        p1.X = pt.X;
                        if (vertType == VertRelationType.G1)
                            res = p1Edge.AdjustP2(1, edgesCount);
                        else
                        {
                            p1Edge.p2 = p1;
                            double dx = p1.X - pt.X;
                            double dy = p1.Y - pt.Y;
                            p1Edge.p1 = new Point(pt.X + 4 * dx, pt.Y + 4 * dy);
                            res = p1Edge.p1Edge.AdjustP2(2, edgesCount);
                        }
                        break;
                    }
                case RelationType.Bezier:
                    {
                        p1Edge.AdjustCP2(0, 0);
                        res = true;
                        break;
                    }
                case RelationType.FixedLen:
                    {
                        Point p1Ep2old = new Point(p1Edge.p2.X, p1Edge.p2.Y);
                        if (vertType == VertRelationType.C1) 
                        {
                            p1.X -= p1cold.X - pt.X;
                            p1.Y -= p1cold.Y - pt.Y;
                            p1Edge.p2 = p1;
                        }
                        res = p1Edge.MakeCollinearToP2(1, edgesCount);
                        if (!res) { p1Edge.p2 = p1Ep2old; }
                        break;
                    }
                default:
                    {
                        if (vertType == VertRelationType.G1)
                            res = p1Edge.MakeCollinearToP2(1, edgesCount);
                        else
                        {
                            double dx = p1.X - pt.X;
                            double dy = p1.Y - pt.Y;
                            Point p1Ep1old = new Point(p1Edge.p1.X, p1Edge.p1.Y);
                            p1Edge.p1 = new Point(pt.X + 4 * dx, pt.Y + 4 * dy);
                            res = p1Edge.p1Edge.AdjustP2(2, edgesCount);
                            if(!res) p1Edge.p1 = p1Ep1old;
                        }
                        break;
                    }
            }
            if (!res)
            {
                p1c = p1cold;
            }
            return res;
        }

        // Zmieniamy miejsce punktu kontrolnego p2c
        public bool MoveCP2To(Point pt, int edgesCount)
        {
            Point p2cold = p2c;
            p2c.X = pt.X;
            p2c.Y = pt.Y;
            if (p2Edge.vertType == VertRelationType.G0)
                return true;
            (Point pt1, Point pt2) = p2Edge.GetCollinearPoints(1);
            if (Geometry.IsCollinear(pt1, pt2, pt) || (pt - p2).Length < 2)
                return true;
            bool res = false;
            switch (p2Edge.type)
            {
                case RelationType.Horizontal:
                    {
                        p2.Y = pt.Y;
                        if (p2Edge.vertType == VertRelationType.G1)
                            res = p2Edge.AdjustP1(1, edgesCount);
                        else
                        {
                            p2Edge.p1 = p2;
                            double dx = p2.X - pt.X;
                            double dy = p2.Y - pt.Y;
                            Point p2Ep2old = new Point(p2Edge.p2.X, p2Edge.p2.Y);
                            p2Edge.p2 = new Point(pt.X + 4 * dx, pt.Y + 4 * dy);
                            res = p2Edge.p2Edge.AdjustP1(2, edgesCount);
                            if (!res) p2Edge.p2 = p2Ep2old;
                        }
                        break;
                    }
                case RelationType.Vertical:
                    {
                        p2.X = pt.X;
                        if (p2Edge.vertType == VertRelationType.G1)
                            res = p2Edge.AdjustP1(1, edgesCount);
                        else
                        {
                            p2Edge.p1 = p2;
                            double dx = p2.X - pt.X;
                            double dy = p2.Y - pt.Y;
                            Point p2Ep2old = new Point(p2Edge.p2.X, p2Edge.p2.Y);
                            p2Edge.p2 = new Point(pt.X + 4 * dx, pt.Y + 4 * dy);
                            res = p2Edge.p2Edge.AdjustP1(2, edgesCount);
                            if (!res) p2Edge.p2 = p2Ep2old;
                        }
                        break;
                    }
                case RelationType.Bezier:
                    {
                        p2Edge.AdjustCP1(0, 0);
                        res = true;
                        break;
                    }
                case RelationType.FixedLen:
                    {
                        Point p2Ep1old = new Point( p2Edge.p1.X, p2Edge.p1.Y);
                        if (p2Edge.vertType == VertRelationType.C1)
                        {
                            p2.X -= p2cold.X - pt.X;
                            p2.Y -= p2cold.Y - pt.Y;
                            p2Edge.p1 = p2;
                        }
                        res = p2Edge.MakeCollinearToP1(1, edgesCount);
                        if (!res)
                            p2Edge.p1 = p2Ep1old;
                        break;
                    }
                default:
                    {
                        if (p2Edge.vertType == VertRelationType.G1)
                            res = p2Edge.MakeCollinearToP1(1, edgesCount);
                        else
                        {
                            double dx = p2.X - pt.X;
                            double dy = p2.Y - pt.Y;
                            Point p2Ep2old = new Point(p2Edge.p2.X, p2Edge.p2.Y);
                            p2Edge.p2 = new Point(pt.X + 4 * dx, pt.Y + 4 * dy);
                            res = p2Edge.p2Edge.AdjustP1(2, edgesCount);
                            if (!res) p2Edge.p2 = p2Ep2old;
                        }
                        break;
                    }
            }
            if (!res)
            {
                p2c = p2cold;
            }
            return res;
        }

        // Sprawdzam piksele, czy punkt liknięty na ekranie jest blisko piksela,
        // a nie punktu pomiędzy wierzchołkami p1 i p2
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
