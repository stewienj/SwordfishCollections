using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using Swordfish.NET.TestV3.Auxiliary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Swordfish.NET.Demo.ViewModels
{
    public class ConcurrentObservableSortedDictionaryTestViewModel : ConcurrentObservableDictionaryTestViewModel
    {
        public ConcurrentObservableSortedDictionaryTestViewModel()
        {
            TestCollection = new ConcurrentObservableSortedDictionary<string, string>();
            NormalDictionary = new SortedDictionary<string, string>();
        }

        public SortedList<string, string> SortedList = new SortedList<string, string>();

        private RelayCommandFactory _runTestScript = new RelayCommandFactory();
        public override ICommand RunTestScript
        {
            get
            {
                return _runTestScript.GetCommandAsync(async () =>
                {
                    Stopwatch sw = new Stopwatch();
                    ClearMessages();
                    await Clear(false, TestCollection, NormalDictionary);

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

            // New collection supports inserts by index, test by starting at 0 and adding 3 each time

            Message("");
                    Message($"Creating 100,000 items to to insert by index...");
                    sw.Restart();
                    var itemsToInsert = await Task.Run(() =>
              Enumerable.Range(0, 100000).
              Select(x => KeyValuePair.Create($"Insert Key {x}", $"Insert Value {x}")).ToList());
                    sw.Stop();
                    Message("", sw.Elapsed);


                    var itemsToRemove = itemsToInsert.Take(1000).ToList();

            // Add items to all collections and then compare
            Message("");
                    await Task.Run(() =>
              Task.WaitAll(
                Add(itemsToAdd, SortedList, false),
                Add(itemsToAdd, NormalDictionary, false),
                Add(itemsToAdd, TestCollection, false)
              ));
                    await CompareCollections();

                    Message("");
                    await Task.Run(() =>
              Task.WaitAll(
                InsertItems(itemsToInsert, SortedList, false),
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
                Remove(itemsToRemove, SortedList, false),
                Remove(itemsToRemove, NormalDictionary, false),
                Remove(itemsToRemove, TestCollection, false)
              ));
                    await CompareCollections();

                    Message("");
                    await Task.Run(() =>
              Task.WaitAll(
                // Don't do NormalDictionary, takes too long
                //RemoveAtIndex(NormalDictionary, false),
                RemoveAtIndex(SortedList, false),
                RemoveAtIndex(TestCollection, false)
              ));
                    await CompareCollectionsBase(SortedList, TestCollection);

            // Do GUI thread tests
            Message("");
                    Message("GUI Thread Test");
                    Message("");

                    await Clear(true, TestCollection);
                    await AddRange(itemsToAdd, TestCollection, true);
                    await InsertItems(itemsToInsert, TestCollection, true);

                    await Clear(true, NormalDictionary);
                    await AddRange(itemsToAdd, NormalDictionary, true);
                    await InsertItems(itemsToInsert, NormalDictionary, true);

                    await CompareCollectionsBase(NormalDictionary, TestCollection);

            // Test adding and inserting from multiple threads at once
            Message("");
                    await Clear(TestCollection, false);
                    await AddItemsParallel(itemsToAdd, TestCollection);
                    await InsertItemsParallel(itemsToInsert, TestCollection);

                    await CompareCollectionsBase(NormalDictionary, TestCollection);

                    Message("");
                    Message("-- Finished Testing --");
                });
            }
        }
        protected async Task RemoveAtIndex(SortedList<string, string> destCollection, bool onGuiThread)
        {
            Action<int> removeAction = (i) => destCollection.RemoveAt(i);
            await RemoveAtIndex(destCollection, removeAction, onGuiThread);
        }

        protected override Task CompareCollections(params ICollection<KeyValuePair<string, string>>[] collections)
        {
            var allCollections = collections.Concat(new ICollection<KeyValuePair<string, string>>[] { SortedList, NormalDictionary, TestCollection, TestCollection.CollectionView });

            return CompareCollectionsBase(allCollections.ToArray());
        }

    }
}