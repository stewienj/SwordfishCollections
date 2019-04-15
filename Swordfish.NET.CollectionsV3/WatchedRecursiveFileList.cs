using System;
using System.IO;
using System.Linq;

namespace Swordfish.NET.Collections
{
  public class WatchedRecursiveFileList : IDisposable
  {

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// A list of files that are updated when the file system changes.
    /// </summary>
    private ConcurrentObservableCollection<string> _fileList;
    /// <summary>
    /// File system watcher that detects when any files have been added, deleted
    /// or modified at the file system level.
    /// </summary>
    private FileSystemWatcher _watcher;
    /// <summary>
    /// The location of the directory that is watched
    /// </summary>
    private string _watchedDirectoryLocation;
    /// <summary>
    /// File filter for which files are watched
    /// </summary>
    private string _fileFilter;

    #endregion Private Fields

    // ************************************************************************
    // Public Methods
    // ************************************************************************
    #region Public Methods

    public WatchedRecursiveFileList(DirectoryInfo directory, string fileFilter = "*.3ds")
    {
      if (!directory.Exists)
        directory.Create();

      _fileList = new ConcurrentObservableCollection<string>();
      _fileFilter = fileFilter;
      _watchedDirectoryLocation = directory.FullName;

      _watcher = new FileSystemWatcher(_watchedDirectoryLocation, _fileFilter);
      _watcher.IncludeSubdirectories = true;
      _watcher.Filter = _fileFilter;
      _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

      _watcher.Changed += new FileSystemEventHandler(watcher_Changed);
      _watcher.Created += new FileSystemEventHandler(watcher_Changed);
      _watcher.Deleted += new FileSystemEventHandler(watcher_Changed);
      _watcher.Renamed += new RenamedEventHandler(watcher_Renamed);

      _watcher.EnableRaisingEvents = true;

      _fileList.Clear();
      _fileList.Add("");
      BuildFileList(null, "");
    }

    public string GetFullPath(string filename, string extension = ".3ds")
    {
      if (string.IsNullOrWhiteSpace(filename))
      {
        return null;
      }
      else
      {
        var fullPath = Path.Combine(_watchedDirectoryLocation, filename);
        if (File.Exists(fullPath))
        {
          return fullPath;
        }
        else
        {
          return Directory.EnumerateFiles(_watchedDirectoryLocation, Path.GetFileNameWithoutExtension(filename) + extension, SearchOption.AllDirectories).FirstOrDefault();
        }
      }
    }

    public void Dispose()
    {
      if (_watcher != null)
      {
        _watcher.Dispose();
        _watcher = null;
      }
    }

    #endregion Public Methods

    // ************************************************************************
    // Private Methods
    // ************************************************************************
    #region Private Methods

    private void BuildFileList(DirectoryInfo dir, string directory)
    {
      if (dir == null)
      {
        dir = new DirectoryInfo(_watchedDirectoryLocation);
      }
      FileInfo[] rgFiles = dir.GetFiles(_fileFilter);
      foreach (FileInfo fi in rgFiles)
      {
        _fileList.Add(Path.Combine(directory, fi.Name));
      }
      DirectoryInfo[] subDirs = dir.GetDirectories();
      foreach (DirectoryInfo subDir in subDirs)
      {
        BuildFileList(subDir, Path.Combine(directory, subDir.Name));
      }
    }

    private void RebuildFileList(FileSystemEventArgs e)
    {
      if (e is RenamedEventArgs)
      {
        int i = _fileList.IndexOf(((RenamedEventArgs)e).OldName);
        _fileList[i] = e.Name;
      }
      else
      {
        switch (e.ChangeType)
        {
          case WatcherChangeTypes.Changed:
            // Need to send event
            int i = _fileList.IndexOf(e.Name);
            _fileList[i] = e.Name;
            break;
          case WatcherChangeTypes.Created:
            _fileList.Add(e.Name);
            break;
          case WatcherChangeTypes.Deleted:
            if (_fileList.Contains(e.Name))
            {
              _fileList.Remove(e.Name);
            }
            break;
          default:
            break;
        }
      }
    }

    #endregion Private Methods

    // ************************************************************************
    // Event Handlers and Events
    // ************************************************************************
    #region Event Handlers and Events

    private void watcher_Changed(object sender, FileSystemEventArgs e)
    {
      RebuildFileList(e);
    }

    private void watcher_Renamed(object sender, RenamedEventArgs e)
    {
      RebuildFileList(e);
    }

    #endregion Event Handlers and Events

    // ************************************************************************
    // Properties
    // ************************************************************************
    #region Properties

    /// <summary>
    /// Returns an observable list of models that can be bound to
    /// a combo box.
    /// </summary>
    public ConcurrentObservableCollection<string> FileList
    {
      get
      {
        return this._fileList;
      }
    }

    #endregion Properties
  }
}
