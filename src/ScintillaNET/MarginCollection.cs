/*
 * MarginCollection.cs --
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
using System.ComponentModel;

namespace ScintillaNET
{
    /// <summary>
    /// An immutable collection of margins in a <see cref="Scintilla" />
    /// control.
    /// </summary>
    [ObjectId("a5109cb6-5290-4cdd-9250-1bab6e2eea62")]
    public class MarginCollection : IEnumerable<Margin>
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
        /// Initializes a new instance of the <see cref="MarginCollection" />
        /// class.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this collection.
        /// </param>
        public MarginCollection(
            Scintilla scintilla /* in */
            )
        {
            this.scintilla = scintilla;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets or sets the number of margins in the
        /// <see cref="MarginCollection" />.
        /// </summary>
        /// <returns>
        /// The number of margins in the collection. The default is 5.
        /// </returns>
        [DefaultValue(NativeMethods.SC_MAX_MARGIN + 1)]
        [Description("The maximum number of margins.")]
        public int Capacity
        {
            get
            {
                return scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINS).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);
                scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINS, new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number of margins in the <see cref="MarginCollection" />.
        /// </summary>
        /// <returns>
        /// The number of margins in the collection.
        /// </returns>
        /// <remarks>
        /// This property is kept for convenience. The return value will always
        /// be equal to <see cref="Capacity" />.
        /// </remarks>
        /// <seealso cref="Capacity" />
        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public int Count
        {
            get
            {
                return Capacity;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the width in pixels of the left margin padding.
        /// </summary>
        /// <returns>
        /// The left margin padding measured in pixels. The default is 1.
        /// </returns>
        [DefaultValue(1)]
        [Description("The left margin padding in pixels.")]
        public int Left
        {
            get
            {
                return scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINLEFT).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);
                scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINLEFT, IntPtr.Zero,
                    new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the width in pixels of the right margin padding.
        /// </summary>
        /// <returns>
        /// The right margin padding measured in pixels. The default is 1.
        /// </returns>
        [DefaultValue(1)]
        [Description("The right margin padding in pixels.")]
        public int Right
        {
            get
            {
                return scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINRIGHT).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);
                scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINRIGHT, IntPtr.Zero,
                    new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a <see cref="Margin" /> object at the specified index.
        /// </summary>
        /// <param name="index">
        /// The margin index.
        /// </param>
        /// <returns>
        /// An object representing the margin at the specified
        /// <paramref name="index" />.
        /// </returns>
        /// <remarks>
        /// By convention margin 0 is used for line numbers and the two
        /// following for symbols.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public Margin this[int index]
        {
            get
            {
                index = Helpers.Clamp(index, 0, Math.Max(0, Count - 1));
                return new Margin(scintilla, index);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Removes all text displayed in every <see cref="MarginType.Text" />
        /// and <see cref="MarginType.RightText" /> margins.
        /// </summary>
        public void ClearAllText()
        {
            scintilla.DirectMessage(NativeMethods.SCI_MARGINTEXTCLEARALL);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<Margin> Members
        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An object that contains all <see cref="Margin" /> objects within
        /// the <see cref="MarginCollection" />.
        /// </returns>
        public IEnumerator<Margin> GetEnumerator()
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
