/*
 * UpdateChange.cs --
 *
 * Copyright (c) 2017 Jacob Slusser, https://github.com/jacobslusser
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
    /// Specifies the change that triggered a <see cref="Scintilla.UpdateUI" /> event.
    /// </summary>
    /// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
    [Flags]
    [ObjectId("3eeaf4a1-9dc6-48e0-8f2c-9f696f271741")]
    public enum UpdateChange
    {
        /// <summary>
        /// Contents, styling or markers have been changed.
        /// </summary>
        Content = NativeMethods.SC_UPDATE_CONTENT,

        /// <summary>
        /// Selection has been changed.
        /// </summary>
        Selection = NativeMethods.SC_UPDATE_SELECTION,

        /// <summary>
        /// Scrolled vertically.
        /// </summary>
        VScroll = NativeMethods.SC_UPDATE_V_SCROLL,

        /// <summary>
        /// Scrolled horizontally.
        /// </summary>
        HScroll = NativeMethods.SC_UPDATE_H_SCROLL
    }
}
