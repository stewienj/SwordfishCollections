using Swordfish.NET.Collections;
using System;
using System.Windows.Controls;

namespace Swordfish.NET.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ObservableSortedDictionaryTest : UserControl
    {
        public ObservableSortedDictionaryTest()
        {
            this.Initialized += new EventHandler(MainWindow_Initialized);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the controls after they have been created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Initialized(object sender, EventArgs e)
        {
            UnsortedView.InitializeList(new ObservableDictionary<string, string>(), 10, false);
            SortedView.InitializeList(new ObservableSortedDictionary<string, string>(), 10, false);
            ConcurrentUnsortedView.InitializeList(new ConcurrentObservableDictionary<string, string>(), 10, true);
            ConcurrentSortedView.InitializeList(new ConcurrentObservableSortedDictionary<string, string>(), 10, true);
        }
    }
}
