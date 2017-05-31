using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Swordfish.NET.Collections
{
  public class ConcurrentObservableFilter<T> : CollectionViewSource
  {
    private Func<T, object, bool> _filter = null;
    public ConcurrentObservableFilter(IConcurrentObservableBase<T> baseCollection)
    {
      Filter += ConcurrentObservableFilter_Filter;
      baseCollection.PropertyChanged += (s, e) =>
      {
        if (e.PropertyName == nameof(IConcurrentObservableBase<T>.CollectionView))
        {
          Source = baseCollection.CollectionView;
        }
      };
    }

    public ConcurrentObservableFilter(IConcurrentObservableBase<T> baseCollection, Func<T, object, bool> filter) : this(baseCollection)
    {
      _filter = filter;
    }

    private void ConcurrentObservableFilter_Filter(object sender, FilterEventArgs e)
    {
      if (_filter!=null && e.Item is T)
      {
        e.Accepted = _filter((T)e.Item, FilterParameter);
      }
      else
      {
        e.Accepted = true;
      }
    }



    public object FilterParameter
    {
      get { return (object)GetValue(FilterParameterProperty); }
      set { SetValue(FilterParameterProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FilterParameter.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FilterParameterProperty =
        DependencyProperty.Register("FilterParameter", typeof(object), typeof(ConcurrentObservableFilter<T>), new PropertyMetadata(null, (d,e)=>
        {
          ConcurrentObservableFilter<T> sender = d as ConcurrentObservableFilter<T>;
          if (sender != null)
          {
            sender.View.Refresh();
          }
        }));


    public Func<T,object,bool> ParameterizedItemFilter
    {
      get
      {
        return _filter;
      }
      set
      {
        _filter = value;
        this.View.Refresh();
      }
    }
    
  }
}
