/*
 * FoldLevelFlags.cs --
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
    /// Flags for additional line fold level behavior.
    /// </summary>
    [Flags]
    [ObjectId("9559961f-7adc-44d3-aa89-913b543ef3d4")]
    public enum FoldLevelFlags
    {
        /// <summary>
        /// Indicates that the line is blank and should be treated slightly different than its level may indicate;
        /// otherwise, blank lines should generally not be fold points.
        /// </summary>
        White = NativeMethods.SC_FOLDLEVELWHITEFLAG,

        /// <summary>
        /// Indicates that the line is a header (fold point).
        /// </summary>
        Header = NativeMethods.SC_FOLDLEVELHEADERFLAG
    }
}
