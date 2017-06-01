using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.WPF.ViewModel
{
  /// <summary>
  /// Column provider for binary files
  /// </summary>
  public class BigBinFileLine : BigFileLine
  {
    private static int _bytesPerLine = 64;
    private Lazy<string[]> _columns;

    public BigBinFileLine()
    {
      _columns = new Lazy<string[]>(() => new string[0]);
    }

    public BigBinFileLine(Stream stream)
    {
      byte[] buffer = new byte[_bytesPerLine];
      int bytesRead = 0;
      IsValid = (bytesRead = stream.Read(buffer, 0, _bytesPerLine)) > 0;
      if (IsValid)
      {
        _columns = new Lazy<string[]>(() => buffer.Take(bytesRead).Select(x => x.ToString("x").PadLeft(2, '0')).ToArray(), true);
      }
    }

    /// <summary>
    /// Provides a list of all the starts of lines, including 0 fo
    /// </summary>
    /// <param name="inputStream"></param>
    /// <returns></returns>
    public override IEnumerable<long> CountLines(Stream inputStream)
    {
      long? lineBeginPos = 0;
      byte[] buffer = new byte[_bytesPerLine];
      int bytesRead;
      while ((bytesRead = inputStream.Read(buffer, 0, _bytesPerLine)) > 0)
      {
        if (lineBeginPos.HasValue)
        {
          yield return lineBeginPos.Value;
          lineBeginPos = null;
        }
        lineBeginPos = inputStream.Position;
        if (bytesRead < _bytesPerLine)
        {
          break;
        }
      }
    }

    public override List<BigFileLine> GetLines(Stream stream, int linesToSkip, int linesToRead)
    {
      List<BigFileLine> list = new List<BigFileLine>();
      int bytesToSkip = linesToSkip * _bytesPerLine;
      byte[] buffer = new byte[bytesToSkip];
      if (stream.Read(buffer, 0, bytesToSkip) < bytesToSkip)
      {
        return list;
      }

      for (int i = 0; i < linesToRead; i++)
      {
        BigFileLine displayLine = new BigBinFileLine(stream);
        if (displayLine.IsValid)
        {
          list.Add(displayLine);
        }
        else
        {
          // TODO log something
          break;
        }
      }

      return list;
    }

    public override string[] Columns
    {
      get
      {
        return _columns.Value;
      }
    }
  }
}
