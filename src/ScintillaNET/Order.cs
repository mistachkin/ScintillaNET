/*
 * Order.cs --
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
    /// The sorting order for autocompletion lists.
    /// </summary>
    [ObjectId("80f8c843-994d-4fc2-a981-0b3910af9887")]
    public enum Order
    {
        /// <summary>
        /// Requires that an autocompletion lists be sorted in alphabetical order. This is the default.
        /// </summary>
        Presorted = NativeMethods.SC_ORDER_PRESORTED,

        /// <summary>
        /// Instructs a <see cref="Scintilla" /> control to perform an alphabetical sort of autocompletion lists.
        /// </summary>
        PerformSort = NativeMethods.SC_ORDER_PERFORMSORT,

        /// <summary>
        /// User-defined order.
        /// </summary>
        Custom = NativeMethods.SC_ORDER_CUSTOM
    }
}
