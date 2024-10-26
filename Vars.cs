using System.Windows.Media;

namespace GK_Proj_1
{
    public static class Var
    {
        public static DrawingStyle DrawingStyle { get; set; }

        public static BezierDrawingStyle BezierDrawingStyle { get; set; }

        public static Brush EdgeColor = Brushes.CadetBlue;

        public static Brush VertColor = Brushes.Purple;

        public static int VertSize = 4;

        public static double Eps = 0.01;

        public static int LineWidth = 3;
    }
}
