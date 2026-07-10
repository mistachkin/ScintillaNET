/*
 * GapBuffer.cs --
 *
 * Copyright (c) 2017 Jacob Slusser, https://github.com/jacobslusser
 * Copyright (c) 2019-2026 by Joe Mistachkin.  All rights reserved.
 *
 * This file is part of ScintillaNET, which is distributed under the MIT
 * License; see the file "LICENSE" for full terms and a DISCLAIMER OF ALL
 * WARRANTIES.
 *
 * RCS: @(#) $Id: $
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ScintillaNET
{
    // Do error checking higher up
    // http://www.codeproject.com/Articles/20910/Generic-Gap-Buffer
    /// <summary>
    /// Implements a generic gap buffer, a dynamic array optimized for runs of
    /// insertions and removals clustered near a common location.  A movable
    /// "gap" of unused elements is kept within the backing array so that edits
    /// near the gap require little or no copying.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [ObjectId("34da7b8c-632d-4367-87f4-3885bf67df85")]
    internal sealed class GapBuffer<T> : IEnumerable<T>
    {
        #region Private Data
        /// <summary>
        /// The backing array holding the buffered elements together with the
        /// unused gap region.
        /// </summary>
        private T[] buffer;

        /// <summary>
        /// The index of the first element of the gap within the backing
        /// array.
        /// </summary>
        private int gapStart;

        /// <summary>
        /// The index just past the last element of the gap within the backing
        /// array.
        /// </summary>
        private int gapEnd;
        #endregion

#if DEBUG
        ///////////////////////////////////////////////////////////////////////

        #region Private Properties
        //
        // HACK: Poor man's DebuggerTypeProxy because I can't seem to get that
        //       working.
        //
        /// <summary>
        /// Gets a snapshot of the buffered elements, in order, for display in
        /// the debugger.
        /// </summary>
        private List<T> Debug
        {
            get
            {
                List<T> list = new List<T>(this);
                return list;
            }
        }
        #endregion
#endif

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class with the default (zero)
        /// capacity.
        /// </summary>
        public GapBuffer()
            : this(0)
        {
            // do nothing.
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class with the specified initial
        /// capacity.
        /// </summary>
        /// <param name="capacity">
        /// The initial number of elements the backing array can hold before it
        /// must be resized.
        /// </param>
        public GapBuffer(
            int capacity /* in */
            )
        {
            this.buffer = new T[capacity];
            this.gapEnd = buffer.Length;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the number of elements contained in the buffer.
        /// </summary>
        public int Count
        {
            get
            {
                return buffer.Length - (gapEnd - gapStart);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get or set.
        /// </param>
        public T this[
            int index /* in */
            ]
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
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Adds an item to the end of the buffer.
        /// </summary>
        /// <param name="item">
        /// The item to append to the buffer.
        /// </param>
        public void Add(
            T item /* in */
            )
        {
            Insert(Count, item);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds the items of the specified collection to the end of the
        /// buffer.
        /// </summary>
        /// <param name="collection">
        /// The collection whose items are appended to the buffer.
        /// </param>
        public void AddRange(
            ICollection<T> collection /* in */
            )
        {
            InsertRange(Count, collection);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inserts an item into the buffer at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the item is inserted.
        /// </param>
        /// <param name="item">
        /// The item to insert into the buffer.
        /// </param>
        public void Insert(
            int index, /* in */
            T item     /* in */
            )
        {
            PlaceGapStart(index);
            EnsureGapCapacity(1);

            buffer[index] = item;
            gapStart++;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inserts the items of the specified collection into the buffer at
        /// the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the items are inserted.
        /// </param>
        /// <param name="collection">
        /// The collection whose items are inserted into the buffer.
        /// </param>
        public void InsertRange(
            int index,                /* in */
            ICollection<T> collection /* in */
            )
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

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Removes the element at the specified index from the buffer.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        public void RemoveAt(
            int index /* in */
            )
        {
            PlaceGapStart(index);
            buffer[gapEnd] = default(T);
            gapEnd++;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Removes a range of elements from the buffer.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the first element to remove.
        /// </param>
        /// <param name="count">
        /// The number of elements to remove.
        /// </param>
        public void RemoveRange(
            int index, /* in */
            int count  /* in */
            )
        {
            if (count > 0)
            {
                PlaceGapStart(index);
                Array.Clear(buffer, gapEnd, count);
                gapEnd += count;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Ensures the gap can accommodate at least the specified number of
        /// additional elements, growing the backing array if necessary.
        /// </summary>
        /// <param name="length">
        /// The minimum number of unused elements the gap must be able to
        /// hold.
        /// </param>
        private void EnsureGapCapacity(
            int length /* in */
            )
        {
            if (length > (gapEnd - gapStart))
            {
                // How much to grow the buffer is a tricky question. Our
                // current algo will double the capacity unless that's not
                // enough.
                int minCapacity = Count + length;
                int newCapacity = (int)Math.Min(
                    (long)buffer.Length * 2, int.MaxValue);
                if (newCapacity < minCapacity)
                {
                    newCapacity = minCapacity;
                }

                T[] newBuffer = new T[newCapacity];
                int newGapEnd = newBuffer.Length - (buffer.Length - gapEnd);

                Array.Copy(buffer, 0, newBuffer, 0, gapStart);
                Array.Copy(
                    buffer, gapEnd, newBuffer, newGapEnd,
                    newBuffer.Length - newGapEnd);
                this.buffer = newBuffer;
                this.gapEnd = newGapEnd;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Repositions the gap so that it begins at the specified index,
        /// moving the surrounding elements as needed.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the gap should begin.
        /// </param>
        private void PlaceGapStart(
            int index /* in */
            )
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
                    int deltaLength = (gapEnd - gapStart < length
                        ? gapEnd - gapStart : length);
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
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<T> Members
        /// <summary>
        /// Returns an enumerator that iterates through the buffered elements
        /// in order.
        /// </summary>
        /// <returns>
        /// An enumerator for the buffered elements.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                yield return this[i];

            yield break;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a non-generic enumerator that iterates through the buffered
        /// elements in order.
        /// </summary>
        /// <returns>
        /// A non-generic enumerator for the buffered elements.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
