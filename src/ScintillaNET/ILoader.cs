/*
 * ILoader.cs --
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

namespace ScintillaNET
{
    /// <summary>
    /// Provides methods for loading and creating a <see cref="Document" />
    /// on a background (non-UI) thread.
    /// </summary>
    /// <remarks>
    /// Internally an <see cref="ILoader" /> maintains a
    /// <see cref="Document" /> instance with a reference count of 1.  You are
    /// responsible for ensuring the reference count eventually reaches 0 or
    /// memory leaks will occur.
    /// </remarks>
    [ObjectId("a921fc6e-7bfa-4a72-a415-c16e2565d9d6")]
    public interface ILoader
    {
        #region Public Methods
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
        bool AddData(
            char[] data, /* in */
            int length   /* in */
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the internal document.
        /// </summary>
        /// <returns>
        /// A <see cref="Document" /> containing the added text.  The document
        /// has a reference count of 1.
        /// </returns>
        Document ConvertToDocument();

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
        int Release();
        #endregion
    }
}
