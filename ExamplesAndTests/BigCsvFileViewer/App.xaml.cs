using System.IO;
using System.Windows;

namespace BigCsvFileViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow window = new BigCsvFileViewer.MainWindow();
            if (e.Args.Length > 0)
            {
                string fileToLoad = e.Args[0];
                FileInfo fi = new FileInfo(fileToLoad);
                if (fi.Exists)
                {
                    window.Filename = fileToLoad;
                }
            }
            window.ShowDialog();
            this.Shutdown();
        }
    }
}
