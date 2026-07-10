/*
 * MarkerCollection.cs --
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
    /// An immutable collection of markers in a <see cref="Scintilla" />
    /// control.
    /// </summary>
    [ObjectId("d42651d1-caf1-401c-a30e-6356b20f69e5")]
    public class MarkerCollection : IEnumerable<Marker>
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
        /// Initializes a new instance of the <see cref="MarkerCollection" />
        /// class.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this collection.
        /// </param>
        public MarkerCollection(
            Scintilla scintilla /* in */
            )
        {
            this.scintilla = scintilla;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the number of markers in the <see cref="MarkerCollection" />.
        /// </summary>
        /// <returns>
        /// This property always returns 32.
        /// </returns>
        public int Count
        {
            get
            {
                return (NativeMethods.MARKER_MAX + 1);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a <see cref="Marker" /> object at the specified index.
        /// </summary>
        /// <param name="index">
        /// The marker index.
        /// </param>
        /// <returns>
        /// An object representing the marker at the specified
        /// <paramref name="index" />.
        /// </returns>
        /// <remarks>
        /// Markers 25 through 31 are used by Scintilla for folding.
        /// </remarks>
        public Marker this[int index]
        {
            get
            {
                index = Helpers.Clamp(index, 0, Count - 1);
                return new Marker(scintilla, index);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<Marker> Members
        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An object for enumerating all <see cref="Marker" /> objects within
        /// the <see cref="MarkerCollection" />.
        /// </returns>
        public IEnumerator<Marker> GetEnumerator()
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
