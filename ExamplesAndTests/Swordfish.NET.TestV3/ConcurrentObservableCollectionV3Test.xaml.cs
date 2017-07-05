using Swordfish.NET.Demo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Swordfish.NET.Demo.Tests
{
  /// <summary>
  /// Interaction logic for ConcurrentObservableCollectionV3Test.xaml
  /// </summary>
  public partial class ConcurrentObservableCollectionV3Test : UserControl
  {
    public ConcurrentObservableCollectionV3Test()
    {
      InitializeComponent();
    }

    public ConcurrentObservableCollectionTestViewModel CollectionViewModel { get; } = new ConcurrentObservableCollectionTestViewModel();
    public ConcurrentObservableDictionaryTestViewModel DictionaryViewModel { get; } = new ConcurrentObservableDictionaryTestViewModel();
    public ConcurrentObservableSortedDictionaryTestViewModel SortedDictionaryViewModel { get; } = new ConcurrentObservableSortedDictionaryTestViewModel();
    public ConcurrentObservableCollectionManualTestViewModel ManualCollectionViewModel { get; } = new ConcurrentObservableCollectionManualTestViewModel();
    public ConcurrentObservableCollectionStressTestViewModel StressTestViewModel { get; } = new ConcurrentObservableCollectionStressTestViewModel();



    private void DictionaryListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ListView listView = sender as ListView;
      GridView gridView = listView?.View as GridView;
      if (gridView != null)
      {
        var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
        foreach(var column in gridView.Columns)
        {
          column.Width = workingWidth / gridView.Columns.Count;
        }
      }
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ManualCollectionViewModel.ListView_SelectionChanged(sender, e);
    }
  }
}
