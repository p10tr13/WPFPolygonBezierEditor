using GK_Proj_1.Edges;
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

        public void DrawSampleFigure()
        {
            Edges.Clear();
            Point p1 = new Point(100, 100), p2 = new Point(100, 300), p3 = new Point(300, 300), p4 = new Point(500, 100);
            VerticalEdge e1 = new VerticalEdge(p1, p2);
            HorizontalEdge e2 = new HorizontalEdge(p2, p3);
            BezierEdge e3 = new BezierEdge(p3, p4);
            FixedLenEdge e4 = new FixedLenEdge(p4, p1);
            Edges.Add(e1);
            Edges.Add(e2);
            Edges.Add(e3);
            Edges.Add(e4);
            UpdateAllRelations();
            e3.vertType = VertRelationType.G1;
            e4.vertType = VertRelationType.G1;
            e3.AdjustCP1(0, 0);
            e3.AdjustCP2(0, 0);
        }

        // Sprawdzamy czy dany punkt jest blisko któregoś wierzchołka (numeracja po pierwszych wierzchołkach listy krawędzi)
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

        // Sprawdzamy czy dany punkt leży blisko punktów kontrolnych
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

        // Sprawdzamy czy dany punkt jest blisko którejś krawędzi
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

        // Sprawdzamy czy punkt jest w środku wielokąta(nie uwzględnia krzywych beziera)
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

        // Przesuwamy figurę niezależnie od tego, czy będzie na ekranie czy nie
        public void SimpleMove(double x, double y)
        {
            foreach (Edge ed in Edges)
            {
                ed.p1.X += x;
                ed.p2.X += x;
                ed.p1.Y += y;
                ed.p2.Y += y;
                if (ed.type == RelationType.Bezier)
                {
                    BezierEdge bed = ed as BezierEdge;
                    bed.p1c.X += x;
                    bed.p2c.X += x;
                    bed.p1c.Y += y;
                    bed.p2c.Y += y;
                }
            }
        }

        // Aktualizujemy w każdej krawędzi referencje do p1Edge i p2Edge
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

        // Aktualizujemy w danej krawędzi referencje do p1Edge i p2Edge
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

        // Próbujemy przesunąć wierzchołek o indeksie vertind do punktu pt 
        public bool TryMoveVert(Point pt, int vertind)  
        {
            if(Edges[vertind].MoveP1To(pt,Edges.Count))
                return true;
            SimpleMove(pt.X - Edges[vertind].p1.X, pt.Y - Edges[vertind].p1.Y);
            return false;
        }

        // Próbujemy przesunąć punkt kontrolny
        public bool TryMoveControlPoint(Point pt, int vertind, int controlptind) 
        {
            if(Edges[vertind].MoveCPTo(pt, controlptind, Edges.Count))
                return true;
            if (Edges[vertind].type == RelationType.Bezier)
            {
                BezierEdge bed = Edges[vertind] as BezierEdge;
                if (controlptind == 1)
                    SimpleMove(pt.X - bed.p1c.X, pt.Y - bed.p1c.Y);
                else
                    SimpleMove(pt.X - bed.p2c.X, pt.Y - bed.p2c.Y);
                return true;
            }
            return false;
        }

        // Próbujemy przesunąć całą krawędź
        public bool TryMoveEdge(double x, double y, int edgeind)
        {
            if (Edges[edgeind].MoveEdge(x, y, Edges.Count - 1))
                return true;
            SimpleMove(x, y);
            return false;
        }

        // Sprawdzamy, czy możemy usunąć/dodać krawędź o danym typie
        public bool CheckRelationsOfEdge(int ind, RelationType type)
        {
            int p = ind - 1, n = (ind + 1) % Edges.Count;
            if (ind == 0)
                p = Edges.Count - 1;
            return (type == RelationType.Regular || type == RelationType.FixedLen || type == RelationType.Bezier || (Edges[n].type != type && type != Edges[p].type));
        }

        // Przy zmianie typu wierzchołka korygujemy figure
        public void AdjustFigureToVertRelation(int vertind)
        {
            if (Edges[vertind].type == RelationType.Bezier)
            {
                Edges[vertind].AdjustCP1(0,0);
                return;
            }
            if (Edges[vertind].p1Edge.type == RelationType.Bezier)
                Edges[vertind].p1Edge.AdjustCP2(0,0);
            return;
        }
    }
}
