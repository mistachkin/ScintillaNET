/*
 * NativeMemoryStream.cs --
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
using System.IO;
using System.Runtime.InteropServices;

namespace ScintillaNET
{
    /// <summary>
    /// Implements a growable stream over a block of unmanaged memory, similar
    /// to an <see cref="System.IO.UnmanagedMemoryStream" /> except that it can
    /// grow as data is written to it.
    /// </summary>
    [ObjectId("aab415a5-cc15-426d-b6bf-642e9e61d6c4")]
    internal sealed unsafe class NativeMemoryStream : Stream
    {
        #region Private Data
        /// <summary>
        /// A pointer to the block of unmanaged memory backing this stream.
        /// </summary>
        private IntPtr ptr;

        /// <summary>
        /// The size, in bytes, of the allocated unmanaged memory block.
        /// </summary>
        private long capacity;

        /// <summary>
        /// The current zero-based byte position within the stream.
        /// </summary>
        private long position;

        /// <summary>
        /// The number of bytes that have been written to the stream.
        /// </summary>
        private long length;

        /// <summary>
        /// When true, the unmanaged memory block is freed when this stream is
        /// disposed of.
        /// </summary>
        private bool freeOnDispose;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified initial
        /// capacity, in bytes.
        /// </summary>
        /// <param name="capacity">
        /// The initial size, in bytes, of the unmanaged memory block to
        /// allocate.  A minimum of four bytes is always allocated.
        /// </param>
        public NativeMemoryStream(
            int capacity /* in */
            )
        {
            if (capacity < 4)
                capacity = 4;

            this.capacity = capacity;
            this.ptr = Marshal.AllocHGlobal(capacity);
            FreeOnDispose = true;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets or sets a value indicating whether the unmanaged memory block
        /// is freed when this stream is disposed of.
        /// </summary>
        public bool FreeOnDispose
        {
            get { return this.freeOnDispose; }
            set { this.freeOnDispose = value; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a pointer to the block of unmanaged memory backing this
        /// stream.
        /// </summary>
        public IntPtr Pointer
        {
            get
            {
                return ptr;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Stream Members
        /// <summary>
        /// Gets a value indicating whether the current stream supports
        /// reading.
        /// </summary>
        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating whether the current stream supports
        /// seeking.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating whether the current stream supports
        /// writing.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the length, in bytes, of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return length;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clears all buffers for this stream; this stream buffers nothing in
        /// managed memory, so this method does nothing.
        /// </summary>
        public override void Flush()
        {
            // NOP
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reads a sequence of bytes from the current stream; this operation
        /// is not supported.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to receive the bytes read from the stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the
        /// bytes read.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to read.
        /// </param>
        /// <returns>
        /// The total number of bytes read into the buffer.
        /// </returns>
        /// <exception cref="System.NotImplementedException">
        /// Always thrown, as reading is not supported by this stream.
        /// </exception>
        public override int Read(
            byte[] buffer, /* in */
            int offset,    /* in */
            int count      /* in */
            )
        {
            throw new NotImplementedException();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the position within the current stream; only seeking relative
        /// to the beginning of the stream is supported.
        /// </summary>
        /// <param name="offset">
        /// The byte offset relative to origin.
        /// </param>
        /// <param name="origin">
        /// A value indicating the reference point used to obtain the new
        /// position.  Only <see cref="System.IO.SeekOrigin.Begin" /> is
        /// supported.
        /// </param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.NotImplementedException">
        /// Thrown when origin is any value other than
        /// <see cref="System.IO.SeekOrigin.Begin" />.
        /// </exception>
        public override long Seek(
            long offset,      /* in */
            SeekOrigin origin /* in */
            )
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;

                default:
                    throw new NotImplementedException();
            }

            return position;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the length of the current stream; this operation is not
        /// supported.
        /// </summary>
        /// <param name="value">
        /// The desired length, in bytes, of the current stream.
        /// </param>
        /// <exception cref="System.NotImplementedException">
        /// Always thrown, as setting the length is not supported by this
        /// stream.
        /// </exception>
        public override void SetLength(
            long value /* in */
            )
        {
            throw new NotImplementedException();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Writes a sequence of bytes to the current stream, growing the
        /// unmanaged memory block as needed, and advances the position within
        /// the stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        /// The buffer containing the bytes to write to the stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying
        /// bytes to the stream.
        /// </param>
        /// <param name="count">
        /// The number of bytes to write to the stream.
        /// </param>
        public override void Write(
            byte[] buffer, /* in */
            int offset,    /* in */
            int count      /* in */
            )
        {
            if ((position + count) > capacity)
            {
                // Realloc buffer
                // Grow with 64-bit intermediates so the capacity math cannot
                // overflow to a negative size (which AllocHGlobal would treat
                // as a huge SIZE_T, i.e. either OOM or an under-sized buffer
                // followed by an OOB write).
                int minCapacity = (int)Math.Min(
                    position + count, int.MaxValue);
                int newCapacity = (int)Math.Min(capacity * 2, int.MaxValue);
                if (newCapacity < minCapacity)
                    newCapacity = minCapacity;

                IntPtr newPtr = Marshal.AllocHGlobal(newCapacity);
                NativeMethods.MoveMemory(
                    newPtr, ptr, new UIntPtr((ulong)length));
                Marshal.FreeHGlobal(ptr);

                ptr = newPtr;
                capacity = newCapacity;
            }

            Marshal.Copy(
                buffer, offset, (IntPtr)((long)ptr + position), count);
            position += count;
            length = Math.Max(length, position);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Releases the unmanaged resources used by this stream and,
        /// optionally, the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to
        /// release only unmanaged resources.
        /// </param>
        protected override void Dispose(
            bool disposing /* in */
            )
        {
            if (FreeOnDispose && ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
