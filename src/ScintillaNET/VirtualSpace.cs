/*
 * VirtualSpace.cs --
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
    /// Enables virtual space for rectangular selections or in other circumstances or in both.
    /// </summary>
    /// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
    [Flags]
    [ObjectId("15c875ec-ef4f-4f18-a25e-ff4d3a57231b")]
    public enum VirtualSpace
    {
        /// <summary>
        /// Virtual space is not enabled. This is the default.
        /// </summary>
        None = NativeMethods.SCVS_NONE,

        /// <summary>
        /// Virtual space is enabled for rectangular selections.
        /// </summary>
        RectangularSelection = NativeMethods.SCVS_RECTANGULARSELECTION,

        /// <summary>
        /// Virtual space is user accessible.
        /// </summary>
        UserAccessible = NativeMethods.SCVS_USERACCESSIBLE,

        /// <summary>
        /// Prevents left arrow movement and selection from wrapping to the previous line.
        /// </summary>
        NoWrapLineStart = NativeMethods.SCVS_NOWRAPLINESTART
    }
}
