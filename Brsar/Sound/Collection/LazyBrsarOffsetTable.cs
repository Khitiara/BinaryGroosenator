using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NxCore;

namespace Brsar.Sound.Collection
{
    public class LazyBrsarOffsetTable<T> : IList<T>
        where T : struct, IEndianAwareUnmanagedType
    {
        private Lazy<T>[] _lazies;

        public LazyBrsarOffsetTable(BrsarReader reader, uint offsetBase, uint tableOffset) {
            Count = (int)BinaryPrimitives.ReverseEndianness(reader.Handle.ReadUInt32(offsetBase + tableOffset));
            _lazies = new Lazy<T>[Count];
            for (uint i = 0; i < Count; i++) {
                uint idx = i;
                _lazies[i] = new Lazy<T>(() => {
                    reader.MarshalFromTableOffset(offsetBase, tableOffset, idx, out T value);
                    return value;
                });
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return _lazies.Select(l => l.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            throw new NotSupportedException();
        }

        public void Clear() {
            throw new NotSupportedException();
        }

        public bool Contains(T item) {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public bool Remove(T item) {
            throw new NotSupportedException();
        }

        public int Count { get; }

        public bool IsReadOnly => true;

        public int IndexOf(T item) {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item) {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException();
        }

        public T this[int index] {
            get => _lazies[index].Value;
            set => throw new NotSupportedException();
        }
    }
}