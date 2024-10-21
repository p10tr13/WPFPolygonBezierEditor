using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Point = System.Windows.Point;


namespace GK_Proj_1.Edges
{
    public class Edge
    {
        public RelationType type { get; set; } // Typ tej krawędzi
        public VertRelationType vertType { get; set; } // Typ wierzchołka p1
        public Edge? p1Edge, p2Edge; // Krawędzie sąsiadujące (np. p1Edge - krawędź sąsiadująca zz wierzchołkiem p1)
        public Point p1, p2; // Wierzchołki krawędzi

        public Edge(Point pnt1, Point pnt2)
        {
            p1 = pnt1;
            p2 = pnt2;
            type = RelationType.Regular;
            vertType = VertRelationType.G0;
        }

        // Rysowanie krawędzi i kropki wierzchołka p1
        public virtual void Draw(DrawingContext dc)
        {
            switch(Var.DrawingStyle)
            {
                case DrawingStyle.Windows:
                    {
                        Pen pen = new Pen(Var.EdgeColor, Var.LineWidth);
                        dc.DrawLine(pen, p1, p2);
                        break;
                    }
                case DrawingStyle.Bresenham:
                    {
                        DrawingAlgorithms.DrawBresenhamLine(p1, p2, dc);
                        break;
                    }
            }
            dc.DrawEllipse(Var.VertColor, null, p1, Var.VertSize, Var.VertSize);
        }

        // Sprawdzamy, czy punkt kliknięcia jest blisko p1
        public bool IsNearP1Vert(Point pt)
        {
            if ((p1 - pt).Length < 10)
            {
                return true;
            }
            return false;
        }

        // Sprawdzamy, czy punkt kliknięcia jest blisko p2
        public bool IsNearP2Vert(Point pt)
        {
            if ((p2 - pt).Length < 10)
            {
                return true;
            }
            return false;
        }

        // Sprawdzamy, czy punkt kliknięcia jest blisko krawędzi
        public virtual bool IsNearEdge(Point pt)
        {
            Point closestpt = Geometry.ClosestPointOnEdge(pt, p1, p2);
            if (closestpt.X == -1 && closestpt.Y == -1)
                return false;
            return (closestpt - pt).Length < 10;
        }

        // Sprawdzamy, czy dany punkt jest blisko punktu kontrolnego
        public virtual bool IsNearControlPoint(Point pt, out int indc)
        {
            indc = -1;
            return false;
        }

        // Dopasowanie pozycji wierzchołka p1 do p1Edge.p2
        public virtual bool AdjustP1(int ind, int maxRecCount)
        {
            if (p1Edge == null || p2Edge == null)
                return false;
            p1 = p1Edge.p2;
            if (p2Edge.vertType == VertRelationType.G1)
                p2Edge.AdjustCP1(0,0);
            return true;
        }

        // Dopasowanie pozycji wierzchołka p2 do p2Edge.p1
        public virtual bool AdjustP2(int ind, int maxRecCount)
        {
            if (p2Edge == null || p1Edge == null)
                return false;
            p2 = p2Edge.p1;
            if (vertType == VertRelationType.G1)
                p1Edge.AdjustCP2(0,0);
            return true;
        }

        // Przesunięcie wierzchołka p1 w dane miejsce, a następnie dopasowanie krawędzi z nim sąsiadującą
        public virtual bool MoveP1To(Point pt, int edgesCount)
        {
            if (p1Edge == null || p2Edge == null)
                return false;
            p1 = pt;
            if (p2Edge.vertType == VertRelationType.G1)
                p2Edge.AdjustCP1(0,0);
            return p1Edge.AdjustP2(0, edgesCount);
        }

        // Funkcja przesuwa punkt kontrolny, jeżeli taki przy krawędzi jest
        public virtual bool MoveCPTo(Point pt, int cpind, int edgesCount)
        {
            return false;
        }

        // Przesuwamy całą krawędź i "doczepiamy" do niej krawędzie odczepiuone przy przesunięciu
        public virtual bool MoveEdge(double x, double y, int edgesCount)
        {
            if(p1Edge == null || p2Edge == null)
                return false;
            p1.X += x;
            p1.Y += y;
            p2.X += x;
            p2.Y += y;
            bool res = p1Edge.AdjustP2(0, edgesCount - 1);
            if(!res)
            {
                p1.X -= x;
                p1.Y -= y;
                p2.X -= x;
                p2.Y -= y;
                return res;
            }
            res = p2Edge.AdjustP1(0, edgesCount - 1);
            if (!res)
            {
                p1.X -= x;
                p1.Y -= y;
                p2.X -= x;
                p2.Y -= y;
            }
            return res;
        }

        // Dopasowywuje położenie punktu kontrolnego przy wierzchołku p1, jeżeli jest taka potrzeba
        public virtual void AdjustCP1(double dx, double dy) {}

        // Dopasowywuje położenie punktu kontrolnego przy wierzchołku p2, jeżeli jest taka potrzeba 
        public virtual void AdjustCP2(double dx, double dy) {}

