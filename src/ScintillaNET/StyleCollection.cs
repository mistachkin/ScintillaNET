/*
 * StyleCollection.cs --
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

using System.Collections;
using System.Collections.Generic;

namespace ScintillaNET
{
    /// <summary>
    /// An immutable collection of style definitions in a
    /// <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("cefa1aa9-1d80-4a70-b14a-cfc107875fea")]
    public class StyleCollection : IEnumerable<Style>
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
        /// Initializes a new instance of the <see cref="StyleCollection" />
        /// class.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this collection.
        /// </param>
        public StyleCollection(
            Scintilla scintilla /* in */
            )
        {
            this.scintilla = scintilla;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the number of styles.
        /// </summary>
        /// <returns>
        /// The number of styles in the <see cref="StyleCollection" />.
        /// </returns>
        public int Count
        {
            get
            {
                return (NativeMethods.STYLE_MAX + 1);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a <see cref="Style" /> object at the specified index.
        /// </summary>
        /// <param name="index">
        /// The style definition index.
        /// </param>
        /// <returns>
        /// An object representing the style definition at the specified
        /// <paramref name="index" />.
        /// </returns>
        /// <remarks>
        /// Styles 32 through 39 have special significance.
        /// </remarks>
        public Style this[int index]
        {
            get
            {
                index = Helpers.Clamp(index, 0, Count - 1);
                return new Style(scintilla, index);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<Style> Members
        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An object that contains all <see cref="Style" /> objects within the
        /// <see cref="StyleCollection" />.
        /// </returns>
        public IEnumerator<Style> GetEnumerator()
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
