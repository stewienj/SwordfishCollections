// from https://stackoverflow.com/questions/18122751/wpf-datagrid-customsort-for-each-column/18218963

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DGGroupSortFilterExampleConcurrent.Controls
{
    /// <summary>
    /// Optional behavior for doing custom sorting
    /// </summary>
    /// <example>
    ///   <Window.Resources>
    ///       <local:CustomSorter x:Key="MySorter"/>
    ///   </Window.Resources>
    ///
    ///       <Grid>
    ///           <DataGrid ItemsSource = "{Binding ...}"
    ///               local:CustomSortBehavior.CustomSorter = 
    ///               local:CustomSortBehavior.AllowCustomSort="True"
    ///               <DataGrid.Columns>
    ///                   <DataGridTextColumn Header = "Column 1" Binding="{Binding Column1}"/>
    ///                   <DataGridTextColumn Header = "Column 2" Binding="{Binding Column2}"/>
    ///                   <DataGridTextColumn Header = "Column 3" Binding="{Binding Column3}"/>
    ///               </DataGrid.Columns>
    ///          </DataGrid>
    ///       </Grid>
    ///   </Window>
    /// </example>
    public class CustomSortBehavior
    {
        #region Fields and Constants

        public static readonly DependencyProperty CustomSorterProperty =
            DependencyProperty.RegisterAttached("CustomSorter", typeof(ICustomSorter), typeof(CustomSortBehavior));

        public static readonly DependencyProperty AllowCustomSortProperty =
            DependencyProperty.RegisterAttached("AllowCustomSort",
                typeof(bool),
                typeof(CustomSortBehavior),
                new UIPropertyMetadata(false, OnAllowCustomSortChanged));

        #endregion

        #region public Methods

        public static bool GetAllowCustomSort(DataGrid grid)
        {
            return (bool)grid.GetValue(AllowCustomSortProperty);
        }


        public static ICustomSorter GetCustomSorter(DataGrid grid)
        {
            return (ICustomSorter)grid.GetValue(CustomSorterProperty);
        }

        public static void SetAllowCustomSort(DataGrid grid, bool value)
        {
            grid.SetValue(AllowCustomSortProperty, value);
        }


        public static void SetCustomSorter(DataGrid grid, ICustomSorter value)
        {
            grid.SetValue(CustomSorterProperty, value);
        }

        #endregion

        #region nonpublic Methods

        private static void HandleCustomSorting(object sender, DataGridSortingEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null || !GetAllowCustomSort(dataGrid))
            {
                return;
            }

            var listColView = dataGrid.ItemsSource as ListCollectionView;
            if (listColView == null)
            {
                throw new Exception("The DataGrid's ItemsSource property must be of type, ListCollectionView");
            }

            // Sanity check
            var sorter = GetCustomSorter(dataGrid);
            if (sorter == null)
            {
                return;
            }

            // The guts.
            e.Handled = true;

            var direction = (e.Column.SortDirection != ListSortDirection.Ascending)
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            e.Column.SortDirection = sorter.SortDirection = direction;
            sorter.SortMemberPath = e.Column.SortMemberPath;

            listColView.CustomSort = sorter;
        }

        private static void OnAllowCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var existing = d as DataGrid;
            if (existing == null)
            {
                return;
            }

            var oldAllow = (bool)e.OldValue;
            var newAllow = (bool)e.NewValue;

            if (!oldAllow && newAllow)
            {
                existing.Sorting += HandleCustomSorting;
            }
            else
            {
                existing.Sorting -= HandleCustomSorting;
            }
        }

        #endregion
    }

    public interface ICustomSorter : IComparer
    {
        ListSortDirection SortDirection { get; set; }

        string SortMemberPath { get; set; }
    }

}
