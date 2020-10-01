using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditableDataGridTest.ViewModels
{
	public class ConcurrentObservableCollectionDataGridTestViewModel
	{
		private Random _random = new Random();
		public ConcurrentObservableCollectionDataGridTestViewModel()
		{
			for (int i = 0; i < 10; ++i)
			{
				AddRandom();
			}
		}

		private void AddRandom()
		{
			var testItem = new TestItem
			{
				Label = _random.Next().ToString(),
				Value1 = _random.Next().ToString(),
				Value2 = _random.Next().ToString()
			};
			DataGridTestCollection1.Add(testItem);
			DataGridTestCollection2.Add(testItem);
		}
		public ConcurrentObservableCollection<TestItem> DataGridTestCollection1 { get; } = new ConcurrentObservableCollection<TestItem>();

		public ObservableCollection<TestItem> DataGridTestCollection2 { get; } = new ObservableCollection<TestItem>();
	}

	public class TestItem : ExtendedNotifyPropertyChanged
	{
		public TestItem()
		{
			this.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName != nameof(StringValue))
				{
					RaisePropertyChanged(nameof(StringValue));
				}
			};
		}
		private string _label = "";
		public string Label { get => _label; set => SetProperty(ref _label, value); }

		private string _value1 = "";
		public string Value1 { get => _value1; set => SetProperty(ref _value1, value); }

		private string _value2 = "";
		public string Value2 { get => _value2; set => SetProperty(ref _value2, value); }

		public string StringValue => $"{Label} : {Value1} : {Value2}";
	}
}
