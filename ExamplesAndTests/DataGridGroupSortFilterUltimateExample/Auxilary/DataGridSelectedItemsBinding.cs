using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DataGridGroupSortFilterUltimateExample.Auxilary
{
    /// <summary>
    /// This allows binding to the SelectedValues property on a DataGrid/ListView
    /// </summary>
    /// <remarks>
    /// Derived from https://stackoverflow.com/a/31178953
    /// </remarks>
    /// <example>
    /// <DataGrid sf:DataGridSelectedItemsBinding.SelectedValues="{Binding SelectedItems}"/>
    /// </example>
    public class DataGridSelectedItemsBinding
    {
        private static SelectedDataGridItemsBinder GetSelectedValueBinder(DependencyObject obj)
        {
            return (SelectedDataGridItemsBinder)obj.GetValue(SelectedValueBinderProperty);
        }

        private static void SetSelectedValueBinder(DependencyObject obj, SelectedDataGridItemsBinder items)
        {
            obj.SetValue(SelectedValueBinderProperty, items);
        }

        private static readonly DependencyProperty SelectedValueBinderProperty = DependencyProperty.RegisterAttached("SelectedValueBinder", typeof(SelectedDataGridItemsBinder), typeof(DataGridSelectedItemsBinding));


        public static readonly DependencyProperty SelectedValuesProperty = DependencyProperty.RegisterAttached("SelectedValues", typeof(IList), typeof(DataGridSelectedItemsBinding),
            new FrameworkPropertyMetadata(null, OnSelectedValuesChanged));


        private static void OnSelectedValuesChanged(DependencyObject o, DependencyPropertyChangedEventArgs value)
        {
            var oldBinder = GetSelectedValueBinder(o);
            if (oldBinder != null)
                oldBinder.UnBind();

            if (value.NewValue != null)
            {
                SetSelectedValueBinder(o, new SelectedDataGridItemsBinder((DataGrid)o, (IList)value.NewValue));
                GetSelectedValueBinder(o).Bind();
            }
        }

        public static void SetSelectedValues(Selector elementName, IEnumerable value)
        {
            elementName.SetValue(SelectedValuesProperty, value);
        }

        public static IEnumerable GetSelectedValues(Selector elementName)
        {
            return (IEnumerable)elementName.GetValue(SelectedValuesProperty);
        }
    }

    public class SelectedDataGridItemsBinder
    {
        // Keep a weak reference in case the collection outlives the ListView, or the vice versa
        private WeakReference<DataGrid> _weakListBox;
        private WeakReference<IList> _weakCollection;


        public SelectedDataGridItemsBinder(DataGrid dataGrid, IList collection)
        {
            _weakListBox = new WeakReference<DataGrid>(dataGrid);
            _weakCollection = new WeakReference<IList>(collection);

            dataGrid.SelectedItems.Clear();
            foreach (var item in collection)
            {
                dataGrid.SelectedItems.Add(item);
            }
        }

        public void Bind()
        {
            CheckListBox(dataGrid =>
            {
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            });

            CheckCollection(collection =>
            {
                if (collection is INotifyCollectionChanged observable)
                {
                    observable.CollectionChanged += Collection_CollectionChanged;
                }
            });
        }

        public void UnBind()
        {
            CheckListBox(dataGrid =>
            {
                dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
            });

            CheckCollection(collection =>
            {
                if (collection is INotifyCollectionChanged observable)
                {
                    observable.CollectionChanged -= Collection_CollectionChanged;
                }
            });
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CheckListBox(dataGrid =>
            {
                foreach (var item in e.NewItems ?? Empty)
                {
                    if (!dataGrid.SelectedItems.Contains(item))
                        dataGrid.SelectedItems.Add(item);
                }
                foreach (var item in e.OldItems ?? Empty)
                {
                    dataGrid.SelectedItems.Remove(item);
                }
                CheckCollection(collection =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        dataGrid.SelectedItems.Clear();
                        foreach (var item in collection)
                        {
                            dataGrid.SelectedItems.Add(item);
                        }
                    }
                });
            });
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckCollection(collection =>
            {
                var types = collection.GetType().GetGenericArguments();

                Func<object, bool> typeIsOk = o =>
                {
                    if (types.Length == 0)
                    {
                        return true;
                    }
                    else if (o.GetType() == types[0] || o.GetType().IsSubclassOf(types[0]))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };

                foreach (var item in e.AddedItems ?? Empty)
                {
                    if (typeIsOk(item) && !collection.Contains(item))
                        collection.Add(item);
                }

                foreach (var item in e.RemovedItems ?? Empty)
                {
                    if (typeIsOk(item))
                        collection.Remove(item);
                }
            });
        }

        private void CheckListBox(Action<DataGrid> action)
        {
            if (_weakListBox == null)
            {
                return;
            }

            DataGrid dataGrid;
            if (_weakListBox.TryGetTarget(out dataGrid))
            {
                action(dataGrid);
            }
            else
            {
                _weakListBox = null;
                UnBind();
            }
        }

        private void CheckCollection(Action<IList> action)
        {
            if (_weakCollection == null)
            {
                return;
            }

            IList collection;
            if (_weakCollection.TryGetTarget(out collection))
            {
                action(collection);
            }
            else
            {
                _weakCollection = null;
                UnBind();
            }
        }

        private IList Empty { get; } = new object[0];

    }
}
