/*
 * SCNotificationEventArgs.cs --
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
    /// Provides the data associated with a native Scintilla notification.
    /// This class is intended for internal use only.
    /// </summary>
    [ObjectId("f837836b-7cab-49ba-9793-b4ee22691fdc")]
    internal sealed class SCNotificationEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The native Scintilla notification data.
        /// </summary>
        private NativeMethods.SCNotification scn;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// notification.
        /// </summary>
        /// <param name="scn">
        /// The native Scintilla notification data.
        /// </param>
        public SCNotificationEventArgs(
            NativeMethods.SCNotification scn /* in */
            )
        {
            this.scn = scn;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the native Scintilla notification data.
        /// </summary>
        public NativeMethods.SCNotification SCNotification
        {
            get { return this.scn; }
        }
        #endregion
    }
}
