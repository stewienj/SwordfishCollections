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

namespace EditableDataGridTest.ViewModels
{
    /// <summary>
    /// View model used as a source of items for both a ListView and a DataGrid
    /// </summary>
    public class DataGridTestViewModel : INotifyPropertyChanged
    {
        private Random _random = new Random();
        private System.Threading.Timer _addItemTimer;
        private System.Threading.Timer _updateItemTimer;

        public DataGridTestViewModel()
        {
            _addItemTimer = new System.Threading.Timer(AddRandomItemFromTimer, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            _updateItemTimer = new System.Threading.Timer(UpdateRandomItemFromTimer, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            TestCollection.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConcurrentObservableCollection<TestItem>.CollectionView):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestCollectionView)));
                        break;
                    case nameof(ConcurrentObservableCollection<TestItem>.EditableCollectionView):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditableTestCollectionView)));
                        break;
                }
            };

            var randomItems = Enumerable.Range(0, 10).Select(i => GetRandomItem());
            TestCollection.AddRange(randomItems);
        }

        /// <summary>
        /// This method is called by the timer delegate. Adds a random item.
        /// </summary>
        private void AddRandomItemFromTimer(Object stateInfo)
        {
            TestCollection.Add(GetRandomItem());
        }

        /// <summary>
        /// This method is called by the timer delegate. Updates a random item.
        /// </summary>
        private void UpdateRandomItemFromTimer(Object stateInfo)
        {
            var selectedItem = TestCollection[_random.Next(TestCollection.Count)];
            selectedItem.Value1 = _random.Next().ToString();
            selectedItem.Value2 = _random.Next().ToString();
        }

        private TestItem GetRandomItem() => new TestItem
        {
            Label = _random.Next().ToString(),
            Value1 = _random.Next().ToString(),
            Value2 = _random.Next().ToString()
        };

        private RelayCommandFactory _addRandomItemCommand = new RelayCommandFactory();
        public ICommand AddRandomItemCommand => _addRandomItemCommand.GetCommand(() => TestCollection.Add(GetRandomItem()));

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

        private bool _continuouslyUpdateItems = false;
        public bool ContinuouslyUpdateItems
        {
            get => _continuouslyUpdateItems;
            set
            {
                _continuouslyUpdateItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContinuouslyAddItems)));
                if (_continuouslyUpdateItems)
                {
                    _updateItemTimer.Change(0, 100);
                }
                else
                {
                    _updateItemTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
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
        public ConcurrentObservableCollection<TestItem> TestCollection { get; }  = new ConcurrentObservableCollection<TestItem>();

        /// <summary>
        /// This is the read-only collection view
        /// </summary>
        public IList<TestItem> TestCollectionView => TestCollection.CollectionView;

        /// <summary>
        /// This is an editable collection that can be edited in a DataGrid. 
        /// </summary>
        public IList<TestItem> EditableTestCollectionView => TestCollection.EditableCollectionView;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// This is an example view model, it's not used in this application, but rather it
    /// is featured in the README.md
    /// </summary>
    public class ExampleViewModel : INotifyPropertyChanged
    {
        public ExampleViewModel()
        {
            TestCollection.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConcurrentObservableCollection<TestItem>.CollectionView):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestCollectionView)));
                        break;
                    case nameof(ConcurrentObservableCollection<TestItem>.EditableCollectionView):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditableTestCollectionView)));
                        break;
                }
            };
        }

        public ConcurrentObservableCollection<TestItem> TestCollection { get; }
            = new ConcurrentObservableCollection<TestItem>();

        public IList<TestItem> TestCollectionView => TestCollection.CollectionView;
        public IList<TestItem> EditableTestCollectionView => TestCollection.EditableCollectionView;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Test item used to populate the test collection
    /// </summary>
    public class TestItem : ExtendedNotifyPropertyChanged
    {
        private string _label = "";
        public string Label { get => _label; set => SetProperty(ref _label, value); }

        private string _value1 = "";
        public string Value1 { get => _value1; set => SetProperty(ref _value1, value); }

        private string _value2 = "";
        public string Value2 { get => _value2; set => SetProperty(ref _value2, value); }
    }
}
