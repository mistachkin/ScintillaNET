/*
 * Lexer.cs --
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
using System.Collections.Generic;

namespace ScintillaNET
{
    /// <summary>
    /// Specifies the lexer to use for syntax highlighting in a
    /// <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("896fab5e-e602-4cf7-bfdc-a01ffb1080d5")]
    public enum Lexer
    {
        /// <summary>
        /// Lexing is performed by the <see cref="Scintilla" /> control
        /// container (host) using the
        /// <see cref="Scintilla.StyleNeeded" /> event.
        /// </summary>
        Container = NativeMethods.SCLEX_CONTAINER,

        /// <summary>
        /// No lexing should be performed.
        /// </summary>
        Null = NativeMethods.SCLEX_NULL,

        /// <summary>
        /// The Ada (95) language lexer.
        /// </summary>
        Ada = NativeMethods.SCLEX_ADA,

        /// <summary>
        /// The assembly language lexer.
        /// </summary>
        Asm = NativeMethods.SCLEX_ASM,

        /// <summary>
        /// The batch file lexer.
        /// </summary>
        Batch = NativeMethods.SCLEX_BATCH,

        /// <summary>
        /// The Clarion language lexer.
        /// </summary>
        Clw = NativeMethods.SCLEX_CLW,

        /// <summary>
        /// The Clarion language (no case) lexer.
        /// </summary>
        ClwNoCase = NativeMethods.SCLEX_CLWNOCASE,

        /// <summary>
        /// The C language family (C++, C, C#, Java, JavaScript, etc...)
        /// lexer.
        /// </summary>
        Cpp = NativeMethods.SCLEX_CPP,

        /// <summary>
        /// The Cascading Style Sheets (CSS, SCSS) lexer.
        /// </summary>
        Css = NativeMethods.SCLEX_CSS,

        /// <summary>
        /// The Fortran language lexer.
        /// </summary>
        Fortran = NativeMethods.SCLEX_FORTRAN,

        /// <summary>
        /// The FreeBASIC language lexer.
        /// </summary>
        FreeBasic = NativeMethods.SCLEX_FREEBASIC,

        /// <summary>
        /// The HyperText Markup Language (HTML) lexer.
        /// </summary>
        Html = NativeMethods.SCLEX_HTML,

        /// <summary>
        /// JavaScript Object Notation (JSON) lexer.
        /// </summary>
        Json = NativeMethods.SCLEX_JSON,

        /// <summary>
        /// The Lisp language lexer.
        /// </summary>
        Lisp = NativeMethods.SCLEX_LISP,

        /// <summary>
        /// The Lua scripting language lexer.
        /// </summary>
        Lua = NativeMethods.SCLEX_LUA,

        /// <summary>
        /// The Pascal language lexer.
        /// </summary>
        Pascal = NativeMethods.SCLEX_PASCAL,

        /// <summary>
        /// The Perl language lexer.
        /// </summary>
        Perl = NativeMethods.SCLEX_PERL,

        /// <summary>
        /// The PHP: Hypertext Preprocessor (PHP) script lexer.
        /// </summary>
        PhpScript = NativeMethods.SCLEX_PHPSCRIPT,

        /// <summary>
        /// PowerShell script lexer.
        /// </summary>
        PowerShell = NativeMethods.SCLEX_POWERSHELL,

        /// <summary>
        /// Properties file (INI) lexer.
        /// </summary>
        Properties = NativeMethods.SCLEX_PROPERTIES,

        /// <summary>
        /// The PureBasic language lexer.
        /// </summary>
        PureBasic = NativeMethods.SCLEX_PUREBASIC,

        /// <summary>
        /// The Python language lexer.
        /// </summary>
        Python = NativeMethods.SCLEX_PYTHON,

        /// <summary>
        /// The Ruby language lexer.
        /// </summary>
        Ruby = NativeMethods.SCLEX_RUBY,

        /// <summary>
        /// The SmallTalk language lexer.
        /// </summary>
        Smalltalk = NativeMethods.SCLEX_SMALLTALK,

        /// <summary>
        /// The Structured Query Language (SQL) lexer.
        /// </summary>
        Sql = NativeMethods.SCLEX_SQL,

        /// <summary>
        /// The Tool Command Language (Tcl) lexer.
        /// </summary>
        Tcl = NativeMethods.SCLEX_TCL,

        /// <summary>
        /// The Visual Basic (VB) lexer.
        /// </summary>
        Vb = NativeMethods.SCLEX_VB,

        /// <summary>
        /// The Visual Basic Script (VBScript) lexer.
        /// </summary>
        VbScript = NativeMethods.SCLEX_VBSCRIPT,

        /// <summary>
        /// The Verilog hardware description language lexer.
        /// </summary>
        Verilog = NativeMethods.SCLEX_VERILOG,

        /// <summary>
        /// The Extensible Markup Language (XML) lexer.
        /// </summary>
        Xml = NativeMethods.SCLEX_XML,

        /// <summary>
        /// The Blitz (Blitz3D, BlitzMax, etc...) variant of Basic lexer.
        /// </summary>
        BlitzBasic = NativeMethods.SCLEX_BLITZBASIC,

        /// <summary>
        /// The Markdown syntax lexer.
        /// </summary>
        Markdown = NativeMethods.SCLEX_MARKDOWN,

        /// <summary>
        /// The R programming language lexer.
        /// </summary>
        R = NativeMethods.SCLEX_R
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Maps the legacy numeric <see cref="Lexer" /> enumeration to the
    /// Lexilla lexer name passed to CreateLexer (Scintilla 5.x installs
    /// lexers by name via Lexilla, not by numeric id).  Names were taken
    /// from the Scintilla 4.4.6 LexerModule registrations; if a lexer ever
    /// fails to load, cross-check against Lexilla's GetLexerName.
    /// <see cref="Lexer.Container" /> has no entry -- container (host-driven)
    /// styling installs no lexer at all.
    /// </summary>
    [ObjectId("ab38c575-1fbc-462b-a4bf-75f1186c8b8c")]
    internal static class LexerNames
    {
        #region Private Static Data
        /// <summary>
        /// Maps each <see cref="Lexer" /> enumeration value to its Lexilla
        /// lexer name.  <see cref="Lexer.Container" /> is intentionally
        /// omitted (container styling installs no lexer at all).
        /// </summary>
        private static readonly Dictionary<Lexer, string> toName =
            new Dictionary<Lexer, string>()
            {
                { Lexer.Null, "null" },
                { Lexer.Ada, "ada" },
                { Lexer.Asm, "asm" },
                { Lexer.Batch, "batch" },
                { Lexer.Clw, "clarion" },
                { Lexer.ClwNoCase, "clarionnocase" },
                { Lexer.Cpp, "cpp" },
                { Lexer.Css, "css" },
                { Lexer.Fortran, "fortran" },
                { Lexer.FreeBasic, "freebasic" },
                { Lexer.Html, "hypertext" },
                { Lexer.Json, "json" },
                { Lexer.Lisp, "lisp" },
                { Lexer.Lua, "lua" },
                { Lexer.Pascal, "pascal" },
                { Lexer.Perl, "perl" },
                { Lexer.PhpScript, "phpscript" },
                { Lexer.PowerShell, "powershell" },
                { Lexer.Properties, "props" },
                { Lexer.PureBasic, "purebasic" },
                { Lexer.Python, "python" },
                { Lexer.Ruby, "ruby" },
                { Lexer.Smalltalk, "smalltalk" },
                { Lexer.Sql, "sql" },
                { Lexer.Tcl, "tcl" },
                { Lexer.Vb, "vb" },
                { Lexer.VbScript, "vbscript" },
                { Lexer.Verilog, "verilog" },
                { Lexer.Xml, "xml" },
                { Lexer.BlitzBasic, "blitzbasic" },
                { Lexer.Markdown, "markdown" },
                { Lexer.R, "r" }
            };

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The reverse of <see cref="toName" />, mapping each Lexilla lexer
        /// name back to its <see cref="Lexer" /> enumeration value using an
        /// ordinal (case-sensitive) string comparison.
        /// </summary>
        private static readonly Dictionary<string, Lexer> toLexer =
            BuildReverse();
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Static Methods
        /// <summary>
        /// Gets the Lexilla lexer name for the specified
        /// <see cref="Lexer" /> enumeration value.
        /// </summary>
        /// <param name="lexer">
        /// The <see cref="Lexer" /> enumeration value to look up.
        /// </param>
        /// <returns>
        /// The Lexilla lexer name for <paramref name="lexer" />, or null if
        /// it has no associated lexer (e.g. <see cref="Lexer.Container" />).
        /// </returns>
        public static string GetName(
            Lexer lexer /* in */
            )
        {
            string name;

            return toName.TryGetValue(lexer, out name) ? name : null;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the <see cref="Lexer" /> enumeration value for the specified
        /// Lexilla lexer name.
        /// </summary>
        /// <param name="name">
        /// The Lexilla lexer name to look up.
        /// </param>
        /// <returns>
        /// The <see cref="Lexer" /> enumeration value for
        /// <paramref name="name" />, or <see cref="Lexer.Container" /> when
        /// the name is null, empty, or unrecognized.
        /// </returns>
        public static Lexer GetLexer(
            string name /* in */
            )
        {
            if (String.IsNullOrEmpty(name))
                return Lexer.Container;

            Lexer lexer;

            return toLexer.TryGetValue(name, out lexer)
                ? lexer : Lexer.Container;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Static Methods
        /// <summary>
        /// Builds the reverse (name to <see cref="Lexer" />) lookup from
        /// <see cref="toName" /> using an ordinal (case-sensitive) string
        /// comparison.
        /// </summary>
        /// <returns>
        /// A new dictionary mapping each Lexilla lexer name to its
        /// <see cref="Lexer" /> enumeration value.
        /// </returns>
        private static Dictionary<string, Lexer> BuildReverse()
        {
            Dictionary<string, Lexer> reverse =
                new Dictionary<string, Lexer>(StringComparer.Ordinal);

            foreach (KeyValuePair<Lexer, string> pair in toName)
                reverse[pair.Value] = pair.Key;

            return reverse;
        }
        #endregion
    }
}
