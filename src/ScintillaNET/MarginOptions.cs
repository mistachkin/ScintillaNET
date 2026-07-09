/*
 * MarginOptions.cs --
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
    /// Flags used to define margin options.
    /// </summary>
    /// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
    [Flags]
    [ObjectId("23fa96f0-cc70-4dc0-82a5-4f75f834adb1")]
    public enum MarginOptions
    {
        /// <summary>
        /// No options. This is the default.
        /// </summary>
        None = NativeMethods.SC_MARGINOPTION_NONE,

        /// <summary>
        /// Lines selected by clicking on the margin will select only the subline of wrapped text.
        /// </summary>
        SublineSelect = NativeMethods.SC_MARGINOPTION_SUBLINESELECT
    }
}
