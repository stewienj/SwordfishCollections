﻿// Copied from http://www.zagstudio.com/blog/378 license MS-PL (Microsoft Public License)

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;

namespace Swordfish.NET.Collections
{
    /// <summary>
    /// Derived VirtualizatingCollection, performing loading asychronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public class VirtualizingCollectionAsync<T> : VirtualizingCollection<T>, INotifyCollectionChanged where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualizingCollectionAsync&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageTimeout">The page timeout.</param>
        public VirtualizingCollectionAsync(IVirtualizingCollectionItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
            : base(itemsProvider, pageSize, pageTimeout)
        {
            _synchronizationContext = SynchronizationContext.Current;
        }

        #region SynchronizationContext

        private readonly SynchronizationContext _synchronizationContext;

        /// <summary>
        /// Gets the synchronization context used for UI-related operations. This is obtained as
        /// the current SynchronizationContext when the AsyncVirtualizingCollection is created.
        /// </summary>
        /// <value>The synchronization context.</value>
        protected SynchronizationContext SynchronizationContext
        {
            get { return _synchronizationContext; }
        }

        #endregion

        #region INotifyCollectionChanged

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this,e);
        }

        /// <summary>
        /// Fires the collection reset event.
        /// </summary>
        private void FireCollectionReset()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }

        #endregion


        private bool _isLoading;
        /// <summary>
        /// Gets or sets a value indicating whether the collection is loading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this collection is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                SetProperty(ref _isLoading, value);
            }
        }

        private bool _isInitializing;
        public bool IsInitializing
        {
            get
            {
                return _isInitializing;
            }
            set
            {
                SetProperty(ref _isInitializing, value);
            }
        }

        #region Load overrides

        /// <summary>
        /// Asynchronously loads the count of items.
        /// </summary>
        protected override void LoadCount()
        {
            if (Count == 0)
            {
                IsInitializing = true;
            }
            ThreadPool.QueueUserWorkItem(LoadCountWork);
        }

        /// <summary>
        /// Performed on background thread.
        /// </summary>
        /// <param name="args">None required.</param>
        private void LoadCountWork(object args)
        {
            int count = FetchCount();
            SynchronizationContext.Send(LoadCountCompleted, count);
        }

        /// <summary>
        /// Performed on UI-thread after LoadCountWork.
        /// </summary>
        /// <param name="args">Number of items returned.</param>
        protected virtual void LoadCountCompleted(object args)
        {
            int newCount = (int)args;
            this.TakeNewCount(newCount);
            IsInitializing = false;
        }

        private void TakeNewCount(int newCount)
        {
            if (newCount != this.Count)
            {
                var oldCount = this.Count;
                this.Count = newCount;
                int pageIndex = (oldCount - 1) / PageSize;

                this.TrimIncompletePages(pageIndex);
                FireCollectionReset();
            }
        }

        /// <summary>
        /// Asynchronously loads the page.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void LoadPage(int pageIndex, int pageLength)
        {
            IsLoading = true;
            ThreadPool.QueueUserWorkItem(LoadPageWork, new int[] { pageIndex, pageLength });
        }

        /// <summary>
        /// Performed on background thread.
        /// </summary>
        /// <param name="args">Index of the page to load.</param>
        private void LoadPageWork(object state)
        {
            int[] args = (int[])state;
            int pageIndex = args[0];
            int pageLength = args[1];
            int overallCount = 0;
            IList<T> dataItems = FetchPage(pageIndex, pageLength, out overallCount);
            SynchronizationContext.Send(LoadPageCompleted, new object[] { pageIndex, dataItems, overallCount });
        }

        /// <summary>
        /// Performed on UI-thread after LoadPageWork.
        /// </summary>
        /// <param name="args">object[] { int pageIndex, IList(T) page }</param>
        private void LoadPageCompleted(object state)
        {
            object[] args = (object[])state;
            int pageIndex = (int)args[0];
            IList<T> dataItems = (IList<T>)args[1];
            int newCount = (int)args[2];
            this.TakeNewCount(newCount);

            PopulatePage(pageIndex, dataItems);
            IsLoading = false;
        }

        #endregion
    }
}
