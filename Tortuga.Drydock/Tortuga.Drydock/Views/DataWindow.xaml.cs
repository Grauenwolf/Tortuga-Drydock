using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            BindingBase bindingBase = null;

            if (e.Column is DataGridBoundColumn dataGridBoundColumn)
            {
                bindingBase = dataGridBoundColumn.Binding;
            }
            else
            {
                if (e.Column is DataGridComboBoxColumn dataGridComboBoxColumn)
                    bindingBase = dataGridComboBoxColumn.SelectedItemBinding;
            }

            if (bindingBase is Binding binding)
            {
                binding.NotifyOnTargetUpdated = true;

                e.Column.CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters = { new EventSetter(Binding.TargetUpdatedEvent, new EventHandler<DataTransferEventArgs>(OnDataGridCellBindingTargetUpdated)) }
                };
            }
        }

        private static void OnDataGridCellBindingTargetUpdated(object sender, DataTransferEventArgs e)
        {
            var dataGridCell = (DataGridCell)sender;

            // Get context: column and item.
            var column = dataGridCell.Column;
            var item = dataGridCell.DataContext;

            if (((System.Data.DataRowView)item).Row.ItemArray[column.DisplayIndex] is DBNull)
                dataGridCell.Background = Brushes.BlanchedAlmond;
        }
    }
}
