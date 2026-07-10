/*
 * IndicatorCollection.cs --
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
using System.ComponentModel;

namespace ScintillaNET
{
    /// <summary>
    /// An immutable collection of indicators in a <see cref="Scintilla" />
    /// control.
    /// </summary>
    [ObjectId("c63301f7-e6ef-4ae2-bf13-edf25c007020")]
    public class IndicatorCollection : IEnumerable<Indicator>
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
        /// <see cref="IndicatorCollection" /> class.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this collection.
        /// </param>
        public IndicatorCollection(
            Scintilla scintilla /* in */
            )
        {
            this.scintilla = scintilla;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the number of indicators.
        /// </summary>
        /// <returns>
        /// The number of indicators in the <see cref="IndicatorCollection" />.
        /// </returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public int Count
        {
            get
            {
                return (NativeMethods.INDIC_MAX + 1);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets an <see cref="Indicator" /> object at the specified index.
        /// </summary>
        /// <param name="index">
        /// The indicator index.
        /// </param>
        /// <returns>
        /// An object representing the indicator at the specified
        /// <paramref name="index" />.
        /// </returns>
        /// <remarks>
        /// Indicators 0 through 7 are used by lexers.
        /// Indicators 32 through 35 are used for IME.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public Indicator this[int index]
        {
            get
            {
                index = Helpers.Clamp(index, 0, Count - 1);
                return new Indicator(scintilla, index);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<Indicator> Members
        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An object that contains all <see cref="Indicator" /> objects within
        /// the <see cref="IndicatorCollection" />.
        /// </returns>
        public IEnumerator<Indicator> GetEnumerator()
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
