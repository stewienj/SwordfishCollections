using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Linq;

namespace EditableDataGridTest.ViewModels
{
    /// <summary>
    /// View model used as a source of items for both a ListView and a DataGrid
    /// </summary>
    public class DataGridTestViewModel
    {
        private Random _random = new Random();
        public DataGridTestViewModel()
        {
            var randomItems = Enumerable.Range(0, 10).Select(i => new TestItem
            {
                Label = _random.Next().ToString(),
                Value1 = _random.Next().ToString(),
                Value2 = _random.Next().ToString()
            });
            TestCollection.AddRange(randomItems);
        }
        /// <summary>
        /// The source collection
        /// </summary>
        public ConcurrentObservableCollection<TestItem> TestCollection { get; }
            = new ConcurrentObservableCollection<TestItem>();
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
