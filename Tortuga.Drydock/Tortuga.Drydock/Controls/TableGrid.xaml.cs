using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tortuga.Drydock.Controls
{
    /// <summary>
    /// Interaction logic for TableGrid.xaml
    /// </summary>
    public partial class TableGrid : UserControl
    {
        public TableGrid()
        {
            InitializeComponent();
        }

        //public static readonly DependencyProperty AnalyzeTableCommandProperty = DependencyProperty.Register("AnalyzeTableCommand", typeof(ICommand), typeof(TableGrid));

        //public ICommand AnalyzeTableCommand
        //{
        //    get { return (ICommand)GetValue(AnalyzeTableCommandProperty); }
        //    set { SetValue(AnalyzeTableCommandProperty, value); }
        //}

    }
}
