using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework.Pool
{
    internal class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new();
        private int m_MaxSize;
        internal Func<T> CreateFunc;

        public int MaxSize
        {
            get { return m_MaxSize; }
            set
            {
                m_MaxSize = Math.Max(0, value);
                while (Size() > m_MaxSize)
                {
                    Get();
                }
            }
        }

        public ObjectPool(Func<T> CreateFunc, int maxSize = 100)
        {
            MaxSize = maxSize;

            if (CreateFunc == null)
            {
                this.CreateFunc = () => new T();
            }
            else
            {
                this.CreateFunc = CreateFunc;
            }
        }

        public int Size()
        {
            return m_Stack.Count;
        }

        public void Clear()
        {
            m_Stack.Clear();
        }

        public T Get()
        {
            T evt = m_Stack.Count == 0 ? CreateFunc() : m_Stack.Pop();
            return evt;
        }

        public void Release(T element)
        {
#if UNITY_DEBUG
            if (m_Stack.Contains(element)) //this is O(n) and will be come an issue when the pool size is large
#else
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
#endif
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");

            if (m_Stack.Count < MaxSize)
            {
                m_Stack.Push(element);
            }
        }
    }
}