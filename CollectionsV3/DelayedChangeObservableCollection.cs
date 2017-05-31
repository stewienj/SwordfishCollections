using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Swordfish.NET.Collections
{

  //-------------------------------------------------------------------------
  // This class exposes an ObservableCollection that matches a SourceList
  // but doesn't update until the sourcelist is stable for a given delay.
  public class DelayedChangeObservableCollection<T>
  {
    //-------------------------------------------------------------------------
    public DelayedChangeObservableCollection(TimeSpan minUpdateInterval)
    {
      if (Application.Current == null || Application.Current.Dispatcher == null)
      {
        throw new ArgumentNullException("No Application.Current.Dispatcher available.");
      }

      Dispatcher = Application.Current.Dispatcher;
      MinUpdateInterval = minUpdateInterval;
    }

    //-------------------------------------------------------------------------
    private DispatcherTimer ListUpdateTimer = null;
    private DispatcherTimer EnumUpdateTimer = null;
    private Dispatcher Dispatcher;
    private TimeSpan MinUpdateInterval;

    //-------------------------------------------------------------------------
    public ObservableCollection<T> DisplayList { get; private set; } = new ObservableCollection<T>();

    //-------------------------------------------------------------------------
    private ICollection<T> mSourceList = null;
    public ICollection<T> SourceList
    {
      get { return mSourceList; }
      set
      {
        INotifyCollectionChanged notifyChanged = mSourceList as INotifyCollectionChanged;
        if (notifyChanged != null)
        {
          notifyChanged.CollectionChanged -= NotifyChanged_CollectionChanged;
        }

        mSourceList = value;

        notifyChanged = mSourceList as INotifyCollectionChanged;
        if (notifyChanged != null)
        {
          notifyChanged.CollectionChanged += NotifyChanged_CollectionChanged;
        }
      }
    }

    //-------------------------------------------------------------------------
    private List<T> EnumerationList { get; set; } = new List<T>();

    //-------------------------------------------------------------------------
    private void NotifyChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (ListUpdateTimer == null)
      {
        ListUpdateTimer = new DispatcherTimer(MinUpdateInterval, DispatcherPriority.Render, UpdateList, Dispatcher);
      }

      // reset the update interval
      ListUpdateTimer.Stop();
      ListUpdateTimer.Start();
    }

    //-------------------------------------------------------------------------
    private void UpdateList(object sender, EventArgs e)
    {
      InPlaceUpdate(DisplayList, SourceList);

      ListUpdateTimer.Stop();
    }

    //-------------------------------------------------------------------------
    public void UpdateFromEnumeration(IEnumerable<T> enumeration)
    {
      if (EnumUpdateTimer == null)
      {
        EnumUpdateTimer = new DispatcherTimer(MinUpdateInterval, DispatcherPriority.Render, UpdateEnumeration, Dispatcher);
      }

      InPlaceUpdate(EnumerationList, enumeration);

      // reset the update interval
      EnumUpdateTimer.Stop();
      EnumUpdateTimer.Start();
    }

    //-------------------------------------------------------------------------
    private void UpdateEnumeration(object sender, EventArgs e)
    {
      InPlaceUpdate(DisplayList, EnumerationList);

      EnumUpdateTimer.Stop();
    }

    //-------------------------------------------------------------------------
    //! Updates a list to match the contents of a another list and leaves
    //! items that are already in the list in place (doesn't delete/unselect them)
    public static void InPlaceUpdate(ICollection<T> container, IEnumerable<T> list)
    {
      LinkedList<T> toDelete = new LinkedList<T>(container);

      var listArray = list.ToArray();

      foreach (T o in listArray)
      {
        toDelete.Remove(o);
        if (!container.Contains(o))
          container.Add(o);
      }

      foreach (var i in toDelete)
        container.Remove(i);
    }
  }
}
