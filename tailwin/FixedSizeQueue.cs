using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tailwin
{
    /// <summary>
    /// A queue with a maximum number of elements. Elements are dropped (dequeued) automatically when 
    /// limit is exceeded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedSizedQueue<T>
    {
        ConcurrentQueue<T> q = new ConcurrentQueue<T>();

        public int Limit { get; set; }

        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            lock (this)
            {
                T overflow;
                while (q.Count > Limit && q.TryDequeue(out overflow)) ;
            }
        }

        public bool TryDequeue(out T result)
        {
            return q.TryDequeue(out result);
        }
    }
}
