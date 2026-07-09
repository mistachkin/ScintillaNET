using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ScintillaNET
{
    // Do error checking higher up
    // http://www.codeproject.com/Articles/20910/Generic-Gap-Buffer
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class GapBuffer<T> : IEnumerable<T>
    {
        private T[] buffer;
        private int gapStart;
        private int gapEnd;

        public void Add(T item)
        {
            Insert(Count, item);
        }

        public void AddRange(ICollection<T> collection)
        {
            InsertRange(Count, collection);
        }

        private void EnsureGapCapacity(int length)
        {
            if (length > (gapEnd - gapStart))
            {
                // How much to grow the buffer is a tricky question.
                // Our current algo will double the capacity unless that's not enough.
                int minCapacity = Count + length;
                int newCapacity = (int)Math.Min((long)buffer.Length * 2, int.MaxValue);
                if (newCapacity < minCapacity)
                {
                    newCapacity = minCapacity;
                }

                T[] newBuffer = new T[newCapacity];
                int newGapEnd = newBuffer.Length - (buffer.Length - gapEnd);

                Array.Copy(buffer, 0, newBuffer, 0, gapStart);
                Array.Copy(buffer, gapEnd, newBuffer, newGapEnd, newBuffer.Length - newGapEnd);
                this.buffer = newBuffer;
                this.gapEnd = newGapEnd;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                yield return this[i];

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Insert(int index, T item)
        {
            PlaceGapStart(index);
            EnsureGapCapacity(1);

            buffer[index] = item;
            gapStart++;
        }

        public void InsertRange(int index, ICollection<T> collection)
        {
            int count = collection.Count;
            if (count > 0)
            {
                PlaceGapStart(index);
                EnsureGapCapacity(count);

                collection.CopyTo(buffer, gapStart);
                gapStart += count;
            }
        }

        private void PlaceGapStart(int index)
        {
            if (index != gapStart)
            {
                if ((gapEnd - gapStart) == 0)
                {
                    // There is no gap
                    gapStart = index;
                    gapEnd = index;
                }
                else if (index < gapStart)
                {
                    // Move gap left (copy contents right)
                    int length = (gapStart - index);
                    int deltaLength = (gapEnd - gapStart < length ? gapEnd - gapStart : length);
                    Array.Copy(buffer, index, buffer, gapEnd - length, length);
                    gapStart -= length;
                    gapEnd -= length;

                    Array.Clear(buffer, index, deltaLength);
                }
                else
                {
                    // Move gap right (copy contents left)
                    int length = (index - gapStart);
                    int deltaIndex = (index > gapEnd ? index : gapEnd);
                    Array.Copy(buffer, gapEnd, buffer, gapStart, length);
                    gapStart += length;
                    gapEnd += length;

                    Array.Clear(buffer, deltaIndex, gapEnd - deltaIndex);
                }
            }
        }

        public void RemoveAt(int index)
        {
            PlaceGapStart(index);
            buffer[gapEnd] = default(T);
            gapEnd++;
        }

        public void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                PlaceGapStart(index);
                Array.Clear(buffer, gapEnd, count);
                gapEnd += count;
            }
        }

        public int Count
        {
            get
            {
                return buffer.Length - (gapEnd - gapStart);
            }
        }

#if DEBUG
        // Poor man's DebuggerTypeProxy because I can't seem to get that working
        private List<T> Debug
        {
            get
            {
                List<T> list = new List<T>(this);
                return list;
            }
        }
#endif

        public T this[int index]
        {
            get
            {
                if (index < gapStart)
                    return buffer[index];

                return buffer[index + (gapEnd - gapStart)];
            }
            set
            {
                if (index >= gapStart)
                    index += (gapEnd - gapStart);

                buffer[index] = value;
            }
        }

        public GapBuffer() : this(0) { }

        public GapBuffer(int capacity)
        {
            this.buffer = new T[capacity];
            this.gapEnd = buffer.Length;
        }
    }
}
