using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Swordfish.NET.Collections
{

    /// <summary>
    /// This class allows reflecting changes between 2 ObservableCollection objects, with an
    /// optional type conversion stage.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class ObservableCollectionBridger : ObservableCollectionBridger<object, object>
    {
        private ObservableCollectionBridger()
        {

        }
    }

    public class ObservableCollectionBridger<T1> : ObservableCollectionBridger<T1, T1>
    {
        private ObservableCollectionBridger()
        {

        }
    }

    public class ObservableCollectionBridger<T1, T2>
    {
        private enum UpdateSourceType
        {
            NotUpdating,
            FromCollection1,
            FromCollection2
        }

        private UpdateSourceType _updateSource = UpdateSourceType.NotUpdating;
        private Func<T1, T2> _converter1to2;
        private Func<T2, T1> _converter2to1;
        private ICollection<T1> _collection1;
        private ICollection<T2> _collection2;

        public static IDisposable Bridge<BT1>(ObservableCollection<BT1> collection1, ObservableCollection<BT1> collection2)
        {
            return Bridge<BT1, BT1, ObservableCollection<BT1>, ObservableCollection<BT1>>(collection1, collection2, x => x, x => x);
        }


        public static IDisposable Bridge<BT1, BT2>(ObservableCollection<BT1> collection1, ObservableCollection<BT2> collection2, Func<BT1, BT2> converter1to2, Func<BT2, BT1> converter2to1)
        {
            return Bridge<BT1, BT2, ObservableCollection<BT1>, ObservableCollection<BT2>>(collection1, collection2, converter1to2, converter2to1);
        }

        public static IDisposable Bridge<BT1, BT2, CT1, CT2>(
           CT1 collection1,
           CT2 collection2,
           Func<BT1, BT2> converter1to2,
           Func<BT2, BT1> converter2to1)
          where CT1 : ICollection<BT1>, INotifyCollectionChanged
          where CT2 : ICollection<BT2>, INotifyCollectionChanged
        {

            var bridger = new ObservableCollectionBridger<BT1, BT2>();

            bridger._collection1 = collection1;
            bridger._collection2 = collection2;
            bridger._converter1to2 = converter1to2;
            bridger._converter2to1 = converter2to1;

            collection1.CollectionChanged += bridger.collection1_CollectionChanged;
            collection2.CollectionChanged += bridger.collection2_CollectionChanged;

            // Need to be careful with the scope of unhook otherwise it won't go out of 
            // scope and won't automatically unhook. So don't store it in this object

            Action unhook = () =>
            {
                collection1.CollectionChanged -= bridger.collection1_CollectionChanged;
                collection2.CollectionChanged -= bridger.collection2_CollectionChanged;
            };

            return new AnonDisposable(unhook);
        }

        internal ObservableCollectionBridger()
        {
        }

        void collection2_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updateSource == UpdateSourceType.NotUpdating)
            {
                try
                {
                    _updateSource = UpdateSourceType.FromCollection2;

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (T2 item in e.NewItems)
                            {
                                _collection1.Add(_converter2to1(item));
                            }
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (T2 item in e.OldItems)
                            {
                                _collection1.Remove(_converter2to1(item));
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            foreach (T2 item in e.OldItems)
                            {
                                _collection1.Remove(_converter2to1(item));
                            }
                            foreach (T2 item in e.NewItems)
                            {
                                _collection1.Add(_converter2to1(item));
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _collection1.Clear();
                            foreach (T2 item in _collection2)
                            {
                                _collection1.Add(_converter2to1(item));
                            }
                            break;
                    }
                }
                finally
                {
                    _updateSource = UpdateSourceType.NotUpdating;
                }
            }
        }

        void collection1_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updateSource == UpdateSourceType.NotUpdating)
            {
                try
                {
                    _updateSource = UpdateSourceType.FromCollection1;

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (T1 item in e.NewItems)
                            {
                                _collection2.Add(_converter1to2(item));
                            }
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (T1 item in e.OldItems)
                            {
                                _collection2.Remove(_converter1to2(item));
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            foreach (T1 item in e.OldItems)
                            {
                                _collection2.Remove(_converter1to2(item));
                            }
                            foreach (T1 item in e.NewItems)
                            {
                                _collection2.Add(_converter1to2(item));
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _collection1.Clear();
                            foreach (T1 item in _collection1)
                            {
                                _collection2.Add(_converter1to2(item));
                            }
                            break;
                    }
                }
                finally
                {
                    _updateSource = UpdateSourceType.NotUpdating;
                }
            }
        }
    }
}
