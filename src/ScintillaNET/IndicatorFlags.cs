/*
 * IndicatorFlags.cs --
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
    /// Flags associated with a <see cref="Indicator" />.
    /// </summary>
    /// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
    [Flags]
    [ObjectId("3cb3314f-8b27-4fc4-81ed-9844a3068092")]
    public enum IndicatorFlags
    {
        /// <summary>
        /// No flags. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// When set, will treat an indicator value as a RGB color that has been OR'd with <see cref="Indicator.ValueBit" />
        /// and will use that instead of the value specified in the <see cref="Indicator.ForeColor" /> property. This allows
        /// an indicator to display more than one color.
        /// </summary>
        ValueFore = NativeMethods.SC_INDICFLAG_VALUEFORE
    }
}
