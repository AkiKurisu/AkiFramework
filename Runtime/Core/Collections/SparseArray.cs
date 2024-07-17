#pragma warning disable CS9258
using System;
using System.Collections;
using System.Collections.Generic;
namespace Kurisu.Framework.Collections
{
    /// <summary>
    /// Similar to UE TSparseArray.
    /// A list where element indices aren't necessarily contiguous. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SparseList<T> : IEnumerable<T>
    {
        private struct FreeListLink
        {
            public int last;
            public int next;
            public T value;
        }
        private readonly List<FreeListLink> data;
        private readonly List<bool> allocationFlags;
        private int firstFreeIndex;
        private int numFreeIndices;
        private readonly int capacity;
        public int FirstFreeIndex => firstFreeIndex;
        public int NumFreeIndices => numFreeIndices;
        public T this[int index]
        {
            get
            {
                if (!IsValidIndex(index)) return default;
                return data[index].value;
            }
            set
            {
                if (!IsValidIndex(index)) return;
                var link = data[index];
                link.value = value;
                data[index] = link;
            }
        }
        public int Count => data.Count - numFreeIndices;
        public SparseList(int length, int capacity)
        {
            this.capacity = capacity;
            data = new List<FreeListLink>(length);
            allocationFlags = new List<bool>(length);
            for (int i = 0; i < length; ++i)
            {
                data.Add(new FreeListLink() { last = i - 1, next = (i + 1) >= length ? -1 : (i + 1) });
                allocationFlags.Add(false);
            }
            firstFreeIndex = 0;
            numFreeIndices = length;
        }
        public void Add(T element)
        {
            if (numFreeIndices > 0)
            {
                // update current
                var link = data[firstFreeIndex];
                link.value = element;
                int next = link.next;
                link.next = -1;
                data[firstFreeIndex] = link;
                // update next if exist
                if (next != -1)
                {
                    var nextLink = data[next];
                    nextLink.last = -1;
                    data[next] = nextLink;
                }
                // set flag
                allocationFlags[firstFreeIndex] = true;
                firstFreeIndex = next;
                numFreeIndices--;
            }
            else
            {
                if (data.Count == capacity)
                {
                    throw new ArgumentOutOfRangeException($"Sparse array should not exceed capacity {capacity}!");
                }
                data.Add(new FreeListLink()
                {
                    value = element,
                    last = -1,
                    next = -1
                });
                allocationFlags.Add(false);
            }
        }
        public int AddUninitialized()
        {
            int index;
            if (numFreeIndices > 0)
            {
                // update current
                var link = data[firstFreeIndex];
                link.value = default;
                int next = link.next;
                link.next = -1;
                data[firstFreeIndex] = link;
                // update next if exist
                if (next != -1)
                {
                    var nextLink = data[next];
                    nextLink.last = -1;
                    data[next] = nextLink;
                }
                // set flag
                allocationFlags[firstFreeIndex] = true;
                index = firstFreeIndex;
                firstFreeIndex = next;
                numFreeIndices--;
            }
            else
            {
                index = data.Count;
                if (index == capacity)
                {
                    throw new ArgumentOutOfRangeException($"Sparse array should not exceed capacity {capacity}!");
                }
                data.Add(new FreeListLink()
                {
                    value = default,
                    last = -1,
                    next = -1
                });
                allocationFlags.Add(false);
            }
            return index;
        }
        public void RemoveAt(int index)
        {
            var link = data[index];
            link.value = default;
            link.last = -1;
            allocationFlags[index] = false;
            if (firstFreeIndex == -1)
            {
                // as link list header
                link.next = -1;
            }
            else
            {
                // link to header
                var headerLink = data[firstFreeIndex];
                headerLink.last = index;
                data[firstFreeIndex] = headerLink;
                link.last = firstFreeIndex;
            }
            // update removed link
            firstFreeIndex = index;
            data[index] = link;
            numFreeIndices++;
        }
        public void Clear()
        {
            numFreeIndices = data.Count;
            for (int i = 0; i < numFreeIndices; ++i)
            {
                data[i] = new FreeListLink() { last = i - 1, next = (i + 1) >= numFreeIndices ? -1 : (i + 1) };
                allocationFlags[i] = false;
            }
            firstFreeIndex = 0;
        }
        public bool IsValidIndex(int index)
        {
            return allocationFlags[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        private struct Enumerator : IEnumerator<T>
        {
            private SparseList<T> sparseList;
            private int currentIndex;

            public Enumerator(SparseList<T> list)
            {
                sparseList = list;
                currentIndex = -1;
            }

            public readonly T Current
            {
                get
                {
                    return sparseList.data[currentIndex].value;
                }
            }

            readonly object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                currentIndex++;
                while (currentIndex < sparseList.data.Count)
                {
                    if (sparseList.allocationFlags[currentIndex])
                    {
                        return true;
                    }
                    currentIndex++;
                }
                return false;
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public void Dispose()
            {
                sparseList = null;
            }
        }
    }
}
#pragma warning restore CS9258