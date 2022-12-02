using System.Collections.Generic;

namespace ACDesigner
{
    public class History<T>
    {
        private int m_start;
        private int m_end;
        private int m_curr;
        private int m_size;

        private T[] m_buff;

        private T this[int index]
        {
            get
            {
                if (index < 0)
                {
                    return default(T);
                }

                return m_buff[index % m_size];
            }
            set
            {
                if (index < 0)
                {
                    return;
                }

                m_buff[index % m_size] = value;
            }
        }

        public History(int size)
        {
            m_size = size;
            m_buff = new T[size];
            Reset();
        }

        public void Reset()
        {
            m_start = 0;
            m_end = m_curr = -1;
        }

        public List<T> All()
        {
            var list = new List<T>();
            for (var i = m_start; i <= m_end; i++)
            {
                list.Add(this[i]);
            }

            return list;
        }

        public void Remove(T t)
        {
            while (true)
            {
                if (!Contains(t))
                {
                    return;
                }

                var startIndex = -1;
                for (var i = m_start; i <= m_end; i++)
                {
                    if (this[i].Equals(t))
                    {
                        startIndex = i;
                        break;
                    }
                }

                if (startIndex < m_start)
                {
                    return;
                }

                for (var i = startIndex + 1; i <= m_end; i++)
                {
                    this[i - 1] = this[i];
                }

                if (m_curr > startIndex)
                {
                    --m_curr;
                }

                --m_end;
            }
        }

        public bool Contains(T t)
        {
            var result = false;
            for (var i = m_start; i <= m_end; i++)
            {
                if (this[i].Equals(t))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public T Get(int index)
        {
            if (!IsValid(index))
            {
                return default(T);
            }

            return this[index];
        }

        public T Curr()
        {
            return Get(m_curr);
        }

        public T Last()
        {
            return Get(m_end);
        }

        public T First()
        {
            return Get(m_start);
        }

        public T Prev()
        {
            if (!IsValid(m_curr - 1))
            {
                return default(T);
            }

            return Get(--m_curr);
        }

        public T Next()
        {
            if (!IsValid(m_curr + 1))
            {
                return default(T);
            }

            return Get(++m_curr);
        }

        public T UnDo()
        {
            if (!IsValid(m_curr))
            {
                return default(T);
            }

            var t = this[m_curr];
            m_curr--;
            return t;
        }

        public T ReDo()
        {
            if (!IsValid(m_curr + 1))
            {
                return default(T);
            }

            var t = this[m_curr + 1];
            m_curr++;
            return t;
        }

        public void Do(T t)
        {
            if (m_curr < m_start) Reset();

            m_end = ++m_curr;
            this[m_end] = t;

            if (m_end - m_start >= m_size)
            {
                m_start = m_end - m_size + 1;
            }
        }

        private bool IsValid(int index)
        {
            if (index < m_start || index > m_end)
            {
                return false;
            }

            return true;
        }
    }
}