using System;

namespace BigCsvFileViewer.BigTextFileViewModel
{
    public class MaxColumnsArgs : EventArgs
    {
        public MaxColumnsArgs(int maxColumns)
        {
            MaxColumns = maxColumns;
        }

        public int MaxColumns
        {
            get;
            private set;
        }
    }
}
