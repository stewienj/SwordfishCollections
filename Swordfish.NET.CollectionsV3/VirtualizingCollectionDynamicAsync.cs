// Copied from http://www.zagstudio.com/blog/378 license MS-PL (Microsoft Public License)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Timers;

using Swordfish.NET.General;

namespace Swordfish.NET.Collections
{
  /// <summary>
  /// This collection should be used when items can be added or removed from the underlying data source.
  /// (Since it derives from AsyncVirtualizingCollection<T>, it also doesn't block the UI on long database queries.)
  /// </summary>
  public class VirtualizingCollectionDynamicAsync<T> : VirtualizingCollectionAsync<T> where T : class
  {
    private System.Timers.Timer timer;

    /// <summary>
    /// Constructor. Hooks up the event handler triggered when the timer ticks.
    /// </summary>
    /// <param name="itemsProvider">The items provider.</param>
    /// <param name="pageSize">Size of the page (number of items).</param>
    /// <param name="pageTimeout">The page timeout, in milliseconds.</param>
    /// <param name="loadCountInterval">The interval between count loads from the database, in milliseconds.</param>
    public VirtualizingCollectionDynamicAsync(IVirtualizingCollectionItemsProvider<T> itemsProvider, int pageSize, int pageTimeout, int loadCountInterval)
        : base(itemsProvider, pageSize, pageTimeout)
    {
      this.timer = new Timer(loadCountInterval);
      timer.AutoReset = false;
      timer.Elapsed += TimerCallback;
      itemsProvider.CountChanged += ItemsProvider_CountChanged;
      ItemsProvider_CountChanged(null, null);
    }

    private void ItemsProvider_CountChanged(object sender, EventArgs e)
    {
      // To give a quick response, load up new data straight away
      if (!timer.Enabled)
      {
        timer.Enabled = true;
        Task.Run(() =>
          {
            // delay 20ms to allow for some rows to be counted
            System.Threading.Thread.Sleep(20);
            LoadCount();
          });
      }
    }

    /// <summary>
    /// As soon as we receive the count we start the timer to retrieve it again.
    /// </summary>
    protected override void LoadCountCompleted(object args)
    {
      base.LoadCountCompleted(args);
      CountChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// We retrieve the count of the underlying collection on a timer. 
    /// </summary>
    private void TimerCallback(object sender, EventArgs args)
    {
       LoadCount();
    }

    public event EventHandler CountChanged;
  }
}
