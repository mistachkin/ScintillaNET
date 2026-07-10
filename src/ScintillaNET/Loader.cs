/*
 * Loader.cs --
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
using System.Runtime.InteropServices;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Provides the default <see cref="ILoader" /> implementation, which
    /// loads text into a native Scintilla document by invoking the methods
    /// of the native ILoader virtual method table.
    /// </summary>
    [ObjectId("1e129e4b-46d2-4a2f-9735-e495daaf875f")]
    internal sealed class Loader : ILoader
    {
        #region Private Data
        /// <summary>
        /// A pointer to the native ILoader instance.
        /// </summary>
        private readonly IntPtr self;

        /// <summary>
        /// The 32-bit view of the native ILoader virtual method table, used
        /// when the process is running as 32-bit.
        /// </summary>
        private readonly NativeMethods.ILoaderVTable32 loader32;

        /// <summary>
        /// The 64-bit view of the native ILoader virtual method table, used
        /// when the process is running as 64-bit.
        /// </summary>
        private readonly NativeMethods.ILoaderVTable64 loader64;

        /// <summary>
        /// The encoding used to convert characters to bytes when adding data
        /// to the document.
        /// </summary>
        private readonly Encoding encoding;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified native
        /// loader pointer and text encoding.
        /// </summary>
        /// <param name="ptr">
        /// A pointer to the native ILoader instance.
        /// </param>
        /// <param name="encoding">
        /// The encoding used to convert characters to bytes when adding data
        /// to the document.
        /// </param>
        public unsafe Loader(
            IntPtr ptr,        /* in */
            Encoding encoding  /* in */
            )
        {
            this.self = ptr;
            this.encoding = encoding;

            //
            // NOTE: http://stackoverflow.com/a/985820/2073621
            //       http://stackoverflow.com/a/2094715/2073621
            //       http://en.wikipedia.org/wiki/Virtual_method_table
            //       http://www.openrce.org/articles/full_view/23
            //
            //       Because I know that I'm not going to remember all this...
            //       In C++, the first variable of an object is a pointer
            //       (v[f]ptr) to the virtual table (v[f]table) containing the
            //       addresses of each function.  The first call below gets
            //       the vtable address by following the object ptr to the vptr
            //       to the vtable.  The second call casts the vtable to a
            //       structure with the same memory layout so we can easily
            //       invoke each function without having to do any pointer
            //       arithmetic.  Depending on the architecture, the function
            //       calling conventions can be different.
            //

            IntPtr vfptr = *(IntPtr*)ptr;

            if (IntPtr.Size == 4)
                loader32 =
                    (NativeMethods.ILoaderVTable32)Marshal.PtrToStructure(
                        vfptr, typeof(NativeMethods.ILoaderVTable32));
            else
                loader64 =
                    (NativeMethods.ILoaderVTable64)Marshal.PtrToStructure(
                        vfptr, typeof(NativeMethods.ILoaderVTable64));
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ILoader Members
        /// <summary>
        /// Adds the data specified to the internal document.
        /// </summary>
        /// <param name="data">
        /// The character buffer to copy to the new document.
        /// </param>
        /// <param name="length">
        /// The number of characters in <paramref name="data" /> to copy.
        /// </param>
        /// <returns>
        /// true if the data was added successfully; otherwise, false.  A
        /// return value of false should be followed by a call to
        /// <see cref="Release" />.
        /// </returns>
        public unsafe bool AddData(
            char[] data, /* in */
            int length   /* in */
            )
        {
            if (data != null)
            {
                length = Helpers.Clamp(length, 0, data.Length);

                byte[] bytes = Helpers.GetBytes(
                    data, length, encoding, false);

                fixed (byte* bp = bytes)
                {
                    int status = (IntPtr.Size == 4 ?
                        loader32.AddData(self, bp, new IntPtr(bytes.Length)) :
                        loader64.AddData(self, bp, new IntPtr(bytes.Length)));

                    if (status != NativeMethods.SC_STATUS_OK)
                        return false;
                }
            }

            return true;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the internal document.
        /// </summary>
        /// <returns>
        /// A <see cref="Document" /> containing the added text.  The document
        /// has a reference count of 1.
        /// </returns>
        public Document ConvertToDocument()
        {
            IntPtr ptr = (IntPtr.Size == 4 ?
                loader32.ConvertToDocument(self) :
                loader64.ConvertToDocument(self));

            Document document = new Document(ptr);

            return document;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called to release the internal document when an error occurs using
        /// <see cref="AddData" /> or to abandon loading.
        /// </summary>
        /// <returns>
        /// The internal document reference count.  A return value of 0
        /// indicates that the document has been destroyed and all associated
        /// memory released.
        /// </returns>
        public int Release()
        {
            int count = (IntPtr.Size == 4 ?
                loader32.Release(self) : loader64.Release(self));

            return count;
        }
        #endregion
    }
}
