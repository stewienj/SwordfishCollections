using Swordfish.NET.Demo.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Swordfish.NET.Demo.Tests
{
  /// <summary>
  /// Interaction logic for ConcurrentObservableCollectionV2Test.xaml
  /// </summary>
  public partial class ConcurrentObservableCollectionV2Test : UserControl
  {
    public ConcurrentObservableCollectionV2Test()
    {
      InitializeComponent();
    }

    public ConcurrentObservableCollectionTestViewModel CollectionViewModel { get; } = new ConcurrentObservableCollectionTestViewModel();
    public ConcurrentObservableDictionaryTestViewModel DictionaryViewModel { get; } = new ConcurrentObservableDictionaryTestViewModel();
    public ConcurrentObservableSortedDictionaryTestViewModel SortedDictionaryViewModel { get; } = new ConcurrentObservableSortedDictionaryTestViewModel();
    public ConcurrentObservableSortedCollectionTestViewModel SortedCollectionViewModel { get; } = new ConcurrentObservableSortedCollectionTestViewModel();
    public ConcurrentObservableCollectionManualTestViewModel ManualCollectionViewModel { get; } = new ConcurrentObservableCollectionManualTestViewModel();
    public ConcurrentObservableCollectionStressTestViewModel StressTestViewModel { get; } = new ConcurrentObservableCollectionStressTestViewModel();



    private void DictionaryListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ListView listView = sender as ListView;
      GridView gridView = listView?.View as GridView;
      if (gridView != null)
      {
        var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
        foreach (var column in gridView.Columns)
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
