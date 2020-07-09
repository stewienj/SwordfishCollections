using System;

namespace BigCsvFileViewer.Auxiliary
{
    public class LazySingleton<T>
    {
        private static Lazy<T> _instance = new Lazy<T>(true);
        public static T Instance
        {
            get
            {
                return _instance.Value;
            }
        }

    }
}
