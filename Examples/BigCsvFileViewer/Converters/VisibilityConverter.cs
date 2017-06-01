using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using Swordfish.NET.General;

namespace Swordfish.NET.WPF.Converters
{
  //-------------------------------------------------------------------------
  /// <summary>
  /// Returns Visibility.Visible for true or null, Visibilitiy.Collapsed for false
  /// </summary>
  [ValueConversion(typeof(bool), typeof(Visibility))]
  public class VisibilityConverter : IValueConverter
  {
    private static Lazy<VisibilityConverter> _instance = new Lazy<VisibilityConverter>(true);

    public static VisibilityConverter Instance
    {
      get
      {
        return _instance.Value;
      }
    }

    //-------------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool? v = value as bool?;
      if (v != null)
        return ((bool)v)?Visibility.Visible:Visibility.Collapsed;

      return Visibility.Visible;
    }

    //-------------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      Visibility? v = value as Visibility?;

      if (v != null)
        return v == Visibility.Visible ? true : false;

      return true;
    }
  }

  [ValueConversion(typeof(bool), typeof(Visibility))]
  public class InverseVisibilityConverter : IValueConverter {
    private static Lazy<InverseVisibilityConverter> _instance = new Lazy<InverseVisibilityConverter>(true);

    public static InverseVisibilityConverter Instance
    {
      get
      {
        return _instance.Value;
      }
    }
    //-------------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
      bool? v = value as bool?;
      if(v != null)
        return ((bool)v) ? Visibility.Collapsed : Visibility.Visible;

      return Visibility.Collapsed;
    }

    //-------------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
      Visibility? v = value as Visibility?;

      if(v != null)
        return v == Visibility.Visible ? false : true;

      return false;
    }
  }

  [ValueConversion(typeof(string), typeof(Visibility))]
  public class StringValidVisibilityConverter : IValueConverter
  {
    private static Lazy<StringValidVisibilityConverter> _instance = new Lazy<StringValidVisibilityConverter>(true);

    public static StringValidVisibilityConverter Instance
    {
      get
      {
        return _instance.Value;
      }
    }

    //-------------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      string v = value as string;
      return string.IsNullOrEmpty(v) ? Visibility.Collapsed : Visibility.Visible;
    }

    //-------------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return null;
    }
  }


  //-------------------------------------------------------------------------
  /// <summary>
  /// Returns Visibility.Visible for != 0 Visibilitiy.Collapsed for anything else
  /// </summary>
  [ValueConversion(typeof(int), typeof(Visibility))]
  public class IsNotZeroVisibilityConverter : LazySingleton<IsNotZeroVisibilityConverter>, IValueConverter
  {
    //-------------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      int? v = value as int?;
      if (v.HasValue)
        return (v.Value!=0) ? Visibility.Visible : Visibility.Collapsed;

      return Visibility.Collapsed;
    }

    //-------------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }


  //-------------------------------------------------------------------------
  /// <summary>
  /// Returns Visibility.Visible for == 0 Visibilitiy.Collapsed for anything else
  /// </summary>
  [ValueConversion(typeof(int), typeof(Visibility))]
  public class IsZeroVisibilityConverter : LazySingleton<IsZeroVisibilityConverter>, IValueConverter
  {
    //-------------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      int? v = value as int?;
      if (v.HasValue)
        return (v.Value == 0) ? Visibility.Visible : Visibility.Collapsed;

      return Visibility.Collapsed;
    }

    //-------------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }

  //-------------------------------------------------------------------------
  /// <summary>
  /// Returns Visibility.Visible when the value == parameter Visibilitiy.Collapsed for anything else
  /// 
  /// Example:
  ///     Converter={x:Static sfConverters:IsEqualToValueConverter.Instance}, ConverterParameter={x:Static csdh:SimStateMonitor+ESimMode.LIVE}
  /// </summary>
  [ValueConversion(typeof(object), typeof(Visibility))]
  public class IsEqualToValueConverter : LazySingleton<IsEqualToValueConverter>, IValueConverter
  {
    //-------------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null && parameter != null)
      {
        return value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;

      }

      return Visibility.Collapsed;
    }

    //-------------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }

  //-------------------------------------------------------------------------
  //
  // Returns Visibility.Visible if value != null
  //
  [ValueConversion(typeof(object), typeof(Visibility))]
  public class IsNotNullVisibilityConverter : LazySingleton<IsNotNullVisibilityConverter>, IValueConverter
  {
    //-------------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    //-------------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }

  //-------------------------------------------------------------------------
  //
  // Returns Visibility.Visible if value == null
  //
  [ValueConversion(typeof(object), typeof(Visibility))]
  public class IsNullVisibilityConverter : LazySingleton<IsNullVisibilityConverter>, IValueConverter
  {

    //-------------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    //-------------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }

}
