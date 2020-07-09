using System;
using System.Text;

namespace BigCsvFileViewer.BigTextFileViewModel
{
    public class TextWriterLineCounter : System.IO.TextWriter
    {
        public class StringEventArgs : EventArgs
        {
            public StringEventArgs(string value)
            {
                Value = value;
            }

            public string Value { get; set; }
        }

        private StringBuilder _buffer = new StringBuilder();
        private System.IO.TextWriter _baseTextWriter;
        public TextWriterLineCounter(System.IO.TextWriter baseTextWriter)
        {
            _baseTextWriter = baseTextWriter;
        }

        public override Encoding Encoding
        {
            get
            {
                return _baseTextWriter.Encoding;
            }
        }

        public override void Write(char value)
        {
            if (value == '\n')
            {
                LineAdded?.Invoke(this, new StringEventArgs(_buffer.ToString()));
                _buffer.Clear();
            }
            if (value > 31)
            {
                _buffer.Append(value);
            }
        }

        public event EventHandler<StringEventArgs> LineAdded;
    }
}
