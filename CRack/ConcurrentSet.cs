using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CRack
{
    public class ConcurrentSet<T> : IEnumerable<T>
    {
        private Pair _pair;

        public ConcurrentSet() {}

        public ConcurrentSet(T value)
        {
            _pair = new Pair(value);
        }

        public void Add(T value)
        {
            Pair newPair;
            do
            {
                newPair = new Pair(value, _pair);
            } while (!ReferenceEquals(newPair.Next, Interlocked.CompareExchange(ref _pair, newPair, newPair.Next)));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate(_pair).GetEnumerator();
        }

        private static IEnumerable<T> Enumerate(Pair pair)
        {
            while (pair != null)
            {
                yield return pair.Value;
                pair = pair.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class Pair
        {
            private readonly T _value;
            private readonly Pair _next;

            public Pair(T value)
                : this(value, null)
            {
            }

            public Pair(T value, Pair next)
            {
                _value = value;
                _next = next;
            }

            public Pair Next
            {
                get { return _next; }
            }

            public T Value
            {
                get { return _value; }
            }
        }
    }
}