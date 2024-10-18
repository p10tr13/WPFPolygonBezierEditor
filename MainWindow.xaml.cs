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
using GK_Proj_1.Edges;

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
            Var.DrawingStyle = DrawingStyle.Windows;
        }

        // Zmienne
        private ContextMenu vertContextMenu, edgeContextMenu;
        private bool drawing = false;
        private Figure drawingFigure = new Figure();
        private Edge drawingAnimationEdge = null;
        private int selectedVert = -1;
        private int selectedCVert = -1;
        private int selectedEdge = -1;
        private bool draggingFigure = false;
        private bool draggingVert = false;
        private bool draggingEdge = false;
        private bool draggingCVert = false;
        private Point addVertPoint = new Point(0, 0);
        private Point lastMousePosition = new Point(0, 0);

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private void InitializeVertContextMenu()
        {
            vertContextMenu = new ContextMenu();

            MenuItem deleteMenuItem = new MenuItem { Header = "Delete vertex" };
            deleteMenuItem.Click += DeleteMenuItemClick;
            vertContextMenu.Items.Add(deleteMenuItem);

            MenuItem g0MenuItem = new MenuItem { Header = "G0" };
            g0MenuItem.Click += G0MenuItemClick;
            vertContextMenu.Items.Add(g0MenuItem);

            MenuItem g1MenuItem = new MenuItem { Header = "G1" };
            g1MenuItem.Click += G1MenuItemClick;
            vertContextMenu.Items.Add(g1MenuItem);
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

            MenuItem fixEdgeMenuItem = new MenuItem { Header = "Change type to fixed length" };
            edgeContextMenu.Items.Add(fixEdgeMenuItem);
            fixEdgeMenuItem.Click += FixEdgeItemClick;

            MenuItem bezEdgeMenuItem = new MenuItem { Header = "Change type to Bezier" };
            edgeContextMenu.Items.Add(bezEdgeMenuItem);
            bezEdgeMenuItem.Click += BezEdgeItemClick;

            MenuItem deleteRelMenuItem = new MenuItem { Header = "Delete relation if exists" };
            edgeContextMenu.Items.Add(deleteRelMenuItem);
            deleteRelMenuItem.Click += DeleteRelItemClick;
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

        private void G0MenuItemClick(object sender, EventArgs e)
        {
            drawingFigure.Edges[selectedVert].vertType = VertRelationType.G0;
            return;
        }

        private void G1MenuItemClick(object sender, EventArgs e)
        {
            if (drawingFigure.Edges[selectedVert].vertType == VertRelationType.G1)
                return;

            int prevVert = -1;
            if (selectedVert == 0)
                prevVert = drawingFigure.Edges.Count - 1;
            else
                prevVert = selectedVert - 1;

            if (drawingFigure.Edges[selectedVert].type != RelationType.Bezier && drawingFigure.Edges[prevVert].type != RelationType.Bezier)
                return;

            drawingFigure.Edges[selectedVert].vertType = VertRelationType.G1;
            drawingFigure.AdjustFigureToVertRelation(selectedVert);
            Redraw();
        }

        private void AddVertexItemClick(object sender, RoutedEventArgs e)
        {
            Point middle = drawingFigure.Edges[selectedEdge].ClosestPointOnEdge(addVertPoint);
            Edge ed = new Edge(middle, drawingFigure.Edges[selectedEdge].p2);
            drawingFigure.AddEdgeAt(selectedEdge + 1, ed);
            drawingFigure.Edges[selectedEdge].AdjustP2(1, drawingFigure.Edges.Count);            
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
            hed.p1Edge.AdjustP2(0, drawingFigure.Edges.Count);
            hed.p2Edge.AdjustP1(1, drawingFigure.Edges.Count);
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
            ved.p1Edge.AdjustP2(1, drawingFigure.Edges.Count);
            ved.p2Edge.AdjustP1(1, drawingFigure.Edges.Count);
            Redraw();
        }

        private void FixEdgeItemClick(object sender, RoutedEventArgs e)
        {
            Edge ed = drawingFigure.Edges[selectedEdge];
            FixedLenEdge fix = new FixedLenEdge(ed.p1, ed.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, fix);
            Redraw();
        }

        private void BezEdgeItemClick(object sender, RoutedEventArgs e)
        {
            Edge ed = drawingFigure.Edges[selectedEdge];
            BezierEdge bed = new BezierEdge(ed.p1, ed.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, bed);
            bed.p1Edge.AdjustP2(1, drawingFigure.Edges.Count);
            bed.p2Edge.AdjustP1(1, drawingFigure.Edges.Count);
            Redraw();
        }

        private void DeleteRelItemClick(object sender, RoutedEventArgs e)
        {
            if (drawingFigure.Edges[selectedEdge].type == RelationType.Regular)
                return;
            Edge edToDel = drawingFigure.Edges[selectedEdge];
            Edge ed = new Edge(edToDel.p1, edToDel.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, ed);
            Redraw();
        }

        private void BresenhamRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            Var.DrawingStyle = DrawingStyle.Bresenham;
            Redraw();
            if(windowsRadioButton != null)
                windowsRadioButton.IsChecked = false;
        }

        private void WindowsRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            Var.DrawingStyle = DrawingStyle.Windows;
            if(bresenhanRadioButton != null)
                bresenhanRadioButton.IsChecked = false;
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

            if(draggingCVert && (lastMousePosition - pt).Length > 2) 
            {
                if(drawingFigure.TryMoveControlPoint(pt, selectedVert, selectedCVert))
                    Redraw();
                lastMousePosition = pt;
                return;
            }

            if (draggingVert && (drawingFigure.Edges[selectedVert].p1 - pt).Length > 2)
            {
                drawingFigure.TryMoveVert(pt, selectedVert);
                Redraw();
                lastMousePosition = pt;
                return;
            }

            if (draggingEdge && (lastMousePosition - pt).Length > 2)
            {
                drawingFigure.TryMoveEdge(pt.X - lastMousePosition.X, pt.Y - lastMousePosition.Y, selectedEdge);
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
            Point pt = e.GetPosition(Canva);

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

            if (drawingFigure.IsNearControlPoint(pt, out int indv, out int indc))
            {
                selectedVert = indv;
                selectedCVert = indc;
                draggingCVert = true;
                lastMousePosition = pt;
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
            Point pt = e.GetPosition(Canva);
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
            draggingCVert = false;
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
}