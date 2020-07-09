using BigCsvFileViewer.Auxiliary;
using Swordfish.NET.WPF.Controls;
using System.ComponentModel;
using System.IO;
using System.Windows;


namespace BigCsvFileViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _titleSuffix;
        public MainWindow()
        {
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
