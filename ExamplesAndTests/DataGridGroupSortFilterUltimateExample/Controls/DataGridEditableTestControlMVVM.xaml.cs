using DataGridGroupSortFilterUltimateExample.ViewModels;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataGridGroupSortFilterUltimateExample.Controls
{
    /// <summary>
    /// Interaction logic for ConcurrentTestControl.xaml
    /// </summary>
    public partial class DataGridEditableTestControlMVVM : UserControl
    {
        public DataGridEditableTestControlMVVM()
        {
            DataContext = new DataGridConcurrentTestViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// Handles when the Multi-Selected Items DataGrid adds columns to allow instant changing of
        /// value.s Don't do it with the main grid because there's an exception thrown due to a sort
        /// occuring during an edit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridComboBoxColumn comboBoxColumn)
            {
                if (comboBoxColumn.SelectedItemBinding is Binding binding)
                {
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                }
            }
            else if (e.Column is DataGridBoundColumn boundColumn)
            {
                if (boundColumn.Binding is Binding binding)
                {
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                }
            }

        }
    }
}
