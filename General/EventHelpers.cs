// Authored by: John Stewien
// Company: Swordfish Computing
// Year: 2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.ComponentModel;

namespace Swordfish.NET.General {
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// from http://geekswithblogs.net/NewThingsILearned/archive/2008/01/16/have-worker-thread-update-observablecollection-that-is-bound-to-a.aspx
  /// </remarks>
  public static class EventHelpers {

    /// <summary>
    /// Fires a Collection Changed event, but invokes the event using the
    /// dispatcher for each DispatcherObject that is subscribed to the event.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="eventHandler"></param>
    public static void DispatchNotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, NotifyCollectionChangedEventHandler eventHandler) {
      if (eventHandler == null)
        return;

      Delegate[] delegates = eventHandler.GetInvocationList();
      // Loop through invocation list
      foreach (NotifyCollectionChangedEventHandler handler in delegates) {
        try {
          DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
          // If the subscriber is a DispatcherObject and different thread
          if (dispatcherObject != null && dispatcherObject.CheckAccess() == false) {
            // Invoke handler in the target dispatcher's thread
            dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, sender, e);
          } else { // Execute handler as is
            handler(sender, e);
          }
        } catch (Exception) {
        }
      }
    }

    /// <summary>
    /// Fires a Property Changed event, but invokes the event using the
    /// dispatcher for each DispatcherObject that is subscribed to the event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="eventHandler"></param>
    public static void DispatchPropertyChanged(object sender, PropertyChangedEventArgs e, PropertyChangedEventHandler eventHandler) {
      if (eventHandler == null)
        return;

      Delegate[] delegates = eventHandler.GetInvocationList();
      // Loop through invocation list
      foreach (PropertyChangedEventHandler handler in delegates) {
        try {
          DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
          // If the subscriber is a DispatcherObject and different thread
          if (dispatcherObject != null && dispatcherObject.CheckAccess() == false) {
            // BeginInvoke handler in the target dispatcher's thread
            // was using Invoke but had a problem where the Winforms thread is blocked by the WPF
            // thread when dispatcherObject.CheckAccess() == false
            dispatcherObject.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, sender, e);
          } else {// Execute handler as is
            handler(sender, e);
          }
        } catch (Exception) {
        }
      }
    }


    /// <summary>
    /// Fires an event, but invokes the event using the
    /// dispatcher for each DispatcherObject that is subscribed to the event.
    /// 
    /// Due to limitations of C# the T template can't be defined as type Delegate
    /// so it's just cast/checked at runtime
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <param name="eventHandler"></param>
    public static void Dispatch<T, U>(object sender, U args, T eventHandler)
    {
      if (eventHandler == null)
        return;

      Delegate tmp = eventHandler as Delegate;
      if (tmp == null)
        throw new Exception("eventHandler must be of Type delegate");

      Delegate[] delegates = tmp.GetInvocationList();
      // Loop through invocation list
      foreach (var handler in delegates)
      {
        try
        {
          DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
          // If the subscriber is a DispatcherObject and different thread
          if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
          {
            // BeginInvoke handler in the target dispatcher's thread
            // was using Invoke but had a problem where the Winforms thread is blocked by the WPF
            // thread when dispatcherObject.CheckAccess() == false
            dispatcherObject.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, sender, args);
          }
          else
          {// Execute handler as is
            int parameterCount = handler.Method.GetParameters().Length;
            if(parameterCount < 2) {
              handler.DynamicInvoke(args);
            } else {
              handler.DynamicInvoke(sender, args);
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

  }
}