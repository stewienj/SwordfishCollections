using BigCsvFileViewer.Auxiliary;
using Microsoft.Win32;
using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BigCsvFileViewer.BigTextFileViewModel
{
    /// <summary>
    /// Virtualizing list of lines, used for viewing files that contain a lot of lines, up to 2 billion
    /// </summary>
    public class BigTextFileViewModel : ExtendedNotifyPropertyChanged, IDisposable
    {

        protected LinesProvider _linesProvider = null;

        public BigTextFileViewModel()
        {
        }

        /// <summary>
        /// Opens the file with a specified maxium number of rows with a default value that insures
        /// the max this code can handle isn't reached.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="maxRows"></param>
        public void OpenFile(string filename, int maxRows = Int32.MaxValue)
        {
            ColumnSelectorsVisible = false;
            if (_linesProvider != null)
            {
                _linesProvider.Dispose();
                _linesProvider = null;
            }
            // Select low level lines provider as being a binary lines provider, or a text line provider
            BigFileLine lineConverter = _binaryMode ? (BigFileLine)(new BigBinFileLine()) : (BigFileLine)(new BigTextFileLine());
            OpenProvider(new FileLinesProvider(filename, maxRows, lineConverter));
        }

        public void OpenProvider(LinesProvider linesProvider)
        {
            _linesProvider = linesProvider;
            // Handles when the number of columns we need to display the data changes
            _linesProvider.MaxColumnsChanged += (s, e) => ColumnCount = e.MaxColumns;
            ColumnCount = 0;
            var fileLines = new VirtualizingCollectionDynamicAsync<BigFileLine>(_linesProvider, 100 /*page size*/, 60000 /*timeout*/, 1000 /* loadCountInterval*/);
            FileLines = fileLines;
            fileLines.CountChanged += (s, e) =>
            {
                CountChanged?.Invoke(this, e);
                RaisePropertyChanged("RowCount");
            };
        }

        public void Dispose()
        {
            if (_linesProvider != null)
            {
                _linesProvider.Dispose();
                _linesProvider = null;
            }
        }

        private int _columnCount = 0;
        public int ColumnCount
        {
            get
            {
                return _columnCount;
            }
            protected set
            {
                SetProperty(ref _columnCount, value);
            }
        }



        public int RowCount
        {
            get
            {
                return FileLines != null ? FileLines.Count : 0;
            }
        }

        public string[] Headers
        {
            get
            {
                if (_linesProvider == null || string.IsNullOrWhiteSpace(_linesProvider.Header))
                {
                    return new string[0];
                }
                else
                {
                    return _linesProvider.Header.Split(BigFileLine.Separators);
                }
            }
        }

        Dictionary<int, int> _columnDisplayIndex = new Dictionary<int, int>();

        public Dictionary<int, int> ColumnDisplayIndex
        {
            get
            {
                return _columnDisplayIndex;
            }
        }

        private bool _columnSelectorsVisible = false;
        public bool ColumnSelectorsVisible
        {
            get
            {
                return _columnSelectorsVisible;
            }
            set
            {
                SetProperty(ref _columnSelectorsVisible, value);
                ColumnSelectorStatus = _columnSelectorsVisible ? "Export Selected Columns" : "Click To Select Columns";

            }
        }

        private string _columnSelectorStatus = "Click To Select Columns";
        public string ColumnSelectorStatus
        {
            get
            {
                return _columnSelectorStatus;
            }
            private set
            {
                SetProperty(ref _columnSelectorStatus, value);
            }
        }

        private Dictionary<int, bool> _columnsSelected = new Dictionary<int, bool>();
        public Dictionary<int, bool> ColumnsSelected
        {
            get
            {
                return _columnsSelected;
            }
        }

        private RelayCommandFactory _columnSelectorCommand = new RelayCommandFactory();
        public ICommand ColumnSelectorCommand
        {
            get
            {
                return _columnSelectorCommand.GetCommandAsync(async () =>
                {
                    if (!_columnSelectorsVisible)
                    {
                        ColumnSelectorsVisible = true;
                    }
                    else
                    {
                        if (!(_linesProvider is FileLinesProvider))
                        {
                            return;
                        }
                // Columns Selectors Have Been Set
                // Export the file
                SaveFileDialog fileDialog = new SaveFileDialog();
                        if (fileDialog.ShowDialog() == true)
                        {
                            int maxRows = ((FileLinesProvider)_linesProvider).MaxRows;
                            string sourceFilename = ((FileLinesProvider)_linesProvider).Filename;
                            string destFilename = fileDialog.FileName;

                            bool sameFile = Path.GetFullPath(destFilename).ToLower() == Path.GetFullPath(sourceFilename).ToLower();
                            if (sameFile)
                            {
                                _linesProvider.Dispose();
                                _linesProvider = null;
                                int suffix = 0;
                                string testPath = sourceFilename + ".bak" + suffix.ToString();
                                while (File.Exists(testPath))
                                {
                                    suffix++;
                                    testPath = sourceFilename + ".bak" + suffix.ToString();
                                }
                                File.Move(sourceFilename, testPath);
                                sourceFilename = testPath;
                            }

                            LongTaskMessage = "Copying Selected Columns to " + destFilename;
                            LongTaskProgress = 0.0;
                            await Task.Run(() =>
                    {
                          using (var reader = FileAndDirectoryUtilities.StreamReaderBuffered(sourceFilename))
                          {
                              using (var writer = FileAndDirectoryUtilities.StreamWriterBuffered(destFilename))
                              {
                                  double totalLength = reader.BaseStream.Length;
                                  Stopwatch timing = new Stopwatch();
                                  timing.Start();
                                  string line;
                                  List<int> columns = ColumnsSelected.
                            Where(x => x.Value).
                            Select(x => x.Key).
                            OrderBy(x => _columnDisplayIndex.ContainsKey(x) ? _columnDisplayIndex[x] : x).
                            ToList();
                                  while ((line = reader.ReadLine()) != null)
                                  {
                                      try
                                      {
                                          string[] parts = line.Split(',');
                                          writer.WriteLine(columns.Where(x => x < parts.Length).Select(x => parts[x]).Aggregate((a, b) => a + ',' + b));
                                          if (timing.ElapsedMilliseconds > 200)
                                          {
                                              LongTaskProgress = reader.BaseStream.Position / totalLength;
                                              timing.Restart();
                                          }
                                      }
                                      catch (Exception ex)
                                      {
                                          System.Diagnostics.Debug.WriteLine(ex.Message);
                                      }
                                  }
                                  LongTaskMessage = "Finished";
                              }
                          }
                      });
                            LongTaskMessage = "";
                            if (sameFile)
                            {
                                this.OpenFile(destFilename, maxRows);
                            }
                        }
                    }
                });
            }
        }

        private string _longTaskMessage = "";
        public string LongTaskMessage
        {
            get
            {
                return _longTaskMessage;
            }
            private set
            {
                SetProperty(ref _longTaskMessage, value);
            }
        }

        private double _longTaskProgress = 0.0;
        public double LongTaskProgress
        {
            get
            {
                return _longTaskProgress;
            }
            private set
            {
                SetProperty(ref _longTaskProgress, value);
            }
        }


        private IList _fileLines = null;
        public IList FileLines
        {
            get
            {
                return _fileLines;
            }
            protected set
            {
                SetProperty(ref _fileLines, value);
            }
        }

        private bool _fileHasHeader = false;
        public bool FileHasHeader
        {
            get
            {
                return _fileHasHeader;
            }
            set
            {
                SetProperty(ref _fileHasHeader, value);
            }
        }

        private bool _binaryMode = false;
        public bool BinaryMode
        {
            get
            {
                return _binaryMode;
            }
            set
            {
                if (SetProperty(ref _binaryMode, value))
                {
                    if (_linesProvider != null && _linesProvider is FileLinesProvider)
                    {
                        // File Already Open so, reopen
                        OpenFile(((FileLinesProvider)_linesProvider).Filename, ((FileLinesProvider)_linesProvider).MaxRows);
                    }
                }
            }
        }

        public event EventHandler CountChanged;
    }

}
