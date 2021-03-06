using System;

namespace OurPresence.Liquid.Util
{
    internal class WeakTable<TKey, TValue> where TValue : class
    {
        private struct Bucket
        {
            public TKey Key;
            public WeakReference Value;
        }

        private readonly Bucket[] _buckets;

        public WeakTable(int size)
        {
            _buckets = new Bucket[size];
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var ret))
                    throw new ArgumentException("Weak Table Key Not Found");
                return ret;
            }
            set
            {
                var i = Math.Abs(key.GetHashCode()) % _buckets.Length;
                _buckets[i].Key = key;
                _buckets[i].Value = new WeakReference(value);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var i = Math.Abs(key.GetHashCode()) % _buckets.Length;
            WeakReference wr;
            if ((wr = _buckets[i].Value) == null || !_buckets[i].Key.Equals(key))
            {
                value = null;
                return false;
            }
            value = (TValue)wr.Target;
            return wr.IsAlive;
        }

        public void Remove(TKey key)
        {
            var i = Math.Abs(key.GetHashCode()) % _buckets.Length;
            if (_buckets[i].Key.Equals(key))
                _buckets[i].Value = null;
        }
    }
}
