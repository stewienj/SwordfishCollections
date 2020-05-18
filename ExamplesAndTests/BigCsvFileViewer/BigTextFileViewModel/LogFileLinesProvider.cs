using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.WPF.ViewModel
{
  public class LogFileLinesProvider : FileLinesProvider
  {
    private Stream _outputStream;
    private StreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DemoCustomerProvider"/> class.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="fetchDelay">The fetch delay.</param>
    public LogFileLinesProvider(string processName)
    {
      bool success = false;
      try {
        _lineConverter = new LogFileLine();
        _inputFilename = GetLogFileFilename(processName);
        _outputStream = new FileStream(_inputFilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
        _inputStream = new FileStream(_inputFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        _writer = new StreamWriter(_outputStream);
        _writer.AutoFlush = true;
        success = true;
        _externalTextWriter = new Lazy<TextWriter>(() => 
        {
          var textWriter = new TextWriterLineCounter(_writer);
          textWriter.LineAdded += (s, e) => AddLine(e.Value);
          return textWriter;
        }, true);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.WriteLine("Error Opening File");
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }

      if (!success)
      {
        _inputFilename = Path.GetTempFileName();
        _inputStream = new FileStream(_inputFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        _outputStream = new FileStream(_inputFilename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.WriteThrough | FileOptions.DeleteOnClose);
        _writer = new StreamWriter(_outputStream);
        _writer.AutoFlush = true;
      }

      Count = 0;
    }

    public string GetLogFileFilename(string processName)
    {
      string mainModule = Process.GetCurrentProcess().MainModule.FileName;
      string mainModulePath = Path.GetDirectoryName(mainModule);
      string mainModuleName = Path.GetFileNameWithoutExtension(processName);
      string now = DateTime.Now.ToString("yyyyMMddTHHmmssZ");
      string logfilePath = Path.Combine(mainModulePath, "LogFiles");
      if (!Directory.Exists(logfilePath))
      {
        try
        {
          Directory.CreateDirectory(logfilePath);
        }
        catch (Exception)
        {

        }
      }
      return Path.Combine(logfilePath, mainModuleName + now + ".log");
    }

    public override void Dispose()
    {
      lock (_outputStream)
      {
        _outputStream.Dispose();
        base.Dispose();
      }
    }

    public void AddLine(string line)
    {
      int count = Count;
      lock (_outputStream)
      {
        if (_disposed)
        {
          return;
        }
        if ((count % 100) == 0)
        {
          _100LineIndex.TryAdd(count / 100, _writer.BaseStream.Position);
        }
        _writer.WriteLine(line);
        _writer.Flush();
      }
      Count = ++count;

    }
    private Lazy<TextWriter> _externalTextWriter;
    public TextWriter TextWriter
    {
      get
      {
        return _externalTextWriter.Value;
      }
    }
  }

}