        // Funckja ustawia krawędź, aby była współliniowa do p1 i jego punktu kontrolnego,
        // następnie dołącza do nowej pozycji następną krawędź
        public virtual bool MakeCollinearToP1(int ind, int edgesCount)
        {
            bool res = false;
            Point p1old = new Point(p1.X, p1.Y), p2old = new Point(p2.X, p2.Y);
            (Point p1s, Point p2s) = p1Edge.GetCollinearPoints(2);
            switch (p2Edge.type)
            {
                case RelationType.FixedLen:
                    {
                        Point b = Geometry.MovePointToBeCollinear(p1s, p2s, p2Edge.p2);
                        double h = (p2Edge.p2 - b).Length;
                        double p2len = (p2Edge.p2 - p2Edge.p1).Length;
                        if (p2len < h)
                        {
                            p2 = b;
                            res = p2Edge.AdjustP1(++ind, edgesCount);
                        }
                        else
                        {
                            double z = Math.Sqrt(Math.Pow(p2len, 2) - Math.Pow(h, 2));
                            double dx = p2s.X - p1s.X;
                            double dy = p2s.Y - p1s.Y;
                            double len = Math.Sqrt(dx * dx + dy * dy);
                            double uX = dx/len;
                            double uY = dy/len;
                            p2.X = b.X + uX * z;
                            p2.Y = b.Y + uY * z;
                            p2Edge.p1 = p2;
                            return true;
                        }
                        break;
                    }
                case RelationType.Vertical:
                    {
                        if (Math.Abs(p2s.X - p1s.X) < Var.Eps)
                        {
                            res = false;
                            break;
                        }
                        double a = (p2s.Y - p1s.Y) / (p2s.X - p1s.X);
                        double b = p1s.Y - a * p1s.X;
                        double y3 = a * p2Edge.p1.X + b;
                        p2.X = p2Edge.p1.X;
                        p2.Y = y3;
                        p2Edge.p1.Y = y3;
                        res = true;
                        break;
                    }
                case RelationType.Horizontal:
                    {
                        if(Math.Abs(p2s.X - p1s.X) < Var.Eps)
                            { res = false; break; }
                        double a = (p2s.Y - p1s.Y) / (p2s.X - p1s.X);
                        double b = p1s.Y - a * p1s.X;
                        double x3 = (p2Edge.p1.Y - b)/a;
                        p2.Y = p2Edge.p1.Y;
                        p2.X = x3;
                        p2Edge.p1.X = x3;
                        res = true;
                        break;
                    }
                default:
                    {
                        Point p2new = Geometry.FindIntersection(p1s, p2s, p2Edge.p1, p2Edge.p2);
                        if (p2new.X == -1 && p2new.Y == -1)
                            return false;
                        p2 = p2new;
                        res = p2Edge.AdjustP1(++ind, edgesCount);
                        break;
                    }
            }

            if (!res)
            {
                p1 = p1old;
                p2 = p2old;
            }

            return res;
        }

        // Funckja ustawia krawędź, aby była współliniowa do p2 i jego punktu kontrolnego,
        // następnie dołącza do nowej pozycji następną krawędź
        public virtual bool MakeCollinearToP2(int ind, int edgesCount)
        {
            bool res = false;
            Point p1old = new Point(p1.X, p1.Y), p2old = new Point(p2.X, p2.Y);
            (Point p1s, Point p2s) = p2Edge.GetCollinearPoints(1);
            switch (p1Edge.type)
            {
                case RelationType.FixedLen:
                    {
                        Point b = Geometry.MovePointToBeCollinear(p1s, p2s, p1Edge.p1);
                        double h = (p1Edge.p1 - b).Length;
                        double p1len = (p1Edge.p1 - p1Edge.p1).Length;
                        if (p1len < h)
                        {
                            p1 = b;
                            res = p1Edge.AdjustP2(++ind, edgesCount);
                        }
                        else
                        {
                            double z = Math.Sqrt(Math.Pow(p1len, 2) - Math.Pow(h, 2));
                            double dx = p2s.X - p1s.X;
                            double dy = p2s.Y - p1s.Y;
                            double len = Math.Sqrt(dx * dx + dy * dy);
                            double uX = dx / len;
                            double uY = dy / len;
                            p1.X = b.X + uX * z;
                            p1.Y = b.Y + uY * z;
                            p1Edge.p2 = p1;
                            return true;
                        }
                        break;
                    }
                case RelationType.Vertical:
                    {
                        if (Math.Abs(p2s.X - p1s.X) < Var.Eps)
                        {
                            res = false;
                            break;
                        }
                        double a = (p2s.Y - p1s.Y) / (p2s.X - p1s.X);
                        double b = p1s.Y - a * p1s.X;
                        double y3 = a * p1Edge.p2.X + b;
                        p1.X = p1Edge.p2.X;
                        p1.Y = y3;
                        p1Edge.p2.Y = y3;
                        res = true;
                        break;
                    }
                case RelationType.Horizontal:
                    {
                        if (Math.Abs(p2s.X - p1s.X) < Var.Eps)
                        { res = false; break; }
                        double a = (p2s.Y - p1s.Y) / (p2s.X - p1s.X);
                        double b = p1s.Y - a * p1s.X;
                        double x3 = (p1Edge.p2.Y - b) / a;
                        p1.Y = p1Edge.p2.Y;
                        p1.X = x3;
                        p1Edge.p2.X = x3;
                        res = true;
                        break;
                    }
                default:
                    {
                        Point p1new = Geometry.FindIntersection(p1s, p2s, p1Edge.p1, p1Edge.p2);
                        if (p1new.X == -1 && p1new.Y == -1)
                            return false;
                        p1 = p1new;
                        res = p1Edge.AdjustP2(++ind, edgesCount);
                        break;
                    }
            }

            if (!res)
            {
                p1 = p1old;
                p2 = p2old;
            }

            return res;
        }

        // Podajemy z którym wierzchołkiem jest połączony bezpośrednio ten wierzchołek i zwraca
        // punkty, z którymi powinniśmy ustalić współliniowość
        public virtual (Point p1,  Point p2) GetCollinearPoints(int vert)
        {
            if(vert == 2)
                return (p1, p2);
            else
                return (p2, p1);
        }
    }
}
