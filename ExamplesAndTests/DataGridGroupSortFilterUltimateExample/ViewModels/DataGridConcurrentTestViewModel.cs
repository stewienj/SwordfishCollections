using DataGridGroupSortFilterUltimateExample.Auxilary;
using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace DataGridGroupSortFilterUltimateExample.ViewModels
{
    /// <summary>
    /// View model used as a source of items for both both the simple and the complex example
    /// </summary>
    public class DataGridConcurrentTestViewModel : NotifyPropertyChanged
    {
        // ********************************************************************
        // Enumerated Types
        // ********************************************************************
        #region Enumerated Types

        /// <summary>
        /// Enumerated type used to determing the source of a group or sort change, which
        /// can either be a change in the Group/Sort options, or a change from the DataGrid
        /// when its column headers are clicked.
        /// </summary>
        private enum UpdateMode
        {
            NotUpdating,
            UpdatingFromGroupSortOptions,
            UpdatingFromDataGrid
        }

        #endregion Enumerated Types

        // ********************************************************************
        // Private Fields
        // ********************************************************************
        #region Private Fields

        /// <summary>
        /// Tracks the current update mode, whether from the group/sort options, or from
        /// clicking on the DataGrid headers.
        /// </summary>
        private UpdateMode _updateMode = UpdateMode.NotUpdating;

        /// <summary>
        /// Timer used for adding items to the TestCollection
        /// </summary>
        private System.Threading.Timer _addItemTimer;

        #endregion Private Fields

        // ********************************************************************
        // Constructors
        // ********************************************************************
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DataGridConcurrentTestViewModel()
        {
            // Timer for adding a new item to the list every second on a background thread
            _addItemTimer = new System.Threading.Timer(AddItemFromTimer, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            // Seed the collection with 15 items
            var randomItems = Enumerable.Range(0, 15).Select(i => ProjectDetails.GetNewProject());
            TestCollection.AddRange(randomItems);

            // When the TestCollection changes notify the view that the properties
            // it is bound to have been updated
            TestCollection.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConcurrentObservableCollection<ProjectDetails>.CollectionView):
                        RaisePropertyChanged(nameof(ProjectList));
                        break;
                    case nameof(ConcurrentObservableCollection<ProjectDetails>.EditableCollectionView):
                        RaisePropertyChanged(nameof(EditableProjectList));
                        break;
                }
            };

            // Hook into the sort/group descriptions to handle when they change
            HookDescriptions();

            // Hook into the sort/group options (editable list of descriptions)
            HookOptions();
        }

        #endregion Constructors

        // ********************************************************************
        // Private Methods
        // ********************************************************************
        #region Private Methods

        /// <summary>
        /// This method is called by the timer delegate. Adds a random item.
        /// </summary>
        private void AddItemFromTimer(Object stateInfo)
        {
            TestCollection.Add(ProjectDetails.GetNewProject());
        }

        /// <summary>
        /// Hooks into the sort/group descriptions to handle when they change
        /// </summary>
        private void HookDescriptions()
        {
            GroupDescriptions.CollectionChanged += GroupDescriptions_CollectionChanged;
            SortDescriptions.CollectionChanged += SortDescriptions_CollectionChanged;
        }

        private void GroupDescriptions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItem = e.OldItems?.OfType<PropertyGroupDescription>().FirstOrDefault();
            var newItem = e.NewItems?.OfType<PropertyGroupDescription>().FirstOrDefault();

            if (_updateMode == UpdateMode.NotUpdating)
            {
                _updateMode = UpdateMode.UpdatingFromDataGrid;

                switch(e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (newItem != null)
                        {
                            // Turn off GroupDescriptionOption
                            var option = GroupDescriptionOptions.Where(o => o.PropertyName == newItem.PropertyName).FirstOrDefault();
                            if (option != null)
                            {
                                option.IsActive = true;
                                GroupDescriptionOptions.Remove(option);
                                GroupDescriptionOptions.Insert(0, option);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (oldItem != null)
                        {
                            // Turn off GroupDescriptionOption
                            var option = GroupDescriptionOptions.Where(o => o.PropertyName == oldItem.PropertyName).FirstOrDefault();
                            if (option!=null)
                            {
                                option.IsActive = false;
                            }
                        }
                        break;
                }

                _updateMode = UpdateMode.NotUpdating;
            }
        }

        private void SortDescriptions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            var oldItem = e.OldItems?.OfType<SortDescription>().FirstOrDefault();
            var newItem = e.NewItems?.OfType<SortDescription>().FirstOrDefault();

            if (_updateMode == UpdateMode.NotUpdating)
            {
                _updateMode = UpdateMode.UpdatingFromDataGrid;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (newItem != null)
                        {
                            // Turn off SortDescriptionOption
                            var option = SortDescriptionOptions.Where(o => o.PropertyName == newItem.Value.PropertyName).FirstOrDefault();
                            if (option != null)
                            {
                                option.IsActive = true;
                                option.Ascending = newItem.Value.Direction == ListSortDirection.Ascending;
                                SortDescriptionOptions.Remove(option);
                                SortDescriptionOptions.Insert(0, option);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (oldItem != null)
                        {
                            // Turn off GroupDescriptionOption
                            var option = SortDescriptionOptions.Where(o => o.PropertyName == oldItem.Value.PropertyName).FirstOrDefault();
                            if (option != null)
                            {
                                option.IsActive = false;
                                option.Ascending = oldItem.Value.Direction == ListSortDirection.Ascending;

                            }
                        }
                        break;
                }

                _updateMode = UpdateMode.NotUpdating;
            }
        }

        /// <summary>
        /// Hooks into the sort/group options changes (editable list of sort/group descriptions)
        /// </summary>
        private void HookOptions()
        {
            // Hook GroupDesciptionOptions
            GroupDescriptionOptions.CollectionChanged += (s, e) => GroupDesciptionOptionsChanged();
            foreach(var item in GroupDescriptionOptions)
            {
                item.PropertyChanged += (s, e) => GroupDesciptionOptionsChanged();
            }

            // Hook SortDescriptionOptions
            SortDescriptionOptions.CollectionChanged += (s, e) => SortDesciptionOptionsChanged();
            foreach (var item in SortDescriptionOptions)
            {
                item.PropertyChanged += (s, e) => SortDesciptionOptionsChanged();
            }

            GroupDesciptionOptionsChanged();
            SortDesciptionOptionsChanged();
        }

        /// <summary>
        /// Mirror the changes from the editable group lists to the datagrid groupers
        /// </summary>
        private void GroupDesciptionOptionsChanged()
        {
            if (_updateMode == UpdateMode.NotUpdating)
            {
                _updateMode = UpdateMode.UpdatingFromGroupSortOptions;
                GroupDescriptions.Clear();
                foreach (var item in GroupDescriptionOptions)
                {
                    if (item.IsActive)
                    {
                        GroupDescriptions.Add(item.GroupDescription);
                    }
                }
                _updateMode = UpdateMode.NotUpdating;
            }
        }

        /// <summary>
        /// Mirror the changes from the editable sort lists to the datagrid sorters
        /// </summary>
        private void SortDesciptionOptionsChanged()
        {
            if (_updateMode == UpdateMode.NotUpdating)
            {
                _updateMode = UpdateMode.UpdatingFromGroupSortOptions;
                SortDescriptions.Clear();
                foreach (var item in SortDescriptionOptions)
                {
                    if (item.IsActive)
                    {
                        SortDescriptions.Add(item.SortDescription);
                    }
                }
                _updateMode = UpdateMode.NotUpdating;
            }
        }


        #endregion

        // ********************************************************************
        // Commands
        // ********************************************************************
        #region Commands

        /// <summary>
        /// Tell the collection that we are beginning an edit so the edit mode isn't exited when an update comes through
        /// </summary>
        private RelayCommandFactory _beginningEditCommand = new RelayCommandFactory();
        public ICommand BeginningEditCommand => _beginningEditCommand.GetCommand(() => TestCollection.BeginEditingItem());

        private RelayCommandFactory _cellChangedCommand = new RelayCommandFactory();
        public ICommand CellChangedCommand => _cellChangedCommand.GetCommand(() =>
        {
            TestCollection.EndedEditingItem();
        });


        /// <summary>
        /// Command to add single item to the collection
        /// </summary>
        private RelayCommandFactory _addSingleItemCommand = new RelayCommandFactory();
        public ICommand AddSingleItemCommand => _addSingleItemCommand.GetCommand(() =>
        {
            TestCollection.Add(ProjectDetails.GetNewProject());
        });

        #endregion Commands

        // ********************************************************************
        // Properties
        // ********************************************************************
        #region Properties

        /// <summary>
        /// The main source source collection. You can bing to TestCollection.CollectionView or bind to TestCollectionView,
        /// they are both the same thing, but TestCollectionView shows how to shorten the path to the ItemsSource source.
        /// For an editable DataGrid you can bind to TestCollection.EditableCollectionView or bind to the EditableTestCollectionView
        /// property further down. Again they are both the same thing, just the latter is a shorter path.
        /// </summary>
        public ConcurrentObservableCollection<ProjectDetails> TestCollection { get; } = new ConcurrentObservableCollection<ProjectDetails>();

        /// <summary>
        /// This is the read-only collection view
        /// </summary>
        public IList<ProjectDetails> ProjectList => TestCollection.CollectionView;

        /// <summary>
        /// This is an editable collection that can be edited in a DataGrid. 
        /// </summary>
        public IList<ProjectDetails> EditableProjectList => TestCollection.EditableCollectionView;

        public ObservableCollection<SortDescription> SortDescriptions { get; } = new ObservableCollection<SortDescription>();

        public ObservableCollection<GroupDescription> GroupDescriptions { get; } = new ObservableCollection<GroupDescription>();

        public ObservableCollection<SortDescriptionOption> SortDescriptionOptions { get; } =
            new ObservableCollection<SortDescriptionOption>
            (
                new[]
                {
                    new SortDescriptionOption(true, "ProjectName"),
                    new SortDescriptionOption(false, "TaskName"),
                    new SortDescriptionOption(false, "DueDate"),
                    new SortDescriptionOption(true, "Status"),
                }
            );


        public ObservableCollection<GroupDescriptionOption> GroupDescriptionOptions { get; } =
            new ObservableCollection<GroupDescriptionOption>
            (
                new[]
                {
                    new GroupDescriptionOption(true, "ProjectName"),
                    new GroupDescriptionOption(false, "TaskName"),
                    new GroupDescriptionOption(false, "DueDate"),
                    new GroupDescriptionOption(true, "Status"),
                }
            );


        private Predicate<object> _filter = null;
        public Predicate<object> Filter
        {
            get => _filter;
            set => SetProperty(ref _filter, value);
        }

        private bool _doCompletedFiltering = false;
        public bool DoCompletedFiltering
        {
            get => _doCompletedFiltering;
            set
            {
                if (SetProperty(ref _doCompletedFiltering, value))
                {
                    if (_doCompletedFiltering)
                    {
                        Filter = p => p is ProjectDetails projectDetails ?
                                projectDetails.Status == CompleteStatus.Active :
                                true;
                    }
                    else
                    {
                        Filter = null;
                    }
                }
            }
        }

        private bool _continuouslyAddItems = false;
        public bool ContinuouslyAddItems
        {
            get => _continuouslyAddItems;
            set
            {
                if (SetProperty(ref _continuouslyAddItems, value))
                {
                    if (_continuouslyAddItems)
                    {
                        _addItemTimer.Change(0, 1000);
                    }
                    else
                    {
                        _addItemTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    }
                }
            }
        }

        private ProjectDetails _selectedProjectDetails = null;
        public ProjectDetails SelectedProjectDetails
        {
            get => _selectedProjectDetails;
            set => SetProperty(ref _selectedProjectDetails, value);
        }

        public ObservableCollection<ProjectDetails> MultiSelectedProjectDetails { get; } = new ObservableCollection<ProjectDetails>();

        #endregion Properties
    }
}
