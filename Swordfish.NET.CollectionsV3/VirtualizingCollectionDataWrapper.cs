// Copied from http://www.zagstudio.com/blog/378 license MS-PL (Microsoft Public License)

using System.ComponentModel;

namespace Swordfish.NET.Collections
{
    public class VirtualizingCollectionDataWrapper<T> : INotifyPropertyChanged where T : class
    {
        private int index;
        private T data;

        public event PropertyChangedEventHandler PropertyChanged;

        public VirtualizingCollectionDataWrapper(int index)
        {
            this.index = index;
        }

        public int Index
        {
            get { return this.index; }
        }

        public int ItemNumber
        {
            get { return this.index + 1; }
        }

        public bool IsLoading
        {
            get { return this.Data == null; }
        }

        public T Data
        {
            get { return this.data; }
            internal set
            {
                this.data = value;
                this.OnPropertyChanged("Data");
                this.OnPropertyChanged("IsLoading");
            }
        }

        public bool IsInUse
        {
            get { return this.PropertyChanged != null; }
        }

        private void OnPropertyChanged(string propertyName)
        {
            System.Diagnostics.Debug.Assert(this.GetType().GetProperty(propertyName) != null);
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
