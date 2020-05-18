using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using Swordfish.NET.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Swordfish.NET.Demo.ViewModels
{
  public class ConcurrentObservableSortedCollectionTestViewModel : ConcurrentObservableTestBaseViewModel<string>
  {
    public ConcurrentObservableSortedCollectionTestViewModel()
    {
    }

    public ConcurrentObservableSortedCollection<string> TestCollection { get; } = new ConcurrentObservableSortedCollection<string>();

    public List<string> NormalCollection { get; } = new List<string>();

    public List<string> NormalCollectionView { get; set; }

    private RelayCommandFactory _runTestScript = new RelayCommandFactory();
    public ICommand RunTestScript =>
        _runTestScript.GetCommandAsync(async () =>
        {
        Stopwatch sw = new Stopwatch();
        ClearMessages();
        await Clear(false, TestCollection, NormalCollection);

        // Show a message
        Message("Running tests on normal, concurrent, and view collections...");
        Message("");

        // Create the items to addd and insert
        Message($"Creating 1,000,000 items to add ...");
        sw.Restart();
        var itemsToAdd = await Task.Run(() =>
          Enumerable.Range(0, 1000000).
          Select(x => $"Value {x}").ToList());
        sw.Stop();
        Message("", sw.Elapsed);

        // New collection supports inserts by index, test by starting at 0 and adding 3 each time

        Message("");
        Message($"Creating 100,000 items to to insert by index...");
        sw.Restart();
        var itemsToInsert = await Task.Run(() =>
          Enumerable.Range(0, 100000).
          Select(x => $"Insert Value {x}").ToList());
        sw.Stop();
        Message("", sw.Elapsed);


        var itemsToRemove = itemsToInsert.Take(1000).ToList();

          // Add items to all collections and then compare
          Message("");
          await Task.Run(() =>
            Task.WaitAll(
              Add(itemsToAdd, NormalCollection, false),
              Add(itemsToAdd, TestCollection, false)
            ));
          await CompareCollections();

          Message("");
          await Task.Run(() =>
            Task.WaitAll(
              InsertItems(itemsToInsert, NormalCollection, false),
              InsertItems(itemsToInsert, TestCollection, false)
            ));
          await CompareCollections();

          Message("");
          // Test adding range of items
          await Clear(false, TestCollection);
          await AddRange(itemsToAdd, TestCollection, false);
          await InsertItems(itemsToInsert, TestCollection, false);
          await CompareCollections();

          Message("");
          await Task.Run(() =>
            Task.WaitAll(
              Remove(itemsToRemove, NormalCollection, false),
              Remove(itemsToRemove, TestCollection, false)
            ));
          await CompareCollections();

          Message("");
          await Task.Run(() =>
            Task.WaitAll(
              // Don't do NormalDictionary, takes too long
              //RemoveAtIndex(NormalDictionary, false),
              RemoveAtIndexAfterSort(NormalCollection, false),
              RemoveAtIndex(TestCollection, false)
            ));
          await CompareCollections();

          // Do GUI thread tests
          Message("");
          Message("GUI Thread Test");
          Message("");

          await Clear(true, TestCollection);
          await AddRange(itemsToAdd, TestCollection, true);
          await InsertItems(itemsToInsert, TestCollection, true);

          await Clear(true, NormalCollection);
          await AddRange(itemsToAdd, NormalCollection, true);
          await InsertItems(itemsToInsert, NormalCollection, true);

          await CompareCollections();

          // Test adding and inserting from multiple threads at once
          Message("");
          await Clear(TestCollection, false);
          await AddItemsParallel(itemsToAdd, TestCollection);
          await InsertItemsParallel(itemsToInsert, TestCollection);

          await CompareCollections();

          Message("");
          Message("-- Finished Testing --");
        });

    private async Task RemoveAtIndexAfterSort(List<string> collectionToSort, bool onGuiThread)
    {
      collectionToSort.Sort();
      Action<int> preRemoveSort = (i) =>
      {
        collectionToSort.RemoveAt(i);
      };

      await RemoveAtIndex(collectionToSort, preRemoveSort, onGuiThread);
    }

    protected Task CompareCollections(params ICollection<string>[] collections)
    {
      NormalCollection.Sort();
      NormalCollectionView = NormalCollection.ToList();
      OnPropertyChanged(nameof(NormalCollectionView));
      var allCollections = collections.Concat(new ICollection<string>[] {NormalCollection, TestCollection, TestCollection.CollectionView });

      return CompareCollectionsBase(allCollections.ToArray());
    }

  }
}
