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
        }

        private bool drawing = false;
        private Figure drawingFigure = new Figure();
        private Edge drawingAnimationEdge = null;

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(Canva);
            if (drawing && drawingFigure.Edges.Count != 0 && (drawingAnimationEdge.p2 - pt).Length > 2)
            {
                drawingAnimationEdge.p2 = e.GetPosition(Canva);
                Redraw();
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
                if(drawingFigure.IsNearVert(pt,out int ind))
                {
                    if (ind == 0 && drawingFigure.Edges.Count >= 3)
                    {
                        drawingFigure.Edges[drawingFigure.Edges.Count - 1] = new Edge(drawingAnimationEdge.p1,drawingFigure.Edges[0].p1);
                        drawing = false;
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
        public bool IsSealed { get; set; }

        public Figure()
        {
            Edges = new List<Edge>();
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
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
            for(int i = 0; i < Edges.Count; i++)
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
    }

    public class Edge
    {
        public Point p1;
        public Point p2;

        public Edge(Point pnt1, Point pnt2)
        {
            p1 = pnt1;
            p2 = pnt2;
        }

        public void Draw(DrawingContext dc)
        {
            Pen pen = new Pen(Brushes.CadetBlue, 3);
            dc.DrawLine(pen, p1, p2);
            dc.DrawEllipse(Brushes.Purple, null, p1, 4, 4);
        }

        public bool IsNearP1Vert(Point pt)
        {
            if((p1-pt).Length < 10)
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
    }
}