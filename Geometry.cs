using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace GK_Proj_1
{
    public static class Geometry
    {
        public static bool OnSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        // Skręt w lewo/prawo/prosto
        public static int Orientation(Point p, Point q, Point r)
        {
            double val = (q.Y - p.Y) * (r.X - q.X) -
                    (q.X - p.X) * (r.Y - q.Y);

            if (Math.Abs(val) < Var.Eps) return 0;

            return (val > 0) ? 1 : 2; 
        }

        // Czy dwa dane odcinki się przecinają?
        public static bool Intersect(Point p1, Point q1, Point p2, Point q2)
        {

            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            if (o1 != o2 && o3 != o4)
                return true;

            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false;
        }

        // Czy trzy punkty są współliniowe?
        public static bool IsCollinear(Point x1, Point x2, Point x3)
        {
            return Math.Abs((x2.X - x1.X) * (x3.Y - x1.Y) - (x2.Y - x1.Y) * (x3.X - x1.X)) < Var.Eps;
        }

        // Zwraca punkt najbliższy punktowi , aby był współliniowy z  i  
        public static Point MovePointToBeCollinear(Point x1, Point x2, Point x3)
        {
            double dx = x2.X - x1.X;
            double dy = x2.Y - x1.Y;

            double t = ((x3.X - x1.X) * dx + (x3.Y - x1.Y) * dy) / (Math.Pow(dx, 2) + Math.Pow(dy, 2));

            return new Point(x1.X + dx * t, x1.Y + dy * t);
        }

        // Zwraca środek pomiędzy dwoma punktami
        public static Point GetMiddle(Point p1, Point p2)
        {
            Point middle = new Point();

            if (p1.X < p2.X)
                middle.X = p1.X + (p2.X - p1.X) / 2;
            else
                middle.X = p2.X + (p1.X - p2.X) / 2;

            if (p1.Y < p2.Y)
                middle.Y = p1.Y + (p2.Y - p1.Y) / 2;
            else
                middle.Y = p2.Y + (p1.Y - p2.Y) / 2;

            return middle;
        }

        // Znajduje punkt przecięcia dwóch prostych na przechodzących przez punkty p1-p2 oraz p3-p4
        public static Point FindIntersection(Point p1, Point p2, Point p3, Point p4)
        {
            if (Math.Abs(p2.X - p1.X) < Var.Eps || Math.Abs(p4.X - p3.X) < Var.Eps)
            {
                return new Point(-1, -1);
            }

            double a1 = (p2.Y - p1.Y) / (p2.X - p1.X);
            double a2 = (p4.Y - p3.Y) / (p4.X - p3.X);

            if (Math.Abs(a1 - a2) < Var.Eps)
            {
                return new Point(-1,-1);
            }

            double b1 = p1.Y - a1 * p1.X;
            double b2 = p3.Y - a2 * p3.X;

            double x = (b2 - b1) / (a1 - a2);
            double y = a1 * x + b1;

            if (double.NaN == x || double.NaN == y)
                return new Point(-1, -1);

            return new Point(x, y);
        }

        // Znajduje punkt naleźący do odcinka p1-p2, który jest najbliższy pt
        public static Point ClosestPointOnEdge(Point pt, Point p1, Point p2)
        {
            double LLS = Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2);
            if (LLS == 0.0)
                return new Point(-1, -1);
            double t = ((pt.X - p1.X) * (p2.X - p1.X) + (pt.Y - p1.Y) * (p2.Y - p1.Y)) / LLS;
            t = Math.Max(0, Math.Min(1, t));
            double closeX = p1.X + t * (p2.X - p1.X);
            double closeY = p1.Y + t * (p2.Y - p1.Y);
            return new Point(closeX, closeY);
        }

        // Sprawdza czy współliniowe punkty p1,p2,p3 mają p1 i p3 po przeciwnej stronie od p2
        public static bool OnTheSameSideofP2(Point p1, Point p2, Point p3)
        {
            return ((p2.X - p1.X) * (p3.X - p2.X) + (p2.Y - p1.Y) * (p3.Y - p2.Y)) < 0;
        }
    }
}
