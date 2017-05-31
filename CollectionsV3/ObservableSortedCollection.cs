using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections {
  public class ObservableSortedCollection<TKey> : ObservableCollection<TKey> {

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// Utitlity object that is used for sorting the list
    /// </summary>
    private BinarySorter<TKey> _sorter;

    #endregion Private Fields

    // ************************************************************************
    // Public Methods
    // ************************************************************************
    #region Public Methods

    /// <summary>
    /// Constructor with an optional IComparer<TKey> parameter.
    /// </summary>
    /// <param name="comparer">Comparer used to sort the keys.</param>
    public ObservableSortedCollection(IComparer<TKey> comparer = null) {
      _sorter = new BinarySorter<TKey>(comparer);
    }

    #endregion Public Methods

    protected override void MoveItem(int oldIndex, int newIndex) {
      // This is a sorted list, can't move the position of objects
      return;
    }

    protected override void InsertItem(int index, TKey item) {
      // Sorted list, throw away the index and work out the correct position

      int sortedIndex = _sorter.GetInsertIndex(this.Count, item, delegate(int mid){
        return this[mid];
      });

      base.InsertItem(sortedIndex, item);
    }

    protected override void SetItem(int index, TKey item) {
      // New item might go in a different position, so remove the old item, and insert the new
      RemoveAt(index);
      InsertItem(index, item);
    }
  }
}
