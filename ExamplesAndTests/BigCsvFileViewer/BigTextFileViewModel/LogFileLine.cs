using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BigCsvFileViewer.BigTextFileViewModel
{
    public class LogFileLine : BigTextFileLine
    {
        private string[] _columns;
        public LogFileLine()
        {
            _columns = new string[0];
        }

        public LogFileLine(StreamReader stream) : base()
        {
            string line = stream.ReadLine();
            if (line != null)
            {
                _columns = new string[] { line };
                IsValid = true;
            }
            else
            {
                _columns = new string[0];
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
                    list.Add(new LogFileLine(lineReader));
                }
            }
            return list;
        }

        public override string[] Columns
        {
            get
            {
                return _columns;
            }
        }
    }
}
