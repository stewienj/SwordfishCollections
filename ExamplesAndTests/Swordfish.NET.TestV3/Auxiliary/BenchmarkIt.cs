using System;
using System.Diagnostics;

namespace Swordfish.NET.TestV3.Auxiliary
{
    public class BenchmarkIt : IDisposable
    {
        private Stopwatch _stopwatch = new Stopwatch();
        public BenchmarkIt(string description)
        {
            Console.WriteLine(description);
            _stopwatch.Start();
        }
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            _stopwatch.Stop();
            if (isDisposing)
            {
                Console.WriteLine($"Duration {_stopwatch.Elapsed}");
            }
        }
    }
}
