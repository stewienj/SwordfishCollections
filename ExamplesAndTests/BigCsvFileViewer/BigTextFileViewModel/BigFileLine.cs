using System;
using System.Collections.Generic;
using System.IO;

namespace BigCsvFileViewer.BigTextFileViewModel
{
    public abstract class BigFileLine
    {
        public class ArrayWrapper
        {
            public ArrayWrapper()
            {
            }

            public string this[int index]
            {
                get
                {
                    if (index >= 0 && index < BaseArray.Length)
                    {
                        return BaseArray[index];
                    }
                    else
                    {
                        return "";
                    }
                }
            }

            public string[] BaseArray { get; set; }
        }

        private static Lazy<char[]> _separators = new Lazy<char[]>(() => ",;\t".ToCharArray(), true);

        public static char[] Separators
        {
            get
            {
                return _separators.Value;
            }
        }

        private ArrayWrapper _columnsSafe = new ArrayWrapper();
        public ArrayWrapper ColumnsSafe
        {
            get
            {
                _columnsSafe.BaseArray = Columns;
                return _columnsSafe;

            }
        }

        public abstract string[] Columns { get; }

        public bool IsValid { get; protected set; }

        public abstract List<BigFileLine> GetLines(Stream inputStream, int linesToSkip, int linesToRead);

        /// <summary>
        /// Counts the lines in a file by returning the file position of the beginning of each line, including 0 for the first line
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public abstract IEnumerable<long> CountLines(Stream inputStream);
    }
}
