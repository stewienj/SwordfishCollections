using Swordfish.NET.Collections.Auxiliary;
using Swordfish.NET.WPF.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace BigCsvFileViewer {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    private string _titleSuffix;
    public MainWindow() {
      _titleSuffix = " - Big CSV File Viewer (Build " + PeHeaderReader.GetAssemblyHeader(typeof(MainWindow)).TimeStamp.ToString() + ") ";
      this.Title = _titleSuffix;
      InitializeComponent();
      DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(BigTextFileViewer.FilenameProperty, typeof(BigTextFileViewer));
      dpd.AddValueChanged(viewer, (s, e) =>
      {
        this.Title = Path.GetFileName(viewer.Filename) + _titleSuffix;
      });
    }

    public string Filename
    {
      get
      {
        return viewer.Filename;
      }
      set
      {
        if (viewer != null && viewer.Filename != value)
        {
            viewer.Filename = value;
        }
      }
    }
  }
}
