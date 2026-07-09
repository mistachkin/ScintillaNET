/*
 * AutomaticFold.cs --
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
    /// Configuration options for automatic code folding.
    /// </summary>
    /// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
    [Flags]
    [ObjectId("c9690f3d-0505-424c-9a01-38c5a9a18a76")]
    public enum AutomaticFold
    {
        /// <summary>
        /// Automatic folding is disabled. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Automatically show lines as needed. The <see cref="Scintilla.NeedShown" /> event is not raised when this value is used.
        /// </summary>
        Show = NativeMethods.SC_AUTOMATICFOLD_SHOW,

        /// <summary>
        /// Handle clicks in fold margin automatically. The <see cref="Scintilla.MarginClick" /> event is not raised for folding margins when this value is used.
        /// </summary>
        Click = NativeMethods.SC_AUTOMATICFOLD_CLICK,

        /// <summary>
        /// Show lines as needed when the fold structure is changed.
        /// </summary>
        Change = NativeMethods.SC_AUTOMATICFOLD_CHANGE
    }
}
