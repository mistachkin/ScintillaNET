using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Specifies the lexer to use for syntax highlighting in a <see cref="Scintilla" /> control.
    /// </summary>
    public enum Lexer
    {
        /// <summary>
        /// Lexing is performed by the <see cref="Scintilla" /> control container (host) using
        /// the <see cref="Scintilla.StyleNeeded" /> event.
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
        /// The Clarion language 
        /// </summary>
        Clw = NativeMethods.SCLEX_CLW,

        /// <summary>
        /// The Clarion language No Case 
        /// </summary>
        ClwNoCase = NativeMethods.SCLEX_CLWNOCASE,
        /// <summary>
        /// The C language family (C++, C, C#, Java, JavaScript, etc...) lexer.
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

    // Maps the legacy numeric Lexer enum to the Lexilla lexer name passed to CreateLexer
    // (Scintilla 5.x installs lexers by name via Lexilla, not by numeric id). Names were taken
    // from the Scintilla 4.4.6 LexerModule registrations; if a lexer ever fails to load,
    // cross-check against Lexilla's GetLexerName. Lexer.Container has no entry -- container
    // (host-driven) styling installs no lexer at all.
    internal static class LexerNames
    {
        private static readonly Dictionary<Lexer, string> toName = new Dictionary<Lexer, string>
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
            { Lexer.R, "r" },
        };

        private static readonly Dictionary<string, Lexer> toLexer = BuildReverse();

        // The Lexilla lexer name for an enum value, or null for Container (no lexer installed).
        public static string GetName(Lexer lexer)
        {
            string name;
            return toName.TryGetValue(lexer, out name) ? name : null;
        }

        // The enum value for a lexer name; Container when the name is empty or unrecognized.
        public static Lexer GetLexer(string name)
        {
            if (String.IsNullOrEmpty(name))
                return Lexer.Container;

            Lexer lexer;
            return toLexer.TryGetValue(name, out lexer) ? lexer : Lexer.Container;
        }

        private static Dictionary<string, Lexer> BuildReverse()
        {
            var reverse = new Dictionary<string, Lexer>(StringComparer.Ordinal);
            foreach (var pair in toName)
                reverse[pair.Value] = pair.Key;

            return reverse;
        }
    }
}
