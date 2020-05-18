using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using Swordfish.NET.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Swordfish.NET.Demo.ViewModels
{
  public class ConcurrentObservableCollectionManualTestViewModel : ConcurrentObservableTestBaseViewModel<string>
  {
    private ListView listView;
    public ConcurrentObservableCollectionManualTestViewModel()
    {
    }
    private int _next = 0;
    private string GetNext()
    {
      return (++_next).ToString();
    }

    public void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      listView = (ListView)sender;
      Message($"ListView has {listView.Items.Count} items, selected is {listView.SelectedItem}");
    }

    public ConcurrentObservableCollection<string> TestCollection { get; } = new ConcurrentObservableCollection<string>();

    public string SelectedItem { get; set; } = null;

    private RelayCommandFactory _removeLastCommand = new RelayCommandFactory();
    public ICommand RemoveLastCommand
    {
      get
      {
        return _removeLastCommand.GetCommandAsync(async () =>
        {
          var itemRemoved = await Task.Run(() => TestCollection.RemoveLast());
          Message($"Removed {itemRemoved}");
        });
      }
    }

    private RelayCommandFactory _addItemsCommand = new RelayCommandFactory();
    public ICommand AddItemsCommand
    {
      get
      {
        return _addItemsCommand.GetCommandAsync(async () =>
        {
          // Create the items to addd and insert
          Message($"ListView has {listView?.Items?.Count} items, selected is {listView?.SelectedItem}");
          Message($"Creating 10 items to add ...");
          var itemsToAdd = await Task.Run(() =>
            Enumerable.Range(0, 10).
            Select(x => $"Item {GetNext()}").ToList());
          await AddRange(itemsToAdd, TestCollection, false);
          Message($"ListView has {listView?.Items?.Count} items, selected is {listView?.SelectedItem}");
        });
      }
    }
  }
}