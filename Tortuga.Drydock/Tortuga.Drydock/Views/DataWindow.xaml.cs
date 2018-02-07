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

namespace Tortuga.Drydock.Views
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        public DataWindow()
        {
            InitializeComponent();
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            BindingBase bindingBase = null;

            var dataGridBoundColumn = e.Column as DataGridBoundColumn;
            if (dataGridBoundColumn != null)
            {
                bindingBase = dataGridBoundColumn.Binding;
            }
            else
            {
                var dataGridComboBoxColumn = e.Column as DataGridComboBoxColumn;
                if (dataGridComboBoxColumn != null)
                    bindingBase = dataGridComboBoxColumn.SelectedItemBinding;
            }

            var binding = bindingBase as Binding;
            if (binding != null)
            {
                binding.NotifyOnTargetUpdated = true;

                e.Column.CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters =
            {
                new EventSetter(Binding.TargetUpdatedEvent, new EventHandler<DataTransferEventArgs>(OnDataGridCellBindingTargetUpdated))
            }
                };
            }
        }

        private static void OnDataGridCellBindingTargetUpdated(object sender, DataTransferEventArgs e)
        {
            var dataGridCell = (DataGridCell)sender;

            // Get context: column and item.
            var column = dataGridCell.Column;
            var item = dataGridCell.DataContext;

            // TODO: based on context, format DataGridCell instance.
            if(((System.Data.DataRowView)item).Row.ItemArray[column.DisplayIndex] is DBNull)
                dataGridCell.Background = Brushes.BlanchedAlmond;
        }
    }
}
