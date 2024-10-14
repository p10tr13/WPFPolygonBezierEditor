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
                dc.DrawRectangle(Var.EdgeColor, null, new System.Windows.Rect(x,y,2,2));
            }
        }
    }
}
