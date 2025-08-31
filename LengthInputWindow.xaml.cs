using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GK_Proj_1
{
    /// <summary>
    /// Interaction logic for LengthInputWindow.xaml
    /// </summary>
    public partial class LengthInputWindow : Window
    {
        public double Lenght {get; set; }

        public LengthInputWindow(double currentLength)
        {
            InitializeComponent();
            LengthTextBox.Text = Math.Round(currentLength).ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(LengthTextBox.Text, out double newLength))
            {
                if (newLength <= 0)
                {
                    MessageBox.Show("Enter the correct number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Lenght = newLength;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Enter the correct number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
