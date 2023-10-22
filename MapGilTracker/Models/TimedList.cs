using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;

/* 
 * Note for the reader:
 * The reason I did it this way, with manual checks instead of proper threaded Timers is because 
 * threads are a nightmare. If done incorrectly, could lead to memory leaks or objects not being 
 * deleted properly, etc. This is a small enough application that I don't care about precision
 * down to the millisecond.
 */

namespace MapGilTracker.Models
{
    public class TimedListItem<T>
    {
        public T value;
        public DateTime expiry;

        public TimedListItem(T value, int expirySec)
        {
            this.value = value;
            expiry = DateTime.Now.AddSeconds(expirySec);
        }
    }

    public class TimedList<T>
    {
        private int expirySec;
        private List<TimedListItem<T>> itemList = new List<TimedListItem<T>>();

        public int Count {
            get {
                return itemList.Count;
            }
        }

        public TimedList(int expirySec = 20) 
        {
            this.expirySec = expirySec;
        }

        public TimedListItem<T> Add(T item)
        {
            var t = new TimedListItem<T>(item, expirySec);
            itemList.Add(t);
            return t;
        }

        public void Remove(TimedListItem<T> item)
        {
            Services.Log.Debug($"Removing Item: {item.value}!");
            itemList.Remove(item);
            Services.Log.Debug($"New len: {Count}!");
        }

        public TimedListItem<T>? Contains(T target)
        {
            // Purge, ensure all data is ok
            Purge();

            // Iterate and return
            foreach (var item in itemList)
                if (item.value!.Equals(target))
                    return item;
            return null;
        }

        public int Purge()
        {
            int count = 0;
            var now = DateTime.Now;
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                if (now > item.expiry)
                {
                    itemList.Remove(item);
                    i--; count++;
                }
            }
            Services.Log.Debug($"Purged {count} items!");
            return count;
        }

        public IEnumerator<TimedListItem<T>> GetEnumerator()
        {
            return itemList.GetEnumerator();
        }
    }
}
