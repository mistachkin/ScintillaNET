/*
 * WrapVisualFlagLocation.cs --
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

namespace ScintillaNET
{
    /// <summary>
    /// Additional location options for line wrapping visual indicators.
    /// </summary>
    [ObjectId("7cae0c95-fb0d-40ee-b056-32fc9f609ce2")]
    public enum WrapVisualFlagLocation
    {
        /// <summary>
        /// Wrap indicators are drawn near the border. This is the default.
        /// </summary>
        Default = NativeMethods.SC_WRAPVISUALFLAGLOC_DEFAULT,

        /// <summary>
        /// Wrap indicators are drawn at the end of sublines near the text.
        /// </summary>
        EndByText = NativeMethods.SC_WRAPVISUALFLAGLOC_END_BY_TEXT,

        /// <summary>
        /// Wrap indicators are drawn at the beginning of sublines near the text.
        /// </summary>
        StartByText = NativeMethods.SC_WRAPVISUALFLAGLOC_START_BY_TEXT
    }
}
