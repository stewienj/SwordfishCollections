using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public DataGridTestViewModel()
        {
            TestCollection.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConcurrentObservableCollection<TestItem>.CollectionView):
                        PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(TestCollectionView)));
                        break;
                    case nameof(ConcurrentObservableCollection<TestItem>.EditableCollectionView):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditableTestCollectionView)));
                        break;
                }
            };

            var randomItems = Enumerable.Range(0, 10).Select(i => GetRandomItem());
            TestCollection.AddRange(randomItems);
        }

        private TestItem GetRandomItem() => new TestItem
        {
            Label = _random.Next().ToString(),
            Value1 = _random.Next().ToString(),
            Value2 = _random.Next().ToString()
        };

        private RelayCommandFactory _addRandomItemCommand = new RelayCommandFactory();
        public ICommand AddRandomItemCommand => _addRandomItemCommand.GetCommand(() => TestCollection.Add(GetRandomItem()));

        /// <summary>
        /// The source collection
        /// </summary>
        public ConcurrentObservableCollection<TestItem> TestCollection { get; }
            = new ConcurrentObservableCollection<TestItem>();

        public IList<TestItem> TestCollectionView => TestCollection.CollectionView;
        public IList<TestItem> EditableTestCollectionView => TestCollection.EditableCollectionView;

        public event PropertyChangedEventHandler PropertyChanged;
    }

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
