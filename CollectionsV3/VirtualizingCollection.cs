// Copied from http://www.zagstudio.com/blog/378 license MS-PL (Microsoft Public License)

// TODO implement a proper MRU cache for pages

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using Swordfish.NET.General;

namespace Swordfish.NET.Collections {
  /// <summary>
  /// Specialized list implementation that provides data virtualization. The collection is divided up into pages,
  /// and pages are dynamically fetched from the IItemsProvider when required. Stale pages are removed after a
  /// configurable period of time.
  /// Intended for use with large collections on a network or disk resource that cannot be instantiated locally
  /// due to memory consumption or fetch latency.
  /// </summary>
  /// <remarks>
  /// The IList implmentation is not fully complete, but should be sufficient for use as read only collection 
  /// data bound to a suitable ItemsControl.
  /// </remarks>
  /// <typeparam name="T"></typeparam>
  public class VirtualizingCollection<T> : NotifyPropertyChanged, IList<VirtualizingCollectionDataWrapper<T>>, IList where T : class {

    /// <summary>
    /// Creates a logger for use in this class
    /// </summary>
    /// <remarks>
    /// NOTE that using System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    /// is equivalent to typeof(LoggingExample) but is more portable
    /// i.e. you can copy the code directly into another class without
    // needing to edit the code.
    /// </remarks>-
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="itemsProvider">The items provider.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageTimeout">The page timeout.</param>
    public VirtualizingCollection(IVirtualizingCollectionItemsProvider<T> itemsProvider, int pageSize, int pageTimeout) {
      _itemsProvider = itemsProvider;
      _pageSize = pageSize;
      _pageTimeout = pageTimeout;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="itemsProvider">The items provider.</param>
    /// <param name="pageSize">Size of the page.</param>
    public VirtualizingCollection(IVirtualizingCollectionItemsProvider<T> itemsProvider, int pageSize) {
      _itemsProvider = itemsProvider;
      _pageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="itemsProvider">The items provider.</param>
    public VirtualizingCollection(IVirtualizingCollectionItemsProvider<T> itemsProvider) {
      _itemsProvider = itemsProvider;
    }

    #endregion

    #region Properties


    private readonly IVirtualizingCollectionItemsProvider<T> _itemsProvider;
    /// <summary>
    /// Gets the items provider.
    /// </summary>
    /// <value>The items provider.</value>
    public IVirtualizingCollectionItemsProvider<T> ItemsProvider {
      get { return _itemsProvider; }
    }


    private readonly int _pageSize = 100;
    /// <summary>
    /// Gets the size of the page.
    /// </summary>
    /// <value>The size of the page.</value>
    public int PageSize {
      get { return _pageSize; }
    }


    private readonly long _pageTimeout = 100000;
    /// <summary>
    /// Gets the page timeout.
    /// </summary>
    /// <value>The page timeout.</value>
    public long PageTimeout {
      get { return _pageTimeout; }
    }

    #endregion Properties

    #region IList<DataWrapper<T>>, IList


    private int _count = -1;
    /// <summary>
    /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// The first time this property is accessed, it will fetch the count from the IItemsProvider.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </returns>
    public int Count {
      get {
        if (_count == -1) {
          _count = 0;
          LoadCount();
        }
        return _count;
      }
      protected set {
        _count = value;
      }
    }


    /// <summary>
    /// Gets the item at the specified index. This property will fetch
    /// the corresponding page from the IItemsProvider if required.
    /// </summary>
    /// <value></value>
    public VirtualizingCollectionDataWrapper<T> this[int index] {
      get {
        try {
          // determine which page and offset within page
          int pageIndex = index / PageSize;
          int pageOffset = index % PageSize;

          // request primary page
          RequestPage(pageIndex);

          // if accessing upper 50% then request next page
          if (pageOffset > PageSize / 2 && pageIndex < Count / PageSize)
            RequestPage(pageIndex + 1);

          // if accessing lower 50% then request prev page
          if (pageOffset < PageSize / 2 && pageIndex > 0)
            RequestPage(pageIndex - 1);

          // return requested item
          var items = _pages[pageIndex].Items;
          if (pageOffset < items.Count)
          {
            return items[pageOffset];
          }
          else
          {
            // Was an error causing an exception, fixed it but keep this check in place
            _log.Error("page size too small");
            return null;
          }
        } finally {
          // remove stale pages
          CleanUpPages();
        }
      }
      set { throw new NotSupportedException(); }
    }

    object IList.this[int index] {
      get { return this[index]; }
      set { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <remarks>
    /// This method should be avoided on large collections due to poor performance.
    /// </remarks>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<VirtualizingCollectionDataWrapper<T>> GetEnumerator() {
      for (int i = 0; i < Count; i++) {
        yield return this[i];
      }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
    /// <exception cref="T:System.NotSupportedException">
    /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
    /// </exception>
    public void Add(VirtualizingCollectionDataWrapper<T> item) {
      throw new NotSupportedException();
    }

    int IList.Add(object value) {
      throw new NotSupportedException();
    }


    bool IList.Contains(object value) {
      return Contains((VirtualizingCollectionDataWrapper<T>)value);
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
    /// <returns>
    /// Always false.
    /// </returns>
    public bool Contains(VirtualizingCollectionDataWrapper<T> item) {
      foreach (VirtualizingCollectionDataPage<T> page in _pages.Values) {
        if (page.Items.Contains(item)) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void Clear() {
      throw new NotSupportedException();
    }


    int IList.IndexOf(object value) {
      return IndexOf((VirtualizingCollectionDataWrapper<T>)value);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
    /// <returns>
    /// TODO
    /// </returns>
    public int IndexOf(VirtualizingCollectionDataWrapper<T> item) {
      foreach (KeyValuePair<int, VirtualizingCollectionDataPage<T>> keyValuePair in _pages) {
        int indexWithinPage = keyValuePair.Value.Items.IndexOf(item);
        if (indexWithinPage != -1) {
          return PageSize * keyValuePair.Key + indexWithinPage;
        }
      }
      return -1;
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
    /// </exception>
    /// <exception cref="T:System.NotSupportedException">
    /// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
    /// </exception>
    public void Insert(int index, VirtualizingCollectionDataWrapper<T> item) {
      throw new NotSupportedException();
    }

    void IList.Insert(int index, object value) {
      Insert(index, (VirtualizingCollectionDataWrapper<T>)value);
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
    /// </exception>
    /// <exception cref="T:System.NotSupportedException">
    /// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
    /// </exception>
    public void RemoveAt(int index) {
      throw new NotSupportedException();
    }

    void IList.Remove(object value) {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
    /// <returns>
    /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </returns>
    /// <exception cref="T:System.NotSupportedException">
    /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
    /// </exception>
    public bool Remove(VirtualizingCollectionDataWrapper<T> item) {
      throw new NotSupportedException();
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// 	<paramref name="array"/> is null.
    /// </exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// 	<paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    /// 	<paramref name="array"/> is multidimensional.
    /// -or-
    /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
    /// -or-
    /// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// -or-
    /// Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
    /// </exception>
    public void CopyTo(VirtualizingCollectionDataWrapper<T>[] array, int arrayIndex) {
      throw new NotSupportedException();
    }

    void ICollection.CopyTo(Array array, int index) {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
    /// </returns>
    public object SyncRoot {
      get { return this; }
    }

    /// <summary>
    /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
    /// </summary>
    /// <value></value>
    /// <returns>Always false.
    /// </returns>
    public bool IsSynchronized {
      get { return false; }
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
    /// </summary>
    /// <value></value>
    /// <returns>Always true.
    /// </returns>
    public bool IsReadOnly {
      get { return true; }
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
    /// </summary>
    /// <value></value>
    /// <returns>Always false.
    /// </returns>
    public bool IsFixedSize {
      get { return false; }
    }

    #endregion


    private Dictionary<int, VirtualizingCollectionDataPage<T>> _pages = new Dictionary<int, VirtualizingCollectionDataPage<T>>();
    private DateTime _lastPageCleanUp = DateTime.Now;

    /// <summary>
    /// Cleans up any stale pages that have not been accessed in the period dictated by PageTimeout.
    /// </summary>
    public void CleanUpPages() {
      var timeSinceLastCleanup = DateTime.Now - _lastPageCleanUp;
      if (timeSinceLastCleanup.TotalMilliseconds < PageTimeout)
      {
        return;
      }
      int[] keys = _pages.Keys.ToArray();
      foreach (int key in keys) {
        // page 0 is a special case, since WPF ItemsControl access the first item frequently
        if (key != 0 && (DateTime.Now - _pages[key].TouchTime).TotalMilliseconds > PageTimeout) {
          bool removePage = true;
          VirtualizingCollectionDataPage<T> page;
          if (_pages.TryGetValue(key, out page)) {
            removePage = !page.IsInUse;
          }

          if (removePage) {
            _pages.Remove(key);
            _log.Info("Removed Page: " + key);
          }
        }
      }
      _lastPageCleanUp = DateTime.Now;
    }

    /// <summary>
    /// Makes a request for the specified page, creating the necessary slots in the dictionary,
    /// and updating the page touch time.
    /// </summary>
    /// <param name="pageIndex">Index of the page.</param>
    protected virtual void RequestPage(int pageIndex) {
      if (!_pages.ContainsKey(pageIndex)) {
        // Create a page of empty data wrappers.
        // Use the max page size as the count can still be in progress a will cause a page to be created that is too short
        int pageLength = this.PageSize;// Math.Min(this.PageSize, this.Count - pageIndex * this.PageSize);
        VirtualizingCollectionDataPage<T> page = new VirtualizingCollectionDataPage<T>(pageIndex * this.PageSize, pageLength);
        _pages.Add(pageIndex, page);
        _log.Info("Added page: " + pageIndex);
        LoadPage(pageIndex, pageLength);
      } else {
        _pages[pageIndex].TouchTime = DateTime.Now;
      }
    }

    /// <summary>
    /// Populates the page within the dictionary.
    /// </summary>
    /// <param name="pageIndex">Index of the page.</param>
    /// <param name="page">The page.</param>
    protected virtual void PopulatePage(int pageIndex, IList<T> dataItems) {
      _log.Info("Page populated: " + pageIndex);
      VirtualizingCollectionDataPage<T> page;
      if (_pages.TryGetValue(pageIndex, out page)) {
        page.Populate(dataItems);
      }
    }

    /// <summary>
    /// Removes all cached pages. This is useful when the count of the 
    /// underlying collection changes.
    /// </summary>
    protected void EmptyCache() {
      _pages = new Dictionary<int, VirtualizingCollectionDataPage<T>>();
    }

    /// <summary>
    /// When the collection is extended we trim the last cache entry as it might be
    /// incomplete.
    /// </summary>
    /// <param name="minIndexToRemove"></param>
    protected void TrimIncompletePages(int minIndexToRemove)
    {
      while (_pages.Count > 0)
      {
        int highestIndex = _pages.Keys.Max();
        if (highestIndex < minIndexToRemove)
        {
          break;
        }
        if (highestIndex == 0)
        {
          EmptyCache();
          break;
        }
        else
        {
          var page = _pages[highestIndex];
          if (page.Items.Count < PageSize)
          {
            // JCSTODO move these to a holding area where they can be used for temporary display, as there is flickering in the UI
            // as it loads an empty page, then gets updated with the loaded page
            _pages.Remove(highestIndex);
          }
          else
          {
            break;
          }
        }
      }
    }

    /// <summary>
    /// Loads the count of items.
    /// </summary>
    protected virtual void LoadCount() {
      this.Count = FetchCount();
    }

    /// <summary>
    /// Loads the page of items.
    /// </summary>
    /// <param name="pageIndex">Index of the page.</param>
    /// <param name="pageLength">Number of items in the page.</param>
    protected virtual void LoadPage(int pageIndex, int pageLength) {
      int count = 0;
      PopulatePage(pageIndex, FetchPage(pageIndex, pageLength, out count));
      this.Count = count;
    }

    /// <summary>
    /// Fetches the requested page from the IItemsProvider.
    /// </summary>
    /// <param name="pageIndex">Index of the page.</param>
    /// <returns></returns>
    protected IList<T> FetchPage(int pageIndex, int pageLength, out int count) {
      return ItemsProvider.FetchRange(pageIndex * PageSize, pageLength, out count);
    }

    /// <summary>
    /// Fetches the count of itmes from the IItemsProvider.
    /// </summary>
    /// <returns></returns>
    protected int FetchCount() {
      return ItemsProvider.FetchCount();
    }
  }
}
