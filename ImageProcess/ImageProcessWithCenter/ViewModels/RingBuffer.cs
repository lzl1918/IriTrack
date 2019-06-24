using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcess.ViewModels
{
    public class RingBuffer<T> : IEnumerable<T>, IEnumerator<T>
    {
        private T[] m_array = null;

        private int m_startIndex = 0;
        private int m_offset = -1;

        public event EventHandler BufferChanged;
        public int Count { get; private set; } = 0;
        public int MaxLength { get; private set; } = 0;

        public T Current
        {
            get
            {
                return m_array[(m_offset + m_startIndex) % MaxLength];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }


        public RingBuffer(int maxLength)
        {
            MaxLength = maxLength;
            m_array = new T[maxLength];
            m_startIndex = 0;
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public void Add(T val)
        {
            m_array[(m_startIndex + Count) % MaxLength] = val;
            if (Count < MaxLength)
                Count++;
            else
                m_startIndex = (m_startIndex + 1) % MaxLength;
            BufferChanged?.Invoke(this, new EventArgs());
        }

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            if (m_offset == -1)
            {
                if (Count == 0)
                    return false;

                m_offset = 0;
                return true;
            }
            else
            {
                m_offset++;
                if (m_offset == Count)
                {
                    Reset();
                    return false;
                }
                else
                    return true;
            }
        }

        public void Reset()
        {
            m_offset = -1;
        }
    }
}
