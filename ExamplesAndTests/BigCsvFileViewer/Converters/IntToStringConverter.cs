using BigCsvFileViewer.Auxiliary;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Swordfish.NET.WPF.Converters {
  [ValueConversion(typeof(int), typeof(string))]
  public class IntToStringConverter : IValueConverter {

    private static Lazy<IntToStringConverter> _instance = new Lazy<IntToStringConverter>(true);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      double retVal = 0.0;
      if (value != null) {
        double.TryParse(value.ToString(), out retVal);
      }
      return retVal;
    }

    public static IntToStringConverter Instance {
      get {
        return _instance.Value;
      }
    }
  }

  [ValueConversion(typeof(int), typeof(string))]
  public class IntToFormattedStringConverter : IValueConverter
  {

    private static Lazy<IntToFormattedStringConverter> _instance = new Lazy<IntToFormattedStringConverter>(true);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var chars = value.ToString().Reverse().Batch(3).Select(x => new string(x.ToArray())).Aggregate((a, b) => a + "," + b).Reverse().ToArray();
      return new string(chars);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double retVal = 0.0;
      if (value != null)
      {
        double.TryParse(value.ToString(), out retVal);
      }
      return retVal;
    }

    public static IntToFormattedStringConverter Instance
    {
      get
      {
        return _instance.Value;
      }
    }

  }
}
