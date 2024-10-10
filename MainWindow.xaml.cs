using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace GK_Proj_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeVertContextMenu();
            InitializeEdgeContextMenu();
        }

        private ContextMenu vertContextMenu, edgeContextMenu;
        private bool drawing = false;
        private Figure drawingFigure = new Figure();
        private Edge drawingAnimationEdge = null;
        private int selectedVert = -1;
        private int selectedEdge = -1;
        private bool draggingFigure = false;
        private bool draggingVert = false;
        private bool draggingEdge = false;
        private Point addVertPoint = new Point(0, 0);
        private Point lastMousePosition = new Point(0, 0);

        private void InitializeVertContextMenu()
        {
            vertContextMenu = new ContextMenu();
            MenuItem deleteMenuItem = new MenuItem { Header = "정점을 제거" };
            deleteMenuItem.Click += DeleteMenuItemClick;
            vertContextMenu.Items.Add(deleteMenuItem);
        }

        private void InitializeEdgeContextMenu()
        {
            edgeContextMenu = new ContextMenu();
            MenuItem addVertMenuItem = new MenuItem { Header = "Add vertex" };
            edgeContextMenu.Items.Add(addVertMenuItem);
            addVertMenuItem.Click += AddVertexItemClick;
            MenuItem horEdgeMenuItem = new MenuItem { Header = "Change type to horizontal" };
            edgeContextMenu.Items.Add(horEdgeMenuItem);
            horEdgeMenuItem.Click += HorEdgeItemClick;
            MenuItem verEdgeMenuItem = new MenuItem { Header = "Change type to vertical" };
            edgeContextMenu.Items.Add(verEdgeMenuItem);
            verEdgeMenuItem.Click += VerEdgeItemClick;
        }

        private void DeleteMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (drawingFigure.Edges.Count == 3)
            {
                drawingFigure.Edges.Clear();
                Redraw();
                return;
            }

            Edge ed = new Edge(new Point(0, 0), drawingFigure.Edges[selectedVert].p2);

            if (0 == selectedVert)
            {
                drawingFigure.Edges.RemoveAt(0);
                drawingFigure.Edges.Add(ed);
                ed.p1 = drawingFigure.Edges[drawingFigure.Edges.Count - 2].p1;
                drawingFigure.Edges.RemoveAt(drawingFigure.Edges.Count - 2);
                drawingFigure.UpdateRelation(drawingFigure.Edges.Count - 1);
                Redraw();
                return;
            }

            drawingFigure.Edges.RemoveAt(selectedVert);
            drawingFigure.AddEdgeAt(selectedVert,ed);
            ed.p1 = drawingFigure.Edges[selectedVert - 1].p1;
            drawingFigure.Edges.RemoveAt(selectedVert - 1);
            drawingFigure.UpdateRelation(selectedVert - 1);
            Redraw();
            return;
        }

        private void AddVertexItemClick(object sender, RoutedEventArgs e)
        {
            Point middle = drawingFigure.Edges[selectedEdge].ClosestPointOnEdge(addVertPoint);
            Edge ed = new Edge(middle, drawingFigure.Edges[selectedEdge].p2);
            drawingFigure.AddEdgeAt(selectedEdge + 1, ed);
            drawingFigure.Edges[selectedEdge].AdjustP2();            
            Redraw();
        }

        private void HorEdgeItemClick(object sender, RoutedEventArgs e)
        {
            if (!drawingFigure.CheckRelationsOfEdge(selectedEdge, RelationType.Horizontal))
                return;
            Edge ed = drawingFigure.Edges[selectedEdge];
            HorizontalEdge hed = new HorizontalEdge(ed.p1,ed.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, hed);
            hed.p1Edge.AdjustP2();
            hed.p2Edge.AdjustP1();
            Redraw();
        }

        private void VerEdgeItemClick(object sender, RoutedEventArgs e)
        {
            if (!drawingFigure.CheckRelationsOfEdge(selectedEdge, RelationType.Vertical))
                return;
            Edge ed = drawingFigure.Edges[selectedEdge];
            VerticalEdge ved = new VerticalEdge(ed.p1, ed.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, ved);
            ved.p1Edge.AdjustP2();
            ved.p2Edge.AdjustP1();
            Redraw();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(Canva);
            if (drawing && drawingFigure.Edges.Count != 0 && (drawingAnimationEdge.p2 - pt).Length > 2)
            {
                drawingAnimationEdge.p2 = e.GetPosition(Canva);
                Redraw();
                return;
            }

            if (draggingVert && (lastMousePosition - pt).Length > 2)
            {
                //drawingFigure.MoveVert(pt.X - lastMousePosition.X, pt.Y - lastMousePosition.Y, selectedVert);
                //drawingFigure.AdjustToVert(pt,selectedVert);
                if (!drawingFigure.TryMoveVert(pt, selectedVert))
                    return;
                Redraw();
                lastMousePosition = pt;
                return;
            }

            if (draggingEdge && (lastMousePosition - pt).Length > 2)
            {
                drawingFigure.MoveEdge(pt.X - lastMousePosition.X, pt.Y - lastMousePosition.Y, selectedEdge);
                Redraw();
                lastMousePosition = pt;
                return;
            }

            if (draggingFigure && (lastMousePosition - pt).Length > 2)
            {
                drawingFigure.SimpleMove(pt.X - lastMousePosition.X, pt.Y - lastMousePosition.Y);
                Redraw();
                lastMousePosition = pt;
            }

        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(this);

            if (!drawing && drawingFigure.Edges.Count <= 1)
            {
                drawingAnimationEdge = new Edge(pt, pt);
                drawing = true;
                drawingFigure.Edges.Add(drawingAnimationEdge);
                Redraw();
                return;
            }
            if (drawing)
            {
                if (drawingFigure.IsNearVert(pt, out int ind))
                {
                    if (ind == 0 && drawingFigure.Edges.Count >= 3)
                    {
                        drawingFigure.Edges[drawingFigure.Edges.Count - 1] = new Edge(drawingAnimationEdge.p1, drawingFigure.Edges[0].p1);
                        drawing = false;
                        drawingFigure.UpdateAllRelations();
                        Redraw();
                    }
                    return;
                }
                Edge ed = new Edge(drawingAnimationEdge.p1, pt);
                drawingFigure.Edges[drawingFigure.Edges.Count - 1] = ed;
                drawingFigure.Edges.Add(drawingAnimationEdge);
                drawingAnimationEdge.p1 = pt;
                Redraw();
                return;
            }

            if (drawingFigure.IsNearVert(pt, out int vertind))
            {
                selectedVert = vertind;
                draggingVert = true;
                lastMousePosition = pt;
                return;
            }

            if(drawingFigure.IsNearEdge(pt, out int edgeind))
            {
                selectedEdge = edgeind;
                draggingEdge = true;
                lastMousePosition = pt;
                return;
            }

            if (drawingFigure.IsPointInside(pt))
            {
                lastMousePosition = pt;
                draggingFigure = true;
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(this);
            if (drawing || drawingFigure.Edges.Count < 2)
                return;
            if (drawingFigure.IsNearVert(pt, out int vertind))
            {
                selectedVert = vertind;
                vertContextMenu.PlacementTarget = Canva;
                vertContextMenu.IsOpen = true;
                return;
            }
            if (drawingFigure.IsNearEdge(pt, out int edgind))
            {
                addVertPoint = pt;
                selectedEdge = edgind;
                edgeContextMenu.PlacementTarget = Canva;
                edgeContextMenu.IsOpen = true;
                return;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draggingFigure = false;
            draggingVert = false;
            draggingEdge = false;
        }

        private void Redraw()
        {
            Canva.Children.Clear();
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                drawingFigure.Draw(dc);
            }
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)Canva.ActualWidth, (int)Canva.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            Image img = new Image { Source = rtb };
            Canva.Children.Add(img);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

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

            if(edgeind == Edges.Count - 1)
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
            if(ind == Edges.Count - 1)
            {
                Edges[0].p1Edge = edge;
                edge.p1Edge = Edges[ind - 1];
                edge.p2Edge = Edges[0];
                Edges[ind - 1].p2Edge = edge;
                return;
            }
            if(ind == 0)
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
            return Edges[vertind].MoveP1To(pt);
        }

        public bool CheckRelationsWithoutEdge(int ind)
        {
            if(Edges.Count == 3)
                return false;
            int p = ind - 1, n = (ind + 1) % Edges.Count;
            if (ind == 0)
                p = Edges.Count - 1;

            return (Edges[n].type == RelationType.Regular || !(Edges[p].type == Edges[n].type));
        }

        public bool CheckRelationsOfEdge(int ind, RelationType type)
        {
            int p = ind - 1, n = (ind + 1) % Edges.Count;
            if(ind == 0)
                p =  Edges.Count - 1;
            return (type == RelationType.Regular || (Edges[n].type != type && type != Edges[p].type));
        }
    }

    public class Edge
    {
        public RelationType type { get; set; }
        public Edge? p1Edge, p2Edge;
        public Point p1;
        public Point p2;

        public Edge(Point pnt1, Point pnt2)
        {
            p1 = pnt1;
            p2 = pnt2;
            type = RelationType.Regular;
        }

        public void Draw(DrawingContext dc)
        {
            Pen pen = new Pen(Brushes.CadetBlue, 3);
            dc.DrawLine(pen, p1, p2);
            dc.DrawEllipse(Brushes.Purple, null, p1, 4, 4);
        }

        public bool IsNearP1Vert(Point pt)
        {
            if ((p1 - pt).Length < 10)
            {
                return true;
            }
            return false;
        }

        public bool IsNearP2Vert(Point pt)
        {
            if ((p2 - pt).Length < 10)
            {
                return true;
            }
            return false;
        }

        public Point ClosestPointOnEdge(Point pt)
        {
            double LLS = Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2);
            if (LLS == 0.0)
                return new Point(-1,-1);
            double t = ((pt.X - p1.X) * (p2.X - p1.X) + (pt.Y - p1.Y) * (p2.Y - p1.Y)) / LLS;
            t = Math.Max(0, Math.Min(1, t));
            double closeX = p1.X + t * (p2.X - p1.X);
            double closeY = p1.Y + t * (p2.Y - p1.Y);
            return new Point(closeX, closeY);
        }

        public bool IsNearEdge(Point pt)
        {
            Point closestpt = ClosestPointOnEdge(pt);
            if(closestpt.X == -1 && closestpt.Y == -1)
                return false;
            return (closestpt - pt).Length < 10;
        }

        public virtual bool AdjustP1()
        {
            if(p1Edge == null)
                return false;
            p1 = p1Edge.p2;
            return true;
        }

        public virtual bool AdjustP2()
        {
            if (p2Edge == null)
                return false;
            p2 = p2Edge.p1;
            return true;
        }

        public virtual bool MoveP1To(Point pt)
        {
            p1 = pt;
            return p1Edge.AdjustP2();
        }
    }

    public class HorizontalEdge : Edge
    {
        public HorizontalEdge(Point p1, Point p2): base(p1, new Point(p2.X, p1.Y)) { base.type = RelationType.Horizontal; }

        public override bool AdjustP1()
        {
            if(p1Edge == null)
                return false;
            if (p1.Y == p1Edge.p2.Y)
            {
                p1 = p1Edge.p2;
                return true;
            }

            switch (p2Edge.type)
            {
                case RelationType.FixedLen:
                    {
                        return true;
                    }
                default:
                    {
                        Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
                        bool res = false;
                        p1 = p1Edge.p2;
                        p2 = new Point(p2.X , p1.Y);
                        if (p2Edge != null)
                            res = p2Edge.AdjustP1();
                        if(!res)
                        {
                            p1 = oldp1;
                            p2 = oldp2;
                        }
                        return res;
                    }
            }
        }

        public override bool AdjustP2()
        {
            if(p2Edge == null)
                return false;
            if(p1.Y == p2Edge.p1.Y)
            {
                p2 = p2Edge.p1;
                return true;
            }

            switch(p1Edge.type) 
            {
                case RelationType.FixedLen:
                    {
                        return true;
                    }
                default: 
                    {
                        Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
                        bool res = false;
                        p2 = p2Edge.p1;
                        p1 = new Point(p1.X, p2.Y);
                        if(p1Edge != null)
                            res = p1Edge.AdjustP2();
                        if(!res) 
                        {
                            p1 = oldp1;
                            p2 = oldp2;
                        }
                        return res;
                    }
            }
        }

        public override bool MoveP1To(Point pt)
        {
            if((pt - p2).Length <= 2)
                return false;
            Point oldp1 = new Point(base.p1.X, base.p1.Y), oldp2 = new Point(base.p2.X, base.p2.Y);
            base.p1 = pt;
            p2.Y = pt.Y;
            bool res = p1Edge.AdjustP2();
            if(!res)
            {
                base.p1 = oldp1;
                base.p2 = oldp2;
                return res;
            }
            return base.p2Edge.AdjustP1();
        }
    }

    public class VerticalEdge : Edge
    {
        public VerticalEdge(Point p1, Point p2) : base(p1, new Point(p1.X, p2.Y)) { base.type = RelationType.Vertical; }

        public override bool AdjustP1()
        {
            if (p1Edge == null)
                return false;
            if (p1.X == p1Edge.p2.X)
            {
                p1 = p1Edge.p2;
                return true;
            }

            switch (p2Edge.type)
            {
                case RelationType.FixedLen:
                    {
                        return true;
                    }
                default:
                    {
                        Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
                        bool res = false;
                        p1 = p1Edge.p2;
                        p2 = new Point(p1.X, p2.Y);
                        if (p2Edge != null)
                            res = p2Edge.AdjustP1();
                        if(!res) 
                        {
                            p1 = oldp1;
                            p2 = oldp2;
                        }
                        return res;
                    }
            }
        }

        public override bool AdjustP2()
        {
            if (p2Edge == null)
                return false;
            if (p1.X == p2Edge.p1.X)
            {
                p2 = p2Edge.p1;
                return true;
            }

            switch (p1Edge.type)
            {
                case RelationType.FixedLen:
                    {
                        return true;
                    }
                default:
                    {
                        Point oldp1 = new Point(p1.X, p1.Y), oldp2 = new Point(p2.X, p2.Y);
                        bool res = false;
                        p2 = p2Edge.p1;
                        p1 = new Point(p2.X, p1.Y);
                        if (p1Edge != null)
                            res = p1Edge.AdjustP2();
                        if(!res)
                        {
                            p1 = oldp1;
                            p2 = oldp2;
                        }
                        return res;
                    }
            }
        }

        public override bool MoveP1To(Point pt)
        {
            if ((pt - p2).Length <= 2)
                return false;
            Point oldp1 = new Point(base.p1.X, base.p1.Y), oldp2 = new Point(base.p2.X, base.p2.Y);
            base.p1 = pt;
            base.p2.X = pt.X;
            bool res = base.p1Edge.AdjustP2();
            if (!res)
            {
                base.p1 = oldp1;
                base.p2 = oldp2;
                return res;
            }
            return base.p2Edge.AdjustP1();
        }
    }

    public class FixedLenEdge : Edge
    {
        public FixedLenEdge(Point p1, Point p2) : base(p1,p2){ }
    }
}