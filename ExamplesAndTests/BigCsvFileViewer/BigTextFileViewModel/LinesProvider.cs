using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;

namespace BigCsvFileViewer.BigTextFileViewModel
{
    public abstract class LinesProvider : IVirtualizingCollectionItemsProvider<BigFileLine>, IDisposable
    {
        protected int _lastFetchCount = 0;

        /// <summary>
        /// Fetches the total number of items available.
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Fetches the total number of items available.
        /// </summary>
        /// <returns></returns>
        public int FetchCount()
        {
            _lastFetchCount = Count;
            // Trace.WriteLine(string.Format("FetchCount = {0}", _lastFetchCount));
            return _lastFetchCount;
        }

        /// <summary>
        /// Fetches a range of items.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns></returns>
        public abstract IList<BigFileLine> FetchRange(int startIndex, int pageCount, out int overallCount);

        public abstract void Dispose();

        /// <summary>
        /// Gets the first line of the file
        /// </summary>
        public string Header
        {
            get;
            protected set;
        }

        private int _maxColumns = 0;
        public int MaxColumns
        {
            get
            {
                return _maxColumns;
            }
            protected set
            {
                if (_maxColumns < value)
                {
                    _maxColumns = value;
                    var handler = MaxColumnsChanged;
                    if (handler != null)
                    {
                        handler(this, new MaxColumnsArgs(_maxColumns));
                    }
                }
            }
        }

        private int _count = 0;
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                CountChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler<MaxColumnsArgs> MaxColumnsChanged;
        public event EventHandler CountChanged;
    }
}
