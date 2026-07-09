/*
 * ListCompletionMethod.cs --
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
    /// Indicates how an autocompletion occurred.
    /// </summary>
    [ObjectId("2207a57e-2e63-459b-b5ab-8b6e1ed1d932")]
    public enum ListCompletionMethod
    {
        /// <summary>
        /// A fillup character (see <see cref="Scintilla.AutoCSetFillUps" />) triggered the completion.
        /// The character used is indicated by the <see cref="AutoCSelectionEventArgs.Char" /> property.
        /// </summary>
        FillUp = NativeMethods.SC_AC_FILLUP,

        /// <summary>
        /// A double-click triggered the completion.
        /// </summary>
        DoubleClick = NativeMethods.SC_AC_DOUBLECLICK,

        /// <summary>
        /// A tab key or the <see cref="ScintillaNET.Command.Tab" /> command triggered the completion.
        /// </summary>
        Tab = NativeMethods.SC_AC_TAB,

        /// <summary>
        /// A new line or <see cref="ScintillaNET.Command.NewLine" /> command triggered the completion.
        /// </summary>
        NewLine = NativeMethods.SC_AC_NEWLINE,

        /// <summary>
        /// The <see cref="Scintilla.AutoCSelect" /> method triggered the completion.
        /// </summary>
        Command = NativeMethods.SC_AC_COMMAND
    }
}
