using DataGridGroupSortFilterUltimateExample.ViewModels;
using System.Windows.Controls;

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
    }
}
