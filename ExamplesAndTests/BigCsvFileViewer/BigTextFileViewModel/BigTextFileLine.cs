using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.WPF.ViewModel
{


  public class BigTextFileLine : BigFileLine
  {
    private Lazy<string[]> _columns;


    public BigTextFileLine()
    {
      _columns = new Lazy<string[]>(() => new string[0], true);
    }

    public BigTextFileLine(StreamReader stream)
    {
      string line = stream.ReadLine();
      if (line != null)
      {
        // note space is 32 so this will replace all invalid chars and space with a •
        line = new string(line.Select(x => x > 32 ? x : '•').ToArray());
        _columns = new Lazy<string[]>(() => line.Split(Separators), true);
        IsValid = true;
      }
    }

    /// <summary>
    /// Provides a list of all the starts of lines, including 0 fo
    /// </summary>
    /// <param name="inputStream"></param>
    /// <returns></returns>
    public override IEnumerable<long> CountLines(Stream inputStream)
    {
      // Count the lines by counting the number of line feed characters
      int lineFeed = 10;
      long? lineBeginPos = 0;
      int inByte;
      while ((inByte = inputStream.ReadByte()) != -1)
      {
        if (lineBeginPos.HasValue)
        {
          yield return lineBeginPos.Value;
          lineBeginPos = null;
        }
        if (inByte == lineFeed)
        {
          lineBeginPos = inputStream.Position;
        }
      }
    }


    public override List<BigFileLine> GetLines(Stream stream, int linesToSkip, int linesToRead)
    {
      List<BigFileLine> list = new List<BigFileLine>();
      using (StreamReader lineReader = new StreamReader(stream, Encoding.UTF8, false, 4096, true))
      {
        // Skip the Required lines
        for (int i = 0; i < linesToSkip; ++i)
        {
          lineReader.ReadLine();
        }

        for (int i = 0; i < linesToRead; i++)
        {
          BigFileLine displayLine = new BigTextFileLine(lineReader);
          if (displayLine.IsValid)
          {
            list.Add(displayLine);
          }
          else
          {
            // TODO log something
            list.Add(new BigTextFileLine());
            break;
          }
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
