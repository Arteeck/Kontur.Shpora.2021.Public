using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
    public class SimpleLockThreadPool : IThreadPool
    {
        private int count;
        readonly object _locker = new object();
        EventWaitHandle wh = new AutoResetEvent(false);
        private Thread[] Threads;
        private Queue<Action> actions = new Queue<Action>();

        public SimpleLockThreadPool(int count)
        {
            Threads = new Thread[count];
            this.count = count;
            for (int i = 0; i < count; i++)
            {
                Threads[i] = new Thread(Work);
                Threads[i].Start();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < count; i++)
            {
                EnqueueAction(null);
            }

            for (int i = 0; i < count; i++)
            {
                Threads[i].Join();
            }

            wh.Close();
        }

        public void EnqueueAction(Action action)
        {
            lock (_locker)
                actions.Enqueue(action);
            wh.Set();
        }

        private void Work()
        {
            while (true)
            {
                Action action = null;
                lock (_locker)
                    if (actions.Count > 0)
                    {
                        action = actions.Dequeue();
                        if (action == null)
                            return;
                    }

                if (action != null)
                    action.Invoke();
                else
                {
                    wh.WaitOne(); // No more tasks - wait for a signal
                }
            }
        }
    }
}