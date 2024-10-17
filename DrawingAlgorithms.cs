using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace GK_Proj_1
{
    public static class DrawingAlgorithms
    {
        public static List<(int x,int y)> BresenhamLine(int x1,int y1,int x2, int y2)
        {
            List<(int x, int y)> res = new List<(int x, int y)> ();

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;

            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }

            if (y1 < y2) 
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }
            res.Add((x,y));

            if(dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                while(x != x2)
                {
                    if(d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }
                    res.Add((x, y));
                }
            }
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;

                while (y != y2)
                {
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                    res.Add((x,y));
                }
            }
            return res;
        }

        public static void DrawBresenhamLine(Point p1, Point p2, DrawingContext dc)
        {
            List<(int x, int y)> pixels = BresenhamLine((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y);
            foreach ((int x, int y) in pixels )
            {
                dc.DrawRectangle(Var.EdgeColor, null, new System.Windows.Rect(x,y,3,3));
            }
        }

        // punkty są tutaj nazywane zgodnie z literaturą nie z resztą programu
        // p0 - p3 to odpowiednio początki o końce krzywej Beziera
        public static List<(int x, int y)> DrawBezierCurve(Point p0, Point p1, Point p2, Point p3, DrawingContext dc)
        {
            List<(int x, int y)> pixels = BezierCubicLine((int)p0.X, (int)p0.Y,(int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, (int)p3.X, (int)p3.Y);
            foreach ((int x, int y) in pixels)
            {
                dc.DrawRectangle(Var.EdgeColor, null, new System.Windows.Rect(x, y, 3, 3));
            }
            return pixels;
        }

        // Alois Zingl
        public static List<(int x, int y)> BezierCubicLine(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            List<(int, int)> pixels = new List<(int, int)> ();

            int n = 0, i = 0;
            long xc = x0 + x1 - x2 - x3, xa = xc - 4 * (x1 - x2);
            long xb = x0 - x1 - x2 + x3, xd = xb + 4 * (x1 + x2);
            long yc = y0 + y1 - y2 - y3, ya = yc - 4 * (y1 - y2);
            long yb = y0 - y1 - y2 + y3, yd = yb + 4 * (y1 + y2);
            float fx0 = x0, fx1, fx2, fx3, fy0 = y0, fy1, fy2, fy3;
            double t1 = xb * xb - xa * xc, t2;
            double[] t = new double[5];
            /* sub-divide curve at gradient sign changes */
            if (xa == 0)
            { /* horizontal */
                if (Math.Abs(xc) < 2 * Math.Abs(xb)) t[n++] = xc / (2.0 * xb); /* one change */
            }
            else if (t1 > 0.0)
            { /* two changes */
                t2 = Math.Sqrt(t1);
                t1 = (xb - t2) / xa; if (Math.Abs(t1) < 1.0) t[n++] = t1;
                t1 = (xb + t2) / xa; if (Math.Abs(t1) < 1.0) t[n++] = t1;
            }
            t1 = yb * yb - ya * yc;
            if (ya == 0)
            { /* vertical */
                if (Math.Abs(yc) < 2 * Math.Abs(yb)) t[n++] = yc / (2.0 * yb); /* one change */
            }
            else if (t1 > 0.0)
            { /* two changes */
                t2 = Math.Sqrt(t1);
                t1 = (yb - t2) / ya; if (Math.Abs(t1) < 1.0) t[n++] = t1;
                t1 = (yb + t2) / ya; if (Math.Abs(t1) < 1.0) t[n++] = t1;
            }
            for (i = 1; i < n; i++) /* bubble sort of 4 points */
                if ((t1 = t[i - 1]) > t[i]) { t[i - 1] = t[i]; t[i] = t1; i = 0; }
            t1 = -1.0; t[n] = 1.0; /* begin / end point */
            for (i = 0; i <= n; i++)
            { /* plot each segment separately */
                t2 = t[i]; /* sub-divide at t[i-1], t[i] */
                fx1 = (float)((t1 * (t1 * xb - 2 * xc) - t2 * (t1 * (t1 * xa - 2 * xb) + xc) + xd) / 8 - fx0);
                fy1 = (float)((t1 * (t1 * yb - 2 * yc) - t2 * (t1 * (t1 * ya - 2 * yb) + yc) + yd) / 8 - fy0);
                fx2 = (float)((t2 * (t2 * xb - 2 * xc) - t1 * (t2 * (t2 * xa - 2 * xb) + xc) + xd) / 8 - fx0);
                fy2 = (float)((t2 * (t2 * yb - 2 * yc) - t1 * (t2 * (t2 * ya - 2 * yb) + yc) + yd) / 8 - fy0);
                fx0 -= fx3 = (float)((t2 * (t2 * (3 * xb - t2 * xa) - 3 * xc) + xd) / 8);
                fy0 -= fy3 = (float)((t2 * (t2 * (3 * yb - t2 * ya) - 3 * yc) + yd) / 8);
                x3 = (int)Math.Floor(fx3 + 0.5); y3 = (int)Math.Floor(fy3 + 0.5); /* scale bounds to int */
                if (fx0 != 0.0) { fx1 *= fx0 = (x0 - x3) / fx0; fx2 *= fx0; }
                if (fy0 != 0.0) { fy1 *= fy0 = (y0 - y3) / fy0; fy2 *= fy0; }
                if (x0 != x3 || y0 != y3) /* segment t1 - t2 */
                    pixels.AddRange(BezierCubicSeg(x0, y0, x0 + fx1, y0 + fy1, x0 + fx2, y0 + fy2, x3, y3));
                x0 = x3; y0 = y3; fx0 = fx3; fy0 = fy3; t1 = t2;
            }
            return pixels;
        }

        public static List<(int x, int y)> BezierCubicSeg(int x0, int y0, float x1, float y1, float x2, float y2, int x3, int y3)
        {
            List<(int x, int y)> pixels = new List<(int x, int y)>();

            int f, fx, fy, leg = 1;
            // Kierunek przyrostu
            int sx = x0 < x3 ? 1 : -1, sy = y0 < y3 ? 1 : -1;

            float xc = -Math.Abs(x0 + x1 - x2 - x3), xa = xc - 4 * sx * (x1 - x2), xb = sx * (x0 - x1 - x2 + x3);
            float yc = -Math.Abs(y0 + y1 - y2 - y3), ya = yc - 4 * sy * (y1 - y2), yb = sy * (y0 - y1 - y2 + y3);
            double ab, ac, bc, cb, xx, xy, yy, dx, dy, ex, pxy;

            if (xa == 0 && ya == 0)
            { /* quadratic Bezier */
                sx = (int)Math.Floor((3 * x1 - x0 + 1) / 2); sy = (int)Math.Floor((3 * y1 - y0 + 1) / 2); /* new midpoint */
                return BezierQuadSeg(x0, y0, sx, sy, x3, y3);
            }

            x1 = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0) + 1;
            x2 = (x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3) + 1;

            do
            {
                ab = xa * yb - xb * ya; ac = xa * yc - xc * ya; bc = xb * yc - xc * yb;
                ex = ab * (ab + ac - 3 * bc) + ac * ac; /* P0 part of self-intersection loop? */
                f = ex > 0 ? 1 : (int)Math.Sqrt(1 + 1024 / x1); /* calculate resolution */
                ab *= f; ac *= f; bc *= f; ex *= f * f; /* increase resolution */
                xy = 9 * (ab + ac + bc) / 8; cb = 8 * (xa - ya);/* init differences of 1st degree */
                dx = 27 * (8 * ab * (yb * yb - ya * yc) + ex * (ya + 2 * yb + yc)) / 64 - ya * ya * (xy - ya);
                dy = 27 * (8 * ab * (xb * xb - xa * xc) - ex * (xa + 2 * xb + xc)) / 64 - xa * xa * (xy + xa);
                /* init differences of 2nd degree */
                xx = 3 * (3 * ab * (3 * yb * yb - ya * ya - 2 * ya * yc) - ya * (3 * ac * (ya + yb) + ya * cb)) / 4;
                yy = 3 * (3 * ab * (3 * xb * xb - xa * xa - 2 * xa * xc) - xa * (3 * ac * (xa + xb) + xa * cb)) / 4;
                xy = xa * ya * (6 * ab + 6 * ac - 3 * bc + cb); ac = ya * ya; cb = xa * xa;
                xy = 3 * (xy + 9 * f * (cb * yb * yc - xb * xc * ac) - 18 * xb * yb * ab) / 8;

                if(ex < 0)
                {
                    dx = -dx; dy = -dy; xx = -xx; yy = -yy; xy = -xy; ac = -ac; cb = -cb;
                }
                ab = 6 * ya * ac; ac = -6 * xa * ac; bc = 6 * ya * cb; cb = -6 * xa * cb;
                dx += xy; ex = dx + dy; dy += xy;

                for (pxy = xy, fx = fy = f; x0 != x3 && y0 != y3;)
                {
                    pixels.Add((x0, y0));
                    do
                    { /* move sub-steps of one pixel */
                        if (dx > pxy || dy < pxy) goto exit; /* confusing values */
                        y1 = (float)(2 * ex - dy); /* save value for test of y step */
                        if (2 * ex >= dx)
                        { /* x sub-step */
                            fx--; ex += dx += xx; dy += xy += ac; yy += bc; xx += ab;
                        }
                        if (y1 <= 0)
                        { /* y sub-step */
                            fy--; ex += dy += yy; dx += xy += bc; xx += ac; yy += cb;
                        }
                    } while (fx > 0 && fy > 0); /* pixel complete? */
                    if (2 * fx <= f) { x0 += sx; fx += f; } /* x step */
                    if (2 * fy <= f) { y0 += sy; fy += f; } /* y step */
                    if (pxy == xy && dx < 0 && dy > 0) pxy = Var.Eps;/* pixel ahead valid */
                }
            exit: xx = x0; x0 = x3; x3 = (int)xx; sx = -sx; xb = -xb; /* swap legs */
                yy = y0; y0 = y3; y3 = (int)yy; sy = -sy; yb = -yb; x1 = x2;
            } while (leg-- != 0);
            pixels.AddRange(BresenhamLine(x0, y0, x3, y3));
            return pixels;
        }

        public static List<(int x, int y)> BezierQuadSeg(int x0, int y0, int x1, int y1, int x2, int y2)
        {
            List<(int x, int y)> pixels = new List<(int x, int y)> ();
            int sx = x2 - x1, sy = y2 - y1;
            long xx = x0 - x1, yy = y0 - y1, xy; /* relative values for checks */
            double dx, dy, err, cur = xx * sy - yy * sx; /* curvature */
            if(xx * sx <= 0 && yy * sy <= 0)/* sign of gradient must not change */
                return pixels;
            if (sx * (long)sx + sy * (long)sy > xx * xx + yy * yy)
            { /* begin with longer part */
                x2 = x0; x0 = sx + x1; y2 = y0; y0 = sy + y1; cur = -cur; /* swap P0 P2 */
            }
            if (cur != 0)
            { /* no straight line */
                xx += sx; xx *= sx = x0 < x2 ? 1 : -1; /* x step direction */
                yy += sy; yy *= sy = y0 < y2 ? 1 : -1; /* y step direction */
                xy = 2 * xx * yy; xx *= xx; yy *= yy; /* differences 2nd degree */
                if (cur * sx * sy < 0)
                { /* negated curvature? */
                    xx = -xx; yy = -yy; xy = -xy; cur = -cur;
                }
                dx = 4.0 * sy * cur * (x1 - x0) + xx - xy; /* differences 1st degree */
                dy = 4.0 * sx * cur * (y0 - y1) + yy - xy;
                xx += xx; yy += yy; err = dx + dy + xy; /* error 1st step */
                do
                {
                    pixels.Add((x0, y0)); /* plot curve */
                    if (x0 == x2 && y0 == y2) return pixels; /* last pixel -> curve finished */
                    y1 = 2 * err < dx ? 1 : 0; /* save value for test of y step */
                    if (2 * err > dy) { x0 += sx; dx -= xy; err += dy += yy; } /* x step */
                    if (y1 == 1) { y0 += sy; dy -= xy; err += dx += xx; } /* y step */
                } while (dy < 0 && dx > 0); /* gradient negates -> algorithm fails */
            }
            pixels.AddRange(BresenhamLine(x0, y0, x2, y2));
            return pixels;
        }
    }
}
