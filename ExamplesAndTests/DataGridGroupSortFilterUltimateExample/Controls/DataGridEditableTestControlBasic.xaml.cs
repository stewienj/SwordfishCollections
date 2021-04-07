using DataGridGroupSortFilterUltimateExample.ViewModels;
using System.Windows.Controls;

namespace DataGridGroupSortFilterUltimateExample.Controls
{
    /// <summary>
    /// Interaction logic for ConcurrentTestControl.xaml
    /// </summary>
    public partial class DataGridEditableTestControlBasic : UserControl
    {
        public DataGridEditableTestControlBasic()
        {
            DataContext = new DataGridConcurrentTestViewModel();
            InitializeComponent();
        }
    }
}
