using GK_Proj_1.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace GK_Proj_1
{
    public class Figure
    {
        public List<Edge> Edges { get; set; }

        public Figure()
        {
            Edges = new List<Edge>();
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
        }

        public void AddEdgeAt(int ind, Edge edge)
        {
            Edges.Insert(ind, edge);
            UpdateRelation(ind);
        }

        public void Draw(DrawingContext dc)
        {
            foreach (Edge edge in Edges)
            {
                edge.Draw(dc);
            }
        }

        //Sprawdzamy czy dany punkt jest blisko któregoś wierzchołka (numeracja po pierwszych wierzchołkach listy krawędzi)
        public bool IsNearVert(Point pt, out int ind)
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].IsNearP1Vert(pt))
                {
                    ind = i;
                    return true;
                }
            }
            ind = -1;
            return false;
        }

        //Sprawdzamy czy dany punkt leży blisko punktów kontrolnych
        public bool IsNearControlPoint(Point pt, out int indv, out int indc)
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].IsNearControlPoint(pt,out indc))
                {
                    indv = i;
                    return true;
                }
            }
            indv = -1;
            indc = -1;
            return false;
        }

        //Sprawdzamy czy dany punkt jest blisko którejś krawędzi
        public bool IsNearEdge(Point pt, out int ind)
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].IsNearEdge(pt))
                {
                    ind = i;
                    return true;
                }
            }
            ind = -1;
            return false;
        }

        //Sprawdzamy czy punkt jest w środku wielokąta
        public bool IsPointInside(Point pt)
        {
            double x = pt.X, y = pt.Y;
            bool inside = false;

            Point p1 = Edges[0].p1, p2;

            for (int i = 1; i <= Edges.Count; i++)
            {
                // Get the next point in the polygon
                p2 = Edges[i % Edges.Count].p1;

                // Check if the point is above the minimum y coordinate of the edge
                if (y > Math.Min(p1.Y, p2.Y))
                {
                    // Check if the point is below the maximum y coordinate of the edge
                    if (y <= Math.Max(p1.Y, p2.Y))
                    {
                        // Check if the point is to the left of the maximum x coordinate of the edge
                        if (x <= Math.Max(p1.X, p2.X))
                        {
                            // Calculate the x-intersection of the line connecting the point to the edge
                            double xIntersection = (y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;

                            // Check if the point is on the same line as the edge or to the left of the x-intersection
                            if (p1.X == p2.X || x <= xIntersection)
                            {
                                inside = !inside;
                            }
                        }
                    }
                }
                p1 = p2;
            }
            return inside;
        }

        public int Move(double x, double y, double maxX, double maxY)
        {
            int res = 0;
            bool canX = true, canY = true;
            foreach (Edge ed in Edges)
            {
                if (ed.p1.X + x > maxX || ed.p1.X + x < 0)
                    canX = false;
                if (ed.p1.Y + y > maxY || ed.p2.Y + y < 0)
                    canY = false;
                if (!canY && !canX)
                    return res;
            }

            if (canX)
            {
                MoveX(x);
                res++;
            }

            if (canY)
            {
                MoveY(y);
                res += 2;
            }

            return res;
        }

        public void SimpleMove(double x, double y)
        {
            MoveX(x);
            MoveY(y);
        }

        public void MoveVert(double x, double y, int vertind)
        {
            if (vertind == 0)
            {
                Edges[0].p1.X += x;
                Edges[0].p1.Y += y;
                Edges[Edges.Count - 1].p2.X += x;
                Edges[Edges.Count - 1].p2.Y += y;
                return;
            }

            Edges[vertind].p1.X += x;
            Edges[vertind].p1.Y += y;
            Edges[vertind - 1].p2.X += x;
            Edges[vertind - 1].p2.Y += y;
        }

        public void MoveEdge(double x, double y, int edgeind)
        {
            if (edgeind == 0)
            {
                Edges[0].p1.X += x;
                Edges[0].p1.Y += y;
                Edges[0].p2.X += x;
                Edges[0].p2.Y += y;
                Edges[Edges.Count - 1].p2.X += x;
                Edges[Edges.Count - 1].p2.Y += y;
                Edges[1].p1.X += x;
                Edges[1].p1.Y += y;
                return;
            }

            if (edgeind == Edges.Count - 1)
            {
                Edges[edgeind].p1.X += x;
                Edges[edgeind].p1.Y += y;
                Edges[edgeind].p2.X += x;
                Edges[edgeind].p2.Y += y;
                Edges[edgeind - 1].p2.X += x;
                Edges[edgeind - 1].p2.Y += y;
                Edges[0].p1.X += x;
                Edges[0].p1.Y += y;
                return;
            }

            Edges[edgeind].p1.X += x;
            Edges[edgeind].p1.Y += y;
            Edges[edgeind].p2.X += x;
            Edges[edgeind].p2.Y += y;
            Edges[edgeind - 1].p2.X += x;
            Edges[edgeind - 1].p2.Y += y;
            Edges[edgeind + 1].p1.X += x;
            Edges[edgeind + 1].p1.Y += y;
        }

        public void MoveY(double y)
        {
            foreach (Edge ed in Edges)
            {
                ed.p1.Y += y;
                ed.p2.Y += y;
            }
        }

        public void MoveX(double x)
        {
            foreach (Edge ed in Edges)
            {
                ed.p1.X += x;
                ed.p2.X += x;
            }
        }

        public void UpdateAllRelations()
        {
            int p = Edges.Count - 1, n = 1;
            for (int i = 0; i < Edges.Count; i++)
            {
                Edges[i].p1Edge = Edges[p];
                Edges[i].p2Edge = Edges[n];
                p = (++p) % Edges.Count;
                n = (++n) % Edges.Count;
            }
        }

        public void UpdateRelation(int ind)
        {
            Edge edge = Edges[ind];
            if (ind == Edges.Count - 1)
            {
                Edges[0].p1Edge = edge;
                edge.p1Edge = Edges[ind - 1];
                edge.p2Edge = Edges[0];
                Edges[ind - 1].p2Edge = edge;
                return;
            }
            if (ind == 0)
            {
                Edges[Edges.Count - 1].p2Edge = edge;
                Edges[ind + 1].p1Edge = edge;
                edge.p1Edge = Edges[Edges.Count - 1];
                edge.p2Edge = Edges[1];
                return;
            }
            Edges[ind - 1].p2Edge = edge;
            Edges[ind + 1].p1Edge = edge;
            edge.p1Edge = Edges[ind - 1];
            edge.p2Edge = Edges[ind + 1];
        }

        public bool TryMoveVert(Point pt, int vertind)  
        {
            if(Edges[vertind].MoveP1To(pt,Edges.Count))
                return true;
            SimpleMove(pt.X - Edges[vertind].p1.X, pt.Y - Edges[vertind].p1.Y);
            return false;
        }

        public bool TryMoveControlPoint(Point pt, int vertind, int controlptind) 
        {
            if (Edges[vertind].MoveCPTo(pt, controlptind, Edges.Count))
                return true;
            return false;
        }

        public bool TryMoveEdge(double x, double y, int edgeind)
        {
            if (Edges[edgeind].MoveEdge(x, y, Edges.Count - 1))
                return true;
            SimpleMove(x, y);
            return false;
        }

        public bool CheckRelationsWithoutEdge(int ind)
        {
            if (Edges.Count == 3)
                return false;
            int p = ind - 1, n = (ind + 1) % Edges.Count;
            if (ind == 0)
                p = Edges.Count - 1;

            return (Edges[n].type == RelationType.Regular || Edges[n].type == RelationType.FixedLen || Edges[n].type == RelationType.Bezier || !(Edges[p].type == Edges[n].type));
        }

        public bool CheckRelationsOfEdge(int ind, RelationType type)
        {
            int p = ind - 1, n = (ind + 1) % Edges.Count;
            if (ind == 0)
                p = Edges.Count - 1;
            return (type == RelationType.Regular || type == RelationType.FixedLen || type == RelationType.Bezier || (Edges[n].type != type && type != Edges[p].type));
        }
    }
}
