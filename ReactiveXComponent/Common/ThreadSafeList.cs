using System;
using System.Collections;
using System.Collections.Generic;


namespace ReactiveXComponent.Common
{
    public class ThreadSafeList<T> : IList<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _lock = new object();

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                return _list.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            lock (_lock)
            {
                _list.Add(item);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _list.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_lock)
            {
                return _list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (_lock)
            {
                return _list != null && _list.Remove(item);
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            lock (_lock)
            {
                return _list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (_lock)
            {
                _list.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                _list.RemoveAt(index);
            }
        }

        public bool TryRemove(T item)
        {
            lock (_lock)
            {
                return _list.Contains(item) && _list.Remove(item);
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_lock)
                    {
                        return _list[index];
                    }
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                lock (_lock)
                {
                    this[index] = _list[index];
                }
            }
        }
    }
}
