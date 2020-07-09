using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigCsvFileViewer.Auxiliary
{
    internal static class Extensions
    {

        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            if (batchSize == 0)
            {
                yield break;
            }
            List<T> retVal = new List<T>();
            int count = 0;
            foreach (var item in source)
            {
                if (count >= batchSize)
                {
                    yield return retVal;
                    count = 0;
                    retVal = new List<T>();
                }
                retVal.Add(item);
                count++;
            }
            yield return retVal;
        }
    }
}
