using Microsoft.Xaml.Behaviors;
using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataGridGroupSortFilterUltimateExample.Auxilary
{
    public class GroupSortAndFilterBehavior : Behavior<ItemsControl>
    {
        private bool _liveGroupSortFiltering = true;

        /// <summary>
        /// Attaches to an ItemsControl
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is ItemsControl itemsControl)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
                descriptor.AddValueChanged(itemsControl, ItemsControl_ItemsSourceChanged);
                ItemsControl_ItemsSourceChanged(itemsControl, EventArgs.Empty);
            }

            if (AssociatedObject is DataGrid dataGrid)
            {
                dataGrid.Sorting += DataGrid_UserClickedOnHeaderSorting;
            }

            // Add hooks to when GroupDescriptions and SortDescriptions change

            HookNewGroupDescriptionsAndRefresh(GroupDescriptions);
            HookNewSortDescriptionsAndRefresh(SortDescriptions);
        }

        private void DataGrid_UserClickedOnHeaderSorting(object sender, DataGridSortingEventArgs e)
        {
            var sortMemberPath = e.Column.SortMemberPath;
            var direction = e.Column.SortDirection ?? ListSortDirection.Ascending;

            var toRemove = SortDescriptions.Where(sd => sd.PropertyName == sortMemberPath).FirstOrDefault();

            if (toRemove == SortDescriptions.FirstOrDefault() && toRemove != null)
            {
                direction =
                  toRemove.Direction == ListSortDirection.Ascending ?
                  ListSortDirection.Descending :
                  ListSortDirection.Ascending;
            }

            if (toRemove != null)
            {
                SortDescriptions.Remove(toRemove);
            }

            var sortDescription = new SortDescription(sortMemberPath, direction);
            SortDescriptions.Insert(0, sortDescription);

            e.Handled = true;
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is ItemsControl itemsControl)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
                descriptor.RemoveValueChanged(itemsControl, ItemsControl_ItemsSourceChanged);
            }

            // Remove hooks from when GroupDescriptions and SortDescriptions change
            UnhookOldGroupDescriptions(GroupDescriptions);
            UnhookOldSortDescriptions(SortDescriptions);

            base.OnDetaching();
        }


        protected void HookNewGroupDescriptionsAndRefresh(IEnumerable<GroupDescription> groupDesciptions)
        {
            if (groupDesciptions is INotifyCollectionChanged notify)
            {
                notify.CollectionChanged += GroupDescriptions_CollectionChanged;
            }

            if (CollectionView != null && _liveGroupSortFiltering)
            {
                CollectionView.GroupDescriptions.Clear();
                if (groupDesciptions != null)
                {
                    foreach (var groupDescription in groupDesciptions)
                    {
                        CollectionView.GroupDescriptions.Add(groupDescription);
                    }
                }
            }
        }

        protected void HookNewSortDescriptionsAndRefresh(IEnumerable<SortDescription> sortDesciptions)
        {
            if (sortDesciptions is INotifyCollectionChanged notify)
            {
                notify.CollectionChanged += SortDescriptions_CollectionChanged;
            }

            if (CollectionView != null && _liveGroupSortFiltering)
            {
                CollectionView.SortDescriptions.Clear();
                if (sortDesciptions != null)
                {
                    foreach (var sortDescription in sortDesciptions)
                    {
                        CollectionView.SortDescriptions.Add(sortDescription);
                    }
                }
            }
        }

        protected void UnhookOldGroupDescriptions(IEnumerable<GroupDescription> groupDesciptions)
        {
            if (groupDesciptions is INotifyCollectionChanged notify)
            {
                notify.CollectionChanged -= GroupDescriptions_CollectionChanged;
            }
        }

        protected void UnhookOldSortDescriptions(IEnumerable<SortDescription> sortDesciptions)
        {
            if (sortDesciptions is INotifyCollectionChanged notify)
            {
                notify.CollectionChanged -= SortDescriptions_CollectionChanged;
            }
        }

        private void ItemsControl_ItemsSourceChanged(object sender, EventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.ItemsSource != null)
            {
                CollectionView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            }
            else
            {
                CollectionView = null;
            }
        }

        private void GroupDescriptions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HookNewGroupDescriptionsAndRefresh(GroupDescriptions.ToList());
        }

        private void SortDescriptions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HookNewSortDescriptionsAndRefresh(SortDescriptions.ToList());
            UpdateLiveGroupSortFiltering();
        }

        private void CollectionView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_collectionView is ListCollectionView listCollectionView)
            {
                switch (e.PropertyName)
                {
                    case nameof(listCollectionView.IsAddingNew):
                    case nameof(listCollectionView.IsEditingItem):
                        _liveGroupSortFiltering = !(listCollectionView.IsEditingItem || listCollectionView.IsAddingNew);
                        UpdateLiveGroupSortFiltering();
                        break;
                }
            }
        }


        private ICollectionView _collectionView = null;
        public ICollectionView CollectionView
        {
            get => _collectionView;
            set
            {
                // Check if the collection view is changing
                if (_collectionView != value)
                {
                    if (_collectionView is INotifyPropertyChanged oldPropertyChanged)
                    {
                        oldPropertyChanged.PropertyChanged -= CollectionView_PropertyChanged;
                    }

                    _collectionView = value;

                    // Hook filtering, grouping, and sorting to the new collection view

                    if (_collectionView != null)
                    {
                        if (_collectionView is INotifyPropertyChanged newPropertyChanged)
                        {
                            newPropertyChanged.PropertyChanged += CollectionView_PropertyChanged;
                        }

                        UpdateLiveGroupSortFiltering();

                        _collectionView.Filter = Filter;

                        // Copy group descriptions
                        HookNewGroupDescriptionsAndRefresh(GroupDescriptions.ToList());

                        // Copy sort descriptions
                        HookNewSortDescriptionsAndRefresh(SortDescriptions.ToList());
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets a collection of System.ComponentModel.GroupDescription objects that
        /// describes how the items in the collection are grouped in the view.
        /// </summary>
        public ObservableCollection<GroupDescription> GroupDescriptions
        {
            get { return (ObservableCollection<GroupDescription>)GetValue(GroupDescriptionsProperty); }
            set { SetValue(GroupDescriptionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GroupDescriptions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupDescriptionsProperty =
            DependencyProperty.Register(
              "GroupDescriptions",
              typeof(ObservableCollection<GroupDescription>),
              typeof(GroupSortAndFilterBehavior),
              new PropertyMetadata(new ObservableCollection<GroupDescription>(), (s, e) =>
              {
                  // Handle when the collection changes
                  // remove event hooks from the old collection
                  // add event hooks to the new collection
                  // clear the old group descriptions and add in the new one

                  if (s is GroupSortAndFilterBehavior behavior)
                  {
                      behavior.UnhookOldGroupDescriptions(e.OldValue as IEnumerable<GroupDescription>);
                      behavior.HookNewGroupDescriptionsAndRefresh(e.NewValue as IEnumerable<GroupDescription>);
                  }
              }));

        /// <summary>
        /// Gets or sets a collection of System.ComponentModel.SortDescription objects that
        /// describes how the items in the collection are sorted in the view.
        /// </summary>
        public ObservableCollection<SortDescription> SortDescriptions
        {
            get { return (ObservableCollection<SortDescription>)GetValue(SortDescriptionsProperty); }
            set { SetValue(GroupDescriptionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GroupDescriptions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortDescriptionsProperty =
            DependencyProperty.Register(
              "SortDescriptions",
              typeof(ObservableCollection<SortDescription>),
              typeof(GroupSortAndFilterBehavior),
              new PropertyMetadata(new ObservableCollection<SortDescription>(), (s, e) =>
              {
                  // Handle when the collection changes
                  // remove event hooks from the old collection
                  // add event hooks to the new collection
                  // clear the old sort descriptions and add in the new ones

                  if (s is GroupSortAndFilterBehavior behavior)
                  {
                      behavior.UnhookOldSortDescriptions(e.OldValue as IEnumerable<SortDescription>);
                      behavior.HookNewSortDescriptionsAndRefresh(e.NewValue as IEnumerable<SortDescription>);
                  }
              }));

        /// <summary>
        /// Gets or sets a callback used to determine if an item is suitable for inclusion
        /// in the view.
        /// </summary>
        public Predicate<object> Filter
        {
            get { return (Predicate<object>)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
              "Filter",
              typeof(Predicate<object>),
              typeof(GroupSortAndFilterBehavior),
              new PropertyMetadata(null, (s, e) =>
              {
                  // Handle when filter changes
                  if (s is GroupSortAndFilterBehavior behavior && behavior.CollectionView != null)
                  {
                      behavior.CollectionView.Filter = e.NewValue as Predicate<object>;
                  }
              }));

        public void UpdateLiveGroupSortFiltering()
        {
            if (CollectionView is ICollectionViewLiveShaping liveShaping)
            {
                liveShaping.IsLiveFiltering = _liveGroupSortFiltering;
                liveShaping.IsLiveGrouping = _liveGroupSortFiltering;
                liveShaping.IsLiveSorting = _liveGroupSortFiltering && SortDescriptions.Count >0;

            }
        }
    }
}
