#pragma warning disable CS9258
using System;
using System.Collections;
using System.Collections.Generic;
namespace Chris.Collections
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
        public int InternalCapacity => data.Capacity;
        public int Capacity => capacity;
        public T this[int index]
        {
            get
            {
                if (!IsAllocated(index)) return default;
                return data[index].value;
            }
            set
            {
                if (!IsAllocated(index)) return;
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
                data.Add(new FreeListLink()
                {
                    last = i - 1,
                    value = default,
                    next = ((i + 1) >= length) ? -1 : (i + 1)
                });
                allocationFlags.Add(false);
            }
            firstFreeIndex = 0;
            numFreeIndices = length;
        }
        public int Add(T element)
        {
            int index;
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
                index = firstFreeIndex;
                firstFreeIndex = next;
                numFreeIndices--;
            }
            else
            {
                index = data.Count;
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
                allocationFlags.Add(true);
            }
            return index;
        }
        public int AddUninitialized()
        {
            return Add(default);
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
                link.next = firstFreeIndex;
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
                data[i] = new FreeListLink()
                {
                    last = i - 1,
                    next = ((i + 1) >= numFreeIndices) ? -1 : (i + 1)
                };
                allocationFlags[i] = false;
            }
            firstFreeIndex = 0;
        }
        public void Shrink()
        {
            int firstIndexToRemove = allocationFlags.FindLastIndex(static x => x) + 1;
            int count = data.Count;
            if (firstIndexToRemove < count)
            {
                if (NumFreeIndices > 0)
                {
                    int freeIndex = FirstFreeIndex;
                    while (freeIndex != -1)
                    {
                        if (freeIndex >= firstIndexToRemove)
                        {
                            int PrevFreeIndex = data[freeIndex].last;
                            int NextFreeIndex = data[freeIndex].next;
                            if (NextFreeIndex != -1)
                            {
                                var d = data[NextFreeIndex];
                                d.last = PrevFreeIndex;
                                data[NextFreeIndex] = d;
                            }
                            if (PrevFreeIndex != -1)
                            {
                                var d = data[PrevFreeIndex];
                                d.next = NextFreeIndex;
                                data[PrevFreeIndex] = d;
                            }
                            else
                            {
                                firstFreeIndex = NextFreeIndex;
                            }
                            --numFreeIndices;

                            freeIndex = NextFreeIndex;
                        }
                        else
                        {
                            freeIndex = data[freeIndex].next;
                        }
                    }
                }
                data.RemoveRange(firstIndexToRemove, count - firstIndexToRemove);
                allocationFlags.RemoveRange(firstIndexToRemove, count - firstIndexToRemove);
            }
            // shrink list
            data.Capacity = allocationFlags.Capacity = data.Count;
        }
        public bool IsAllocated(int index)
        {
            if (index < 0 || index >= allocationFlags.Count) return false;
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