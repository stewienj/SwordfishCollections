using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Swordfish.NET.Collections;

namespace Swordfish.NET.Test {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class ObservableSortedDictionaryTest : UserControl {
    public ObservableSortedDictionaryTest() {
      this.Initialized += new EventHandler(MainWindow_Initialized);
      InitializeComponent();
    }

    /// <summary>
    /// Initializes the controls after they have been created
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void MainWindow_Initialized(object sender, EventArgs e) {
      UnsortedView.InitializeList(new ObservableDictionary<string, string>(), 10, false);
      SortedView.InitializeList(new ObservableSortedDictionary<string, string>(), 10, false);
      ConcurrentUnsortedView.InitializeList(new ConcurrentObservableDictionary<string, string>(), 10, true);
      ConcurrentSortedView.InitializeList(new ConcurrentObservableSortedDictionary<string, string>(), 10, true);
    }
  }
}
