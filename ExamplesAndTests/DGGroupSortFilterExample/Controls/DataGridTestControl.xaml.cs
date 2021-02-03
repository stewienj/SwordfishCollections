using DGGroupSortFilterExample.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DGGroupSortFilterExample.Controls
{
    /// <summary>
    /// Interaction logic for DataGridTestControl.xaml
    /// </summary>
    public partial class DataGridTestControl : UserControl
    {
        public DataGridTestControl()
        {
            DataContext = new DataGridTestViewModel();
            InitializeComponent();
        }

        private void UngroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (cvTasks != null)
            {
                cvTasks.GroupDescriptions.Clear();
            }
        }

        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (cvTasks != null && cvTasks.CanGroup == true)
            {
                cvTasks.GroupDescriptions.Clear();
                cvTasks.GroupDescriptions.Add(new PropertyGroupDescription("ProjectName"));
                cvTasks.GroupDescriptions.Add(new PropertyGroupDescription("Complete"));
            }
        }

        private void CompleteFilter_Changed(object sender, RoutedEventArgs e)
        {
            // Refresh the view to apply filters.
            CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource).Refresh();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is ProjectDetails t)
            {
                // If filter is turned on, filter completed items.
                if (cbCompleteFilter?.IsChecked == true && t.Complete == true)
                    e.Accepted = false;
                else
                    e.Accepted = true;
            }
        }
    }
}
