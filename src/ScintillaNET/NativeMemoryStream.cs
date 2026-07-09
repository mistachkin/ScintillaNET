using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Like an UnmanagedMemoryStream execpt it can grow.
    /// </summary>
    internal sealed unsafe class NativeMemoryStream : Stream
    {
        #region Fields

        private IntPtr ptr;
        private long capacity;
        private long position;
        private long length;

        #endregion Fields

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (FreeOnDispose && ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
            }

            base.Dispose(disposing);
        }

        public override void Flush()
        {
            // NOP
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
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

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if ((position + count) > capacity)
            {
                // Realloc buffer
                // Grow with 64-bit intermediates so the capacity math cannot overflow
                // to a negative size (which AllocHGlobal would treat as a huge SIZE_T,
                // i.e. either OOM or an under-sized buffer followed by an OOB write).
                int minCapacity = (int)Math.Min(position + count, int.MaxValue);
                int newCapacity = (int)Math.Min(capacity * 2, int.MaxValue);
                if (newCapacity < minCapacity)
                    newCapacity = minCapacity;

                IntPtr newPtr = Marshal.AllocHGlobal(newCapacity);
                NativeMethods.MoveMemory(newPtr, ptr, new UIntPtr((ulong)length));
                Marshal.FreeHGlobal(ptr);

                ptr = newPtr;
                capacity = newCapacity;
            }

            Marshal.Copy(buffer, offset, (IntPtr)((long)ptr + position), count);
            position += count;
            length = Math.Max(length, position);
        }

        #endregion Methods

        #region Properties

        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        private bool freeOnDispose;

        public bool FreeOnDispose
        {
            get { return this.freeOnDispose; }
            set { this.freeOnDispose = value; }
        }

        public override long Length
        {
            get
            {
                return length;
            }
        }

        public IntPtr Pointer
        {
            get
            {
                return ptr;
            }
        }

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

        #endregion Properties

        #region Constructors

        public NativeMemoryStream(int capacity)
        {
            if (capacity < 4)
                capacity = 4;

            this.capacity = capacity;
            this.ptr = Marshal.AllocHGlobal(capacity);
            FreeOnDispose = true;
        }

        #endregion Constructors
    }
}
