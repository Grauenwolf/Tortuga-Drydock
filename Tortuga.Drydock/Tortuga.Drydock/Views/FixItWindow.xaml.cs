using System.Windows;
using System.Windows.Input;

namespace Tortuga.Drydock.Views
{
    /// <summary>
    /// Interaction logic for FixItWindow.xaml
    /// </summary>
    public partial class FixItWindow : Window
    {
        public FixItWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
