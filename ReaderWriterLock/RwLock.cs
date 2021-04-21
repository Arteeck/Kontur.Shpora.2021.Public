using System;
using System.Threading;

namespace ReaderWriterLock
{
    public class RwLock : IRwLock
    {
        private readonly EventWaitHandle wh = new AutoResetEvent(true);
        private int readers = 0;
        private readonly object locker = new object();

        public void ReadLocked(Action action)
        {
            lock (locker)
            {
                Interlocked.Increment(ref readers);
            }

            action();
            Interlocked.Decrement(ref readers);
            wh.Set();
        }

        public void WriteLocked(Action action)
        {
            lock (locker)
            {
                while (readers != 0)
                {
                    wh.WaitOne();
                }

                action();
            }
        }
    }
}