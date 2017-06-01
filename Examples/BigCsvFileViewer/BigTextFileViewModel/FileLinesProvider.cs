using Swordfish.NET.Collections;
using Swordfish.NET.General;
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
  public class FileLinesProvider : LinesProvider
  {
    protected string _inputFilename;
    /// <summary>
    /// Stores the file position for each 100 lines
    /// </summary>
    protected ConcurrentDictionary<int, long> _100LineIndex = new ConcurrentDictionary<int, long>();
    protected Task _lineCountTask;
    protected Stream _inputStream;
    protected bool _disposed = false;
    protected BigFileLine _lineConverter;
    protected int _maxRows;

    /// <summary>
    /// Initializes a new instance of the <see cref="DemoCustomerProvider"/> class.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="fetchDelay">The fetch delay.</param>
    public FileLinesProvider(string inputFilename, int maxRows, BigFileLine lineConverter)
    {
      _maxRows = maxRows;
      _lineConverter = lineConverter;
      _inputFilename = inputFilename;
      _inputStream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      var firstLine = _lineConverter.GetLines(_inputStream, 0, 1).FirstOrDefault();
      if (firstLine != null)
      {
        Header = firstLine.Columns.Aggregate((a, b) => a + "," + b);
      }
      _lineCountTask = Task.Run(() => CountLines(inputFilename, maxRows));
    }

    public FileLinesProvider() { }

    public override void Dispose()
    {
      lock (_inputStream)
      {
        _inputStream.Dispose();
        _disposed = true;
      }
    }

    /// <summary>
    /// Counts the file lines using the line converter provided
    /// </summary>
    /// <param name="inputFilename"></param>
    /// <param name="maxRows"></param>
    private void CountLines(string inputFilename, int maxRows)
    {
      using (FileStream fileStream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 1024))
      {
        int count = 0;
        _100LineIndex.Clear();
        foreach (var linePosition in _lineConverter.CountLines(fileStream))
        {
          if ((count % 100) == 0)
          {
            _100LineIndex.TryAdd(count / 100, linePosition);
          }
          Count = ++count;
          if (count >= maxRows || _disposed)
          {
            break;
          }
        }
        //Trace.WriteLine("Finish counting up to " + count.ToString() + " lines.");
      }
    }


    /// <summary>
    /// Fetches a range of items.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The number of items to fetch.</param>
    /// <returns></returns>
    public override IList<BigFileLine> FetchRange(int startIndex, int pageCount, out int overallCount)
    {
      //Trace.WriteLine("FetchRangeBigText: " + startIndex + "," + pageCount);
      lock (_inputStream)
      {
        if (_disposed || (startIndex / 100 >= _100LineIndex.Count))
        {
          overallCount = _lastFetchCount;
          return new List<BigFileLine>();
        }

        int pos100BoundaryIndex = startIndex / 100;
        long pos100Boundary = _100LineIndex[startIndex / 100];
        _inputStream.Seek(pos100Boundary, SeekOrigin.Begin);

        List<BigFileLine> list = _lineConverter.GetLines(_inputStream, startIndex - pos100BoundaryIndex * 100, Math.Min(startIndex + pageCount, Count) - startIndex);

        BlockingCollection<BigFileLine> columnCounter = StartCountingColumns();
        foreach (var line in list)
        {
          columnCounter.Add(line);
        }
        columnCounter.CompleteAdding();

        overallCount = _lastFetchCount;
        return list;
      }
    }

    /// <summary>
    /// Allows for counting the columns from blocks of BigFileLines on another thread
    /// </summary>
    /// <returns></returns>
    private BlockingCollection<BigFileLine> StartCountingColumns()
    {
      BlockingCollection<BigFileLine> columnsToCount = new BlockingCollection<BigFileLine>();
      Task.Run(() =>
      {
        foreach (var columnSource in columnsToCount.GetConsumingEnumerable())
        {
          MaxColumns = columnSource.Columns.Length;
        }
      });
      return columnsToCount;
    }


    public int MaxRows
    {
      get
      {
        return _maxRows;
      }
    }

    public string Filename
    {
      get
      {
        return _inputFilename;
      }
    }

  }
}
