using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GK_Proj_1
{
   public static class Var
    {
        public static DrawingStyle DrawingStyle { get; set; }

        public static Brush EdgeColor = Brushes.CadetBlue;

        public static Brush VertColor = Brushes.Purple;

        public static int VertSize = 4;

        public static double Eps = 0.01;
    }
}
