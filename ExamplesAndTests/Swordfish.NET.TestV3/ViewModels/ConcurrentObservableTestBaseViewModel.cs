using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using Swordfish.NET.TestV3.Auxiliary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Swordfish.NET.Demo.ViewModels
{
    public class ConcurrentObservableTestBaseViewModel<T> : ExtendedNotifyPropertyChanged
    {
        public ConcurrentObservableCollection<string> Messages { get; } = new ConcurrentObservableCollection<string>();

        protected void Message(string type, TimeSpan time)
        {
            Message($"Time {type}: {time}");
        }

        protected void Message(string message)
        {
            Messages.Add(message);
        }


        protected void ClearMessages()
        {
            Messages.Clear();
        }

        private RelayCommandFactory _copyLogCommand = new RelayCommandFactory();
        public ICommand CopyLogCommand
        {
            get
            {
                return _copyLogCommand.GetCommand(() =>
                {
                    if (Messages.Any())
                    {
                        Clipboard.SetText(Messages.Aggregate((a, b) => a + Environment.NewLine + b));
                    }
                });
            }
        }

        public async Task TimedAction(string type, Action action, bool onGuiThread)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            if (onGuiThread)
            {
                action();
            }
            else
            {
                await Task.Run(() =>
                {
                    action();
                });
            }
            sw.Stop();
            Message(type, sw.Elapsed);
        }

        // ************************************************************************
        // Clear Functions
        // ************************************************************************
        protected async Task Clear(bool onGuiThread, params ICollection<T>[] destCollections)
        {
            foreach (var colection in destCollections)
            {
                await Clear(colection, onGuiThread);
            }
        }

        protected async Task Clear(ICollection<T> destCollection, bool onGuiThread)
        {
            string collectionType = destCollection.GetType().Name;
            Message($"Clear() {collectionType}: {await ThreadText(onGuiThread)}");
            await TimedAction(
              collectionType,
              () => destCollection.Clear(),
              onGuiThread
            );
        }

        // ************************************************************************
        // Insert Functions
        // ************************************************************************

        protected async Task InsertItems(List<T> itemsToInsert, IList<T> destCollection, bool onGuiThread)
        {
            await InsertItems(itemsToInsert, destCollection, (index, item) => destCollection.Insert(index, item), onGuiThread);
        }

        protected async Task InsertItems(List<T> itemsToInsert, ICollection<T> destCollection, Action<int, T> insertAction, bool onGuiThread)
        {
            string collectionType = destCollection.GetType().Name;
            Message($"Insert() {collectionType}: {itemsToInsert.Count} items 1 at a time {await ThreadText(onGuiThread)}...");
            await TimedAction(
              collectionType,
              () =>
              {
                  double indexMultiplier = 1.0 / itemsToInsert.Count;
                  foreach (var itemAndIndex in itemsToInsert.Select((item, i) => new { Item = item, Index = i }))
                  {
                      int insertPoint = (int)(itemAndIndex.Index * indexMultiplier * destCollection.Count);
                      insertAction(insertPoint, itemAndIndex.Item);
                  }
              },
              onGuiThread
            );
        }

        protected async Task InsertItemsParallel(List<T> itemsToInsert, IList<T> destCollection)
        {
            await InsertItemsParallel(itemsToInsert, destCollection, (index, item) => destCollection.Insert(index, item));
        }

        protected async Task InsertItemsParallel(List<T> itemsToInsert, ICollection<T> destCollection, Action<int, T> insertAction)
        {
            string collectionType = destCollection.GetType().Name;
            Message($"Insert() {collectionType}: {itemsToInsert.Count} items from multiple threads");
            Message($"WARNING: The order of the items is non-determinate");

            await TimedAction(
              collectionType,
              () =>
              {
                  double multiplier = 1.0 / itemsToInsert.Count;
                  Parallel.ForEach(itemsToInsert.Select((item, i) => new { Item = item, Index = i }), item =>
            {
                    insertAction((int)(multiplier * item.Index * destCollection.Count), item.Item);
                });
              },
              false
            );
        }

        // ************************************************************************
        // Add Functions
        // ************************************************************************

        protected async Task AddItemsParallel(List<T> itemsToAdd, IList<T> destCollection)
        {
            Action<List<T>> addBatchAction = (batch) => destCollection.AddRange(batch);
            await AddItemsParallel(itemsToAdd, destCollection, addBatchAction);
        }

        protected async Task AddItemsParallel(List<T> itemsToAdd, ConcurrentObservableCollection<T> destCollection)
        {
            Action<List<T>> addBatchAction = (batch) => destCollection.AddRange(batch);
            await AddItemsParallel(itemsToAdd, destCollection, addBatchAction);
        }

        protected async Task AddItemsParallel(List<T> itemsToAdd, ICollection<T> destCollection, Action<List<T>> addBatchAction)
        {
            string collectionType = destCollection.GetType().Name;
            Message($"AddRange() {collectionType}: {itemsToAdd.Count} items from multiple threads in 16 batched lots");

            await TimedAction(collectionType, () =>
            {
                Parallel.ForEach(itemsToAdd.Batch(itemsToAdd.Count / 16), (itemBatch) =>
          {
                  addBatchAction(itemBatch.ToList());
              });
            }, false);
        }

        protected async Task AddRange(List<T> itemsToAdd, ConcurrentObservableCollection<T> destCollection, bool onGuiThread)
        {
            await AddRange(itemsToAdd, destCollection, (items) => destCollection.AddRange(items), onGuiThread);
        }

        protected async Task AddRange(List<T> itemsToAdd, IList<T> destCollection, bool onGuiThread)
        {
            await AddRange(itemsToAdd, destCollection, (items) => destCollection.AddRange(items), onGuiThread);
        }

        protected async Task AddRange(List<T> itemsToAdd, ICollection<T> destCollection, Action<List<T>> addAction, bool onGuiThread)
        {
            string collectionType = destCollection.GetType().Name;
            Message($"AddRange() {collectionType}: {itemsToAdd.Count} items {await ThreadText(onGuiThread)}...");

            await TimedAction(
              collectionType,
              () => addAction(itemsToAdd),
              onGuiThread
            );
        }
        protected async Task Add(List<T> itemsToAdd, ICollection<T> destCollection, bool onGuiThread)
        {
            await Add(itemsToAdd, destCollection, (item) => destCollection.Add(item), onGuiThread);
        }

        protected async Task Add(List<T> itemsToAdd, ICollection<T> destCollection, Action<T> addAction, bool onGuiThread)
        {
            string collectionType = destCollection.GetType().Name;
            Message($"Add() {collectionType}: {itemsToAdd.Count} items 1 at a time {await ThreadText(onGuiThread)}...");

            await TimedAction(
              collectionType,
              () =>
              {
                  foreach (var item in itemsToAdd)
                  {
                      destCollection.Add(item);
                  }
              },
              onGuiThread
            );
        }

        protected async Task Assign(List<T> itemsToAssign, IList<T> destCollection, bool onGuiThread)
        {
            await Assign(itemsToAssign, destCollection, (index, item) => destCollection[index] = item, onGuiThread);
        }

        protected async Task Assign(List<T> itemsToAssign, ICollection<T> destCollection, Action<int, T> assignAction, bool onGuiThread)
        {
            string collectionType = destCollection.GetType().Name;

            Message($"Assign this[index] {collectionType}: {itemsToAssign.Count} items 1 at a time {await ThreadText(onGuiThread)}...");

            Action assignGroupAction = () =>
            {
                for (int i = 0; i < itemsToAssign.Count; ++i)
                {
                    assignAction(i, itemsToAssign[i]);
                }
            };

            await TimedAction(collectionType, assignGroupAction, onGuiThread);
        }

        protected async Task Remove(List<T> itemsToRemove, IList<T> destCollection, bool onGuiThread)
        {
            await Remove(itemsToRemove, destCollection, (item) => destCollection.Remove(item), onGuiThread);
        }

        protected async Task Remove(List<T> itemsToRemove, ICollection<T> destCollection, Action<T> removeAction, bool onGuiThread)
        {
            string collectionType = destCollection.GetType().Name;

            Message($"Remove() {collectionType}: {itemsToRemove.Count} items 1 at a time {await ThreadText(onGuiThread)}...");

            await TimedAction(
              collectionType,
              () => itemsToRemove.ForEach(item => removeAction(item)),
              onGuiThread
            );
        }

        protected async Task RemoveAtIndex(IList<T> destCollection, bool onGuiThread)
        {
            await RemoveAtIndex(destCollection, (i) => destCollection.RemoveAt(i), onGuiThread);
        }

        protected async Task RemoveAtIndex(IEnumerable<T> destCollection, Action<int> removeAction, bool onGuiThread)
        {
            string collectionType = destCollection.GetType().Name;
            int count = destCollection.Count();
            Message($"RemoveAt() {collectionType}: {count / 30} items 1 at a time starting at end {await ThreadText(onGuiThread)}...");

            Action removeGroupAction = () =>
            {
                int index = count - 30;
                while (index >= 0 && count > 0)
                {
                    removeAction(index);
                    index -= 30;
                    count--;
                }
            };

            await TimedAction(
              collectionType,
              removeGroupAction,
              onGuiThread
            );
        }

        protected async Task<string> ThreadText(bool onGuiThread)
        {
            if (onGuiThread)
            {
                await Task.Delay(200);
            }
            return onGuiThread ? "on GUI thread" : "on background thread";
        }


        protected virtual async Task CompareCollectionsBase(params ICollection<T>[] collections)
        {
            await Task.Run(() =>
            {
                var collectionsAndTypeName = collections.Select(c => new { Collection = c, Name = c.GetType().Name }).ToArray();

          // Get all the combinations of all the collections
          var collectionPairs = collectionsAndTypeName
            .Take(collectionsAndTypeName.Length - 1)
            .Select(
              (item1, index) => collectionsAndTypeName
                .Skip(index + 1)
                .Select(item2 => new { Item1 = item1, Item2 = item2 })
             ).SelectMany(p => p);


                var typeNames = collections.Select(c => c.GetType().Name).ToArray();
                Message("");
                Message($"Comparing {typeNames.Aggregate((a, b) => a + " ," + b)}...");
                try
                {
                    foreach (var pair in collectionPairs)
                    {
                        if (pair.Item1.Collection.Count != pair.Item2.Collection.Count)
                        {
                            Message($"ERROR Different Counts {pair.Item1.Name}:{pair.Item1.Collection.Count} {pair.Item2.Name}:{pair.Item2.Collection.Count}");
                        }
                        else
                        {
                            Message($"Equal Counts {pair.Item1.Name}:{pair.Item1.Collection.Count} {pair.Item2.Name}:{pair.Item2.Collection.Count}");
                            if (pair.Item1.Collection.Except(pair.Item2.Collection).Any())
                            {
                                Message($"ERROR Different Contents {pair.Item1.Name} {pair.Item2.Name}");
                            }
                            else
                            {
                                Message($"Equal Contents {pair.Item1.Name} {pair.Item2.Name}");
                                if (!pair.Item1.Collection.SequenceEqual(pair.Item2.Collection))
                                {
                                    Message($"ERROR Different Order {pair.Item1.Name} {pair.Item2.Name}");
                                }
                                else
                                {
                                    Message($"Equal Order {pair.Item1.Name} {pair.Item2.Name}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Message(ex.Message);
                }
            });
        }
    }
}
