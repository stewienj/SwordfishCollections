using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DGGroupSortFilterExample.ViewModels
{
    /// <summary>
    /// View model used as a source of items for both a ListView and a DataGrid
    /// </summary>
    public class DataGridTestViewModel : INotifyPropertyChanged
    {
        private Random _random = new Random();
        private System.Threading.Timer _addItemTimer;

        public DataGridTestViewModel()
        {
            _addItemTimer = new System.Threading.Timer(AddItemFromTimer, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            TestCollection.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConcurrentObservableCollection<ProjectDetails>.CollectionView):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProjectList)));
                        break;
                    case nameof(ConcurrentObservableCollection<ProjectDetails>.EditableCollectionView):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditableProjectList)));
                        break;
                }
            };

            var randomItems = Enumerable.Range(0, 10).Select(i => ProjectDetails.GetNewProject());
            TestCollection.AddRange(randomItems);
        }

        /// <summary>
        /// This method is called by the timer delegate. Adds a random item.
        /// </summary>
        private void AddItemFromTimer(Object stateInfo)
        {
            TestCollection.Add(ProjectDetails.GetNewProject());
        }

        private RelayCommandFactory _add100000ItemsCommand = new RelayCommandFactory();
        public ICommand Add100000ItemsCommand => _add100000ItemsCommand.GetCommand(() =>
            TestCollection.AddRange(Enumerable.Range(0,100_000).Select(i=>ProjectDetails.GetNewProject())));

        private RelayCommandFactory _addNewItemCommand = new RelayCommandFactory();
        public ICommand AddNewItemCommand => _addNewItemCommand.GetCommand(() => TestCollection.Add(ProjectDetails.GetNewProject()));

        private bool _continuouslyAddItems = false;
        public bool ContinuouslyAddItems
        {
            get => _continuouslyAddItems;
            set
            {
                _continuouslyAddItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContinuouslyAddItems)));
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
        /// The main source source collection. You can bing to TestCollection.CollectionView or bind to TestCollectionView,
        /// they are both the same thing, but TestCollectionView shows how to shorten the path to the ItemsSource source.
        /// For an editable DataGrid you can bind to TestCollection.EditableCollectionView or bind to the EditableTestCollectionView
        /// property further down. Again they are both the same thing, just the latter is a shorter path.
        /// </summary>
        public ConcurrentObservableCollection<ProjectDetails> TestCollection { get; }  = new ConcurrentObservableCollection<ProjectDetails>();

        /// <summary>
        /// This is the read-only collection view
        /// </summary>
        public IList<ProjectDetails> ProjectList => TestCollection.CollectionView;

        /// <summary>
        /// This is an editable collection that can be edited in a DataGrid. 
        /// </summary>
        public IList<ProjectDetails> EditableProjectList => TestCollection.EditableCollectionView;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
