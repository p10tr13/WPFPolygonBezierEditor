using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            Var.BezierDrawingStyle = BezierDrawingStyle.Rasterisation;
        }

        // Variables

        private ContextMenu vertContextMenu, edgeContextMenu;
        private bool drawing = false;
        private Figure drawingFigure = new Figure();
        private Edge drawingAnimationEdge = null; // An edge that follows the cursor when drawing.

        // Variables that indicate which element user clicking the screen had in mind.
        private int selectedVert = -1;
        private int selectedCVert = -1;
        private int selectedEdge = -1;

        // Variables used when dragging elements.
        private bool draggingFigure = false;
        private bool draggingVert = false;
        private bool draggingEdge = false;
        private bool draggingCVert = false;

        private Point addVertPoint = new Point(0, 0); // Mouse press point when adding a vertex to an edge.
        private Point lastMousePosition = new Point(0, 0); // Last mouse position when moving around Canva.

        // Creating a context menu for clicking on the vertex
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

            MenuItem c1MenuItem = new MenuItem { Header = "C1" };
            c1MenuItem.Click += C1MenuItemClick;
            vertContextMenu.Items.Add(c1MenuItem);
        }

        // Creating a context menu for clicking on the edge
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

        // Delets vertex
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

        // Changes vertex type to G0
        private void G0MenuItemClick(object sender, EventArgs e)
        {
            drawingFigure.Edges[selectedVert].vertType = VertRelationType.G0;
            Redraw();
            return;
        }

        // Changes vertex type to G1 (only when adjecent to Bezier)
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

        // Changes vertex type to C1 (only when adjecent to Bezier)
        private void C1MenuItemClick(object sender, EventArgs e)
        {
            if (drawingFigure.Edges[selectedVert].vertType == VertRelationType.C1)
                return;

            int prevVert = -1;
            if (selectedVert == 0)
                prevVert = drawingFigure.Edges.Count - 1;
            else
                prevVert = selectedVert - 1;

            if (drawingFigure.Edges[selectedVert].type != RelationType.Bezier && drawingFigure.Edges[prevVert].type != RelationType.Bezier)
                return;

            drawingFigure.Edges[selectedVert].vertType = VertRelationType.C1;
            drawingFigure.AdjustFigureToVertRelation(selectedVert);
            Redraw();
        }

        // Add new vertex.
        private void AddVertexItemClick(object sender, RoutedEventArgs e)
        {
            Point p1 = drawingFigure.Edges[selectedEdge].p1, p2 = drawingFigure.Edges[selectedEdge].p2;
            Point middle = Geometry.ClosestPointOnEdge(addVertPoint, p1, p2);
            Edge ed = new Edge(middle, drawingFigure.Edges[selectedEdge].p2);
            drawingFigure.AddEdgeAt(selectedEdge + 1, ed);
            drawingFigure.Edges[selectedEdge].AdjustP2(1, drawingFigure.Edges.Count);            
            Redraw();
        }

        // Changes the edge to horizontal.
        private void HorEdgeItemClick(object sender, RoutedEventArgs e)
        {
            if (!drawingFigure.CheckRelationsOfEdge(selectedEdge, RelationType.Horizontal))
                return;
            Edge ed = drawingFigure.Edges[selectedEdge];
            HorizontalEdge hed = new HorizontalEdge(ed.p1,ed.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, hed);
            hed.vertType = ed.vertType; 
            hed.p1Edge.AdjustP2(0, drawingFigure.Edges.Count);
            hed.p2Edge.AdjustP1(0, drawingFigure.Edges.Count);
            // Ta linie jest bez sensu, ale czasem się nie łączyło, dlatego wystarczy poruszyć krawędź o 1 i się naprawia
            hed.p1Edge.MoveP1To(new Point(hed.p1Edge.p1.X + 1, hed.p1Edge.p1.Y), drawingFigure.Edges.Count);
            Redraw();
        }

        // Changes the edge to vertical.
        private void VerEdgeItemClick(object sender, RoutedEventArgs e)
        {
            if (!drawingFigure.CheckRelationsOfEdge(selectedEdge, RelationType.Vertical))
                return;
            Edge ed = drawingFigure.Edges[selectedEdge];
            VerticalEdge ved = new VerticalEdge(ed.p1, ed.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, ved);
            ved.vertType = ed.vertType;
            ved.p1Edge.AdjustP2(0, drawingFigure.Edges.Count);
            ved.p2Edge.AdjustP1(0, drawingFigure.Edges.Count);
            // Ta linie jest bez sensu, ale czasem się nie łączyło, dlatego wystarczy poruszyć krawędź o 1 i się naprawia
            ved.p1Edge.MoveP1To(new Point(ved.p1Edge.p1.X + 1, ved.p1Edge.p1.Y), drawingFigure.Edges.Count);
            Redraw();
        }

        // Changing the edge to one with a fixed length.
        private void FixEdgeItemClick(object sender, RoutedEventArgs e)
        {
            Edge ed = drawingFigure.Edges[selectedEdge];
            FixedLenEdge fix = new FixedLenEdge(ed.p1, ed.p2);
            fix.vertType = ed.vertType;
            LengthInputWindow lWin = new LengthInputWindow(fix.length);
            if (lWin.ShowDialog() == true)
            {
                double newLen = lWin.Lenght;

                drawingFigure.Edges.RemoveAt(selectedEdge);
                drawingFigure.AddEdgeAt(selectedEdge, fix);
                bool res = false;
                fix.length = newLen;
                res = fix.p1Edge.AdjustP2(0, drawingFigure.Edges.Count);
                if (!res) 
                { 
                    MessageBox.Show("Nie da się utworzyć takiego wielokąta", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    drawingFigure.Edges.RemoveAt(selectedEdge);
                    drawingFigure.AddEdgeAt(selectedEdge, ed);
                    return;
                }

                res = fix.p2Edge.AdjustP1(1, drawingFigure.Edges.Count);

                if (!res)
                {
                    MessageBox.Show("Nie da się utworzyć takiego wielokąta", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    drawingFigure.Edges.RemoveAt(selectedEdge);
                    drawingFigure.AddEdgeAt(selectedEdge, ed);
                    return;
                }

                Redraw();
            }
        }

        // Changing edges to a 3rd degree Bézier curve.
        private void BezEdgeItemClick(object sender, RoutedEventArgs e)
        {
            Edge ed = drawingFigure.Edges[selectedEdge];
            BezierEdge bed = new BezierEdge(ed.p1, ed.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, bed);
            bed.p1Edge.AdjustP2(1, drawingFigure.Edges.Count);
            bed.p2Edge.AdjustP1(1, drawingFigure.Edges.Count);
            bed.vertType = VertRelationType.G1;
            bed.p2Edge.vertType = VertRelationType.G1;
            bed.AdjustCP1(0, 0);
            bed.AdjustCP2(0, 0);
            Redraw();
        }

        // Removing a constraint or edge type if it exists.
        private void DeleteRelItemClick(object sender, RoutedEventArgs e)
        {
            if (drawingFigure.Edges[selectedEdge].type == RelationType.Regular)
                return;
            Edge edToDel = drawingFigure.Edges[selectedEdge];
            Edge ed = new Edge(edToDel.p1, edToDel.p2);
            drawingFigure.Edges.RemoveAt(selectedEdge);
            drawingFigure.AddEdgeAt(selectedEdge, ed);
            if (ed.p2Edge.type != RelationType.Bezier)
                ed.p2Edge.vertType = VertRelationType.G0;
            Redraw();
        }

        // Changes the drawing mode to the one implemented by author.
        private void BresenhamRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            Var.DrawingStyle = DrawingStyle.Bresenham;
            Redraw();
            if(windowsRadioButton != null)
                windowsRadioButton.IsChecked = false;
        }

        // Changing the line drawing mode to Windows mode.
        private void WindowsRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            Var.DrawingStyle = DrawingStyle.Windows;
            if(bresenhanRadioButton != null)
                bresenhanRadioButton.IsChecked = false;
        }

        // Mouse movement on Canva.
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

        // Checks whether should start dragging an element or drawing a new edge.
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

        // Opens the context menu if close to an element.
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

        // Releasing the left mouse button = ending the dragging of an element.
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draggingFigure = false;
            draggingVert = false;
            draggingEdge = false;
            draggingCVert = false;
        }

        private void RasterisationRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            Var.BezierDrawingStyle = BezierDrawingStyle.Rasterisation;
            if(easyRadioButton != null)
                easyRadioButton.IsChecked = false;
        }

        private void EasyRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            Var.BezierDrawingStyle = BezierDrawingStyle.Easy;
            Redraw();
            if(rasterisationRadioButton != null)
                rasterisationRadioButton.IsChecked = false;
        }

        private void DrawSampleFigureButton_Click(object sender, RoutedEventArgs e)
        {
            drawing = false;
            drawingFigure.DrawSampleFigure();
            Redraw();
        }

        // Clearing Canva and drawing a polygon.
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