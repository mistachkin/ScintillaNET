/*
 * SelectionCollection.cs --
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

namespace ScintillaNET
{
    /// <summary>
    /// A multiple selection collection.
    /// </summary>
    [ObjectId("b6d395c0-d67a-49e0-860c-4521c9709cd3")]
    public class SelectionCollection : IEnumerable<Selection>
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that created this collection.
        /// </summary>
        private readonly Scintilla scintilla;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="SelectionCollection" /> class.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this collection.
        /// </param>
        public SelectionCollection(
            Scintilla scintilla /* in */
            )
        {
            this.scintilla = scintilla;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the number of active selections.
        /// </summary>
        /// <returns>
        /// The number of selections in the <see cref="SelectionCollection" />.
        /// </returns>
        public int Count
        {
            get
            {
                return scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONS).ToInt32();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating whether all selection ranges are empty.
        /// </summary>
        /// <returns>
        /// true if all selection ranges are empty; otherwise, false.
        /// </returns>
        public bool IsEmpty
        {
            get
            {
                return scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONEMPTY) != IntPtr.Zero;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the <see cref="Selection" /> at the specified zero-based
        /// index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the <see cref="Selection" /> to get.
        /// </param>
        /// <returns>
        /// The <see cref="Selection" /> at the specified index.
        /// </returns>
        public Selection this[int index]
        {
            get
            {
                index = Helpers.Clamp(index, 0, Count - 1);
                return new Selection(scintilla, index);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<Selection> Members
        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An object that contains all <see cref="Selection" /> objects within
        /// the <see cref="SelectionCollection" />.
        /// </returns>
        public IEnumerator<Selection> GetEnumerator()
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                yield return this[i];

            yield break;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable Members
        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
