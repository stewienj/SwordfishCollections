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
using System.Windows.Input;
using System.Windows.Threading;

namespace Swordfish.NET.Demo.ViewModels
{
  public class ConcurrentObservableCollectionTestViewModel : ConcurrentObservableTestBaseViewModel<string>
  {
    public ConcurrentObservableCollectionTestViewModel()
    {
    }

    public ConcurrentObservableCollection<string> TestCollection { get; } = new ConcurrentObservableCollection<string>();

    public ObservableCollection<string> NormalCollection { get; } = new ObservableCollection<string>();


    private RelayCommandFactory _runTestScript = new RelayCommandFactory();
    public ICommand RunTestScript
    {
      get
      {
        return _runTestScript.GetCommandAsync(async () =>
        {
          Stopwatch sw = new Stopwatch();
          ClearMessages();
          TestCollection.Clear();
          NormalCollection.Clear();

          // Show a message
          Message("Running tests on observable, concurrent, and view collections...");
          Message("");

          // Create the items to addd and insert
          Message($"Creating 1,000,000 items to add ...");
          sw.Restart();
          var itemsToAdd = await Task.Run(() =>
            Enumerable.Range(0, 1000000).
            Select(x => $"Add {x}").ToList());
          sw.Stop();
          Message("",sw.Elapsed);
          Message("");
          Message($"Creating only 100,000 items to insert because ObservableCollection is slow ...");
          sw.Restart();
          var itemsToInsert = await Task.Run(() =>
            Enumerable.Range(0, 100000).
            Select(x => $"Insert {x}").ToList());
          sw.Stop();
          Message("",sw.Elapsed);
      
          var itemsToRemove = itemsToInsert.Take(10000).ToList();

          // Add items to both collections and then compare

          await AddRange(itemsToAdd, NormalCollection, true);
          await AddRange(itemsToAdd, TestCollection, false);
          await CompareCollections(itemsToAdd);

          await InsertItems(itemsToInsert, NormalCollection, true);
          await InsertItems(itemsToInsert, TestCollection, false);

          // Backup the current collection
          var allItems = NormalCollection.ToList();

          await CompareCollections(allItems);


          await Remove(itemsToRemove, NormalCollection, true);
          await Remove(itemsToRemove, TestCollection, false);
          await CompareCollections();

          await RemoveAtIndex(NormalCollection, true);
          await RemoveAtIndex(TestCollection, false);
          await CompareCollections();

          await Clear(NormalCollection, true);
          await AddRange(allItems, NormalCollection, true);

          // Do GUI thread tests
          await Clear(TestCollection, true);
          await AddRange(itemsToAdd, TestCollection, true);
          await InsertItems(itemsToInsert, TestCollection, true);

          await CompareCollections(allItems);

          // Test adding items 1 at a time
          await Clear(TestCollection, false);
          await Add(itemsToAdd, TestCollection, false);
          await InsertItems(itemsToInsert, TestCollection, false);

          await CompareCollections(allItems);

          // Test adding and inserting from multiple threads at once
          await Clear(TestCollection, false);
          await AddItemsParallel(itemsToAdd, TestCollection);
          await InsertItemsParallel(itemsToInsert, TestCollection);
          Message($"WARNING: The order of the items is non-determinate");

          await CompareCollections(allItems);

          // Test assigning
          var itemsToAssign = itemsToAdd.Concat(itemsToInsert).ToList();
          await Assign(itemsToAssign, NormalCollection, true);
          await Assign(itemsToAssign, TestCollection, false);

          await CompareCollections(itemsToAssign);

          Message("");
          Message("-- Finished Testing --");
        });
      }
    }

    protected virtual Task CompareCollections(params ICollection<string>[] collections)
    {
      var allCollections = collections.Concat(new[] { NormalCollection, TestCollection, TestCollection.CollectionView }).ToArray();

      return CompareCollectionsBase(allCollections);
    }
  }
}