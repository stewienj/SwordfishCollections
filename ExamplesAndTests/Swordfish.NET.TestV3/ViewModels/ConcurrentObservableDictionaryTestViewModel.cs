using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using Swordfish.NET.TestV3.Auxiliary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Swordfish.NET.Demo.ViewModels
{
  public class ConcurrentObservableDictionaryTestViewModel : ConcurrentObservableTestBaseViewModel<KeyValuePair<string, string>>
  {
    public ConcurrentObservableDictionaryTestViewModel()
    {
    }

    public ConcurrentObservableDictionary<string, string> TestCollection { get; protected set; } = new ConcurrentObservableDictionary<string, string>();
    public IDictionary<string, string> NormalDictionary { get; protected set; } = new ConcurrentDictionary<string, string>();
    public List<KeyValuePair<string, string>> NormalCollection { get; protected set; } = new List<KeyValuePair<string, string>>();

    private RelayCommandFactory _runTestScript = new RelayCommandFactory();
    public virtual ICommand RunTestScript
    {
      get
      {
        return _runTestScript.GetCommandAsync(async () =>
        {
          Stopwatch sw = new Stopwatch();
          ClearMessages();
          await Clear(false, TestCollection, NormalCollection, NormalDictionary);

          // Show a message
          Message("Running tests on observable, concurrent, and view collections...");
          Message("");

          // Create the items to addd and insert
          Message($"Creating 1,000,000 items to add ...");
          sw.Restart();
          var itemsToAdd = await Task.Run(() =>
            Enumerable.Range(0, 1000000).
            Select(x => KeyValuePair.Create($"Key {x}", $"Value {x}")).ToList());
          sw.Stop();
          Message("", sw.Elapsed);

          Message("");
          Message($"Creating 100,000 items to to update ...");
          sw.Restart();
          var itemsToUpdate = await Task.Run(() =>
            Enumerable.Range(0, 100000).
            Select(x => KeyValuePair.Create($"Key {x}", $"New Value {x}")).ToList());
          sw.Stop();
          Message("", sw.Elapsed);

          // New collection supports inserts by index, test by starting at 0 and adding 3 each time

          Message("");
          Message($"Creating 100,000 items to to insert by index...");
          sw.Restart();
          var itemsToInsert = await Task.Run(() =>
            Enumerable.Range(0, 100000).
            Select(x => KeyValuePair.Create($"Insert Key {x}", $"Insert Value {x}")).ToList());
          sw.Stop();
          Message("", sw.Elapsed);


          var itemsToRemove = itemsToUpdate.Take(1000).ToList();

          // Add items to all collections and then compare
          Message("");
          await Task.Run(() =>
            Task.WaitAll(
              Add(itemsToAdd, NormalCollection, false),
              Add(itemsToAdd, NormalDictionary, false),
              Add(itemsToAdd, TestCollection, false)
            ));
          await CompareCollections();

          Message("");
          await Task.Run(() =>
            Task.WaitAll(
              InsertItems(itemsToInsert, NormalCollection, false),
              InsertItems(itemsToInsert, NormalDictionary, false),
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
              Remove(itemsToRemove, NormalDictionary, false),
              Remove(itemsToRemove, TestCollection, false)
            ));
          await CompareCollections();

          var backup = NormalCollection.ToList();

          Message("");
          await Task.Run(() =>
            Task.WaitAll(
              RemoveAtIndex(NormalCollection, false),
              // Don't do NormalDictionary, takes too long
              //RemoveAtIndex(NormalDictionary, false),
              RemoveAtIndex(TestCollection, false)
            ));
          await CompareCollections();

          Message("");
          Message("Restoring List items...");
          await Clear(false, NormalCollection);
          await AddRange(backup, NormalCollection, false);

          // Do GUI thread tests
          Message("");
          Message("GUI Thread Test");
          Message("");
          await Clear(true, TestCollection);
          await AddRange(itemsToAdd, TestCollection, true);
          await InsertItems(itemsToInsert, TestCollection, true);
          await CompareCollections();

          // Test adding and inserting from multiple threads at once
          Message("");
          await Clear(TestCollection, false);
          await Clear(NormalDictionary, false);
          await AddItemsParallel(itemsToAdd, TestCollection);
          await AddItemsParallel(itemsToAdd, NormalDictionary);
          await InsertItemsParallel(itemsToInsert, TestCollection);
          await InsertItemsParallel(itemsToInsert, NormalDictionary);
          Message($"WARNING: The order of the items is non-determinate");

          await CompareCollections();

          Message("");
          Message("-- Finished Testing --");
        });
      }
    }


    protected async Task AddItemsParallel(List<KeyValuePair<string, string>> itemsToAdd, ConcurrentObservableDictionary<string, string> destCollection)
    {
      Action<List<KeyValuePair<string, string>>> addBatchAction = (batch) => destCollection.AddRange(batch);
      await AddItemsParallel(itemsToAdd, destCollection, addBatchAction);
    }

    protected async Task AddItemsParallel(List<KeyValuePair<string, string>> itemsToAdd, IDictionary<string, string> destCollection)
    {
      Action<List<KeyValuePair<string, string>>> addBatchAction = (batch) => destCollection.AddRange(batch);
      await AddItemsParallel(itemsToAdd, destCollection, addBatchAction);
    }


    protected async Task AddRange(List<KeyValuePair<string, string>> itemsToAdd, ConcurrentObservableDictionary<string, string> destCollection, bool onGuiThread)
    {
      await AddRange(itemsToAdd, destCollection, (items) => destCollection.AddRange(items), onGuiThread);
    }

    protected async Task AddRange(List<KeyValuePair<string, string>> itemsToAdd, IDictionary<string, string> destCollection, bool onGuiThread)
    {
      await AddRange(itemsToAdd, destCollection, (items) => destCollection.AddRange(items), onGuiThread);
    }

    protected async Task InsertItems(List<KeyValuePair<string, string>> itemsToInsert, IDictionary<string, string> destCollection, bool onGuiThread)
    {
      await InsertItems(itemsToInsert, destCollection, (index, item) => destCollection.Add(item.Key, item.Value), onGuiThread);
    }

    protected async Task InsertItems(List<KeyValuePair<string, string>> itemsToInsert, ConcurrentObservableDictionary<string, string> destCollection, bool onGuiThread)
    {
      await InsertItems(itemsToInsert, destCollection, (index, item) => destCollection.Insert(index, item), onGuiThread);
    }

    protected async Task InsertItemsParallel(List<KeyValuePair<string, string>> itemsToInsert, ConcurrentObservableDictionary<string, string> destCollection)
    {
      await InsertItemsParallel(itemsToInsert, destCollection, (index, item) => destCollection.Insert(index, item));
    }

    protected async Task InsertItemsParallel(List<KeyValuePair<string, string>> itemsToInsert, IDictionary<string, string> destCollection)
    {
      await InsertItemsParallel(itemsToInsert, destCollection, (index, item) => destCollection.Add(item.Key, item.Value));
    }

    protected async Task Remove(List<KeyValuePair<string, string>> itemsToRemove, IDictionary<string, string> destCollection, bool onGuiThread)
    {
      // Assume we are passing in a item in this test that already exists
      await Remove(itemsToRemove, destCollection, (item) => destCollection.Remove(item.Key), onGuiThread);
    }

    protected async Task Remove(List<KeyValuePair<string, string>> itemsToRemove, ConcurrentObservableDictionary<string, string> destCollection, bool onGuiThread)
    {
      await Remove(itemsToRemove, destCollection, (item) => destCollection.Remove(item), onGuiThread);
    }

    protected async Task RemoveAtIndex(ConcurrentObservableDictionary<string, string> destCollection, bool onGuiThread)
    {
      Action<int> removeAction = (i) => destCollection.RemoveAt(i);
      await RemoveAtIndex(destCollection, removeAction, onGuiThread);
    }

    protected async Task RemoveAtIndex(IDictionary<string, string> destCollection, bool onGuiThread)
    {
      Action<int> removeAction = (i) =>
      {
        var item = destCollection.Skip(i).First();
        destCollection.Remove(item.Key);
      };
      await RemoveAtIndex(destCollection, removeAction, onGuiThread);
    }


    protected virtual Task CompareCollections(params ICollection<KeyValuePair<string, string>>[] collections)
    {
      var allCollections = collections.Concat(new ICollection<KeyValuePair<string, string>>[] { NormalCollection,/* NormalDictionary,*/ TestCollection, TestCollection.CollectionView });

      return CompareCollectionsBase(allCollections.ToArray());
    }


  }
}