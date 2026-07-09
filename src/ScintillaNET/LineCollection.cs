using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ScintillaNET
{
    // TODO Revisit this following Scintilla v3.7.0 because is said to be better about character handling

    /// <summary>
    /// An immutable collection of lines of text in a <see cref="Scintilla" /> control.
    /// </summary>
    public class LineCollection : IEnumerable<Line>
    {
        #region Fields

        private readonly Scintilla scintilla;
        private GapBuffer<PerLine> perLineData;

        // The 'step' is a break in the continuity of our line starts. It allows us
        // to delay the updating of every line start when text is inserted/deleted.
        private long stepLine;
        private long stepLength;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Adjust the number of CHARACTERS in a line.
        /// </summary>
        private void AdjustLineLength(long index, long delta)
        {
            MoveStep(index);
            stepLength += delta;

            // Invalidate multibyte flag
            PerLine perLine = perLineData[(int)index];
            perLine.ContainsMultibyte = ContainsMultibyte.Unkown;
            perLineData[(int)index] = perLine;
        }

        /// <summary>
        /// Converts a BYTE offset to a CHARACTER offset.
        /// </summary>
        internal long ByteToCharPosition(long pos)
        {
            Debug.Assert(pos >= 0);
            Debug.Assert(pos <= scintilla.DirectMessage(NativeMethods.SCI_GETLENGTH).ToInt64());

            long line = scintilla.DirectMessage(NativeMethods.SCI_LINEFROMPOSITION, new IntPtr(pos)).ToInt64();
            long byteStart = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(line)).ToInt64();
            long count = CharPositionFromLine(line) + GetCharCount(byteStart, pos - byteStart);

            return count;
        }

        /// <summary>
        /// Returns the number of CHARACTERS in a line.
        /// </summary>
        internal long CharLineLength(long index)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < Count);

            // A line's length is calculated by subtracting its start offset from
            // the start of the line following. We keep a terminal (faux) line at
            // the end of the list so we can calculate the length of the last line.

            if (index + 1 <= stepLine)
                return perLineData[(int)(index + 1)].Start - perLineData[(int)index].Start;
            else if (index <= stepLine)
                return (perLineData[(int)(index + 1)].Start + stepLength) - perLineData[(int)index].Start;
            else
                return (perLineData[(int)(index + 1)].Start + stepLength) - (perLineData[(int)index].Start + stepLength);
        }

        /// <summary>
        /// Returns the CHARACTER offset where the line begins.
        /// </summary>
        internal long CharPositionFromLine(long index)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < perLineData.Count); // Allow query of terminal line start

            long start = perLineData[(int)index].Start;
            if (index > stepLine)
                start += stepLength;

            return start;
        }

        internal long CharToBytePosition(long pos)
        {
            Debug.Assert(pos >= 0);
            Debug.Assert(pos <= TextLength);

            // Adjust to the nearest line start
            long line = LineFromCharPosition(pos);
            long lineByteStart = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(line)).ToInt64();
            pos -= CharPositionFromLine(line);

            if (pos <= 0)
                return lineByteStart;

            // Optimization when the line contains NO multibyte characters
            if (!LineContainsMultibyteChar(line))
                return (lineByteStart + pos);

            // Find the byte offset within the line whose CHARACTER count equals "pos",
            // using the same whole-buffer decoder that ByteToCharPosition / GetCharCount
            // use. This makes CharToBytePosition the exact inverse of ByteToCharPosition,
            // so the map stays self-consistent even for malformed UTF-8 -- where native
            // SCI_POSITIONRELATIVE classifies invalid bytes differently than the .NET
            // decoder that produces the char counts (and the "Text" a caller sees).
            long lineByteLength = scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(line)).ToInt64();
            IntPtr ptr = scintilla.DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, new IntPtr(lineByteStart), new IntPtr(lineByteLength));

            int lo = 0;
            int hi = (int)lineByteLength;
            while (lo < hi)
            {
                int mid = lo + ((hi - lo) / 2);
                if (GetCharCount(ptr, mid, scintilla.Encoding) < pos)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            return lineByteStart + lo;
        }

        private void DeletePerLine(long index)
        {
            Debug.Assert(index != 0);

            // Bounds guard (Debug.Assert is stripped in Release): refuse an index that
            // would walk off perLineData -- CharLineLength(index) reads perLineData[index+1].
            if (index < 1 || index > perLineData.Count - 2)
                throw new ArgumentOutOfRangeException("index");

            MoveStep(index);

            // Subtract the line length
            stepLength -= CharLineLength(index);

            // Remove the line
            perLineData.RemoveAt((int)index);

            // Move the step to the line before the one removed
            stepLine--;
        }

#if DEBUG

        /// <summary>
        /// Dumps the line buffer to a string.
        /// </summary>
        /// <returns>A string representing the line buffer.</returns>
        public string Dump()
        {
            using (StringWriter writer = new StringWriter())
            {
                scintilla.Lines.Dump(writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Dumps the line buffer to the specified TextWriter.
        /// </summary>
        /// <param name="writer">The writer to use for dumping the line buffer.</param>
        public unsafe void Dump(TextWriter writer)
        {
            int totalChars = 0;

            for (int i = 0; i < perLineData.Count; i++)
            {
                string error = totalChars == CharPositionFromLine(i) ? null : "*";
                if (i == perLineData.Count - 1)
                {
                    writer.WriteLine("{0}[{1}] {2} (terminal)", error, i, CharPositionFromLine(i));
                }
                else
                {
                    int len = scintilla.DirectMessage(NativeMethods.SCI_GETLINE, new IntPtr(i)).ToInt32();
                    byte[] bytes = new byte[len];

                    fixed (byte* ptr = bytes)
                        scintilla.DirectMessage(NativeMethods.SCI_GETLINE, new IntPtr(i), new IntPtr(ptr));

                    string str = scintilla.Encoding.GetString(bytes);
                    string containsMultibyte = "U";
                    if (perLineData[i].ContainsMultibyte == ContainsMultibyte.Yes)
                        containsMultibyte = "Y";
                    else if (perLineData[i].ContainsMultibyte == ContainsMultibyte.No)
                        containsMultibyte = "N";

                    writer.WriteLine("{0}[{1}] {2}:{3}:{4} {5}", error, i, CharPositionFromLine(i), str.Length, containsMultibyte, str.Replace("\r", "\\r").Replace("\n", "\\n"));
                    totalChars += str.Length;
                }
            }
        }

#endif

        /// <summary>
        /// Gets the number of CHARACTERS int a BYTE range.
        /// </summary>
        private int GetCharCount(long pos, long length)
        {
            IntPtr ptr = scintilla.DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, new IntPtr(pos), new IntPtr(length));
            return GetCharCount(ptr, (int)length, scintilla.Encoding);
        }

        /// <summary>
        /// Gets the number of CHARACTERS in a BYTE range.
        /// </summary>
        private static unsafe int GetCharCount(IntPtr text, int length, Encoding encoding)
        {
            if (text == IntPtr.Zero || length == 0)
                return 0;

            // Never use SCI_COUNTCHARACTERS. It counts CRLF as 1 char!
            int count = encoding.GetCharCount((byte*)text, length);
            return count;
        }

        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An object that contains all <see cref="Line" /> objects within the <see cref="LineCollection" />.</returns>
        public IEnumerator<Line> GetEnumerator()
        {
            long count = Count;
            for (long i = 0; i < count; i++)
                yield return this[i];

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private bool LineContainsMultibyteChar(long index)
        {
            PerLine perLine = perLineData[(int)index];
            if (perLine.ContainsMultibyte == ContainsMultibyte.Unkown)
            {
                perLine.ContainsMultibyte =
                    (scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(index)).ToInt64() == CharLineLength(index))
                    ? ContainsMultibyte.No
                    : ContainsMultibyte.Yes;

                perLineData[(int)index] = perLine;
            }

            return (perLine.ContainsMultibyte == ContainsMultibyte.Yes);
        }

        /// <summary>
        /// Returns the line index containing the CHARACTER position.
        /// </summary>
        internal long LineFromCharPosition(long pos)
        {
            Debug.Assert(pos >= 0);

            // Iterative binary search
            // http://en.wikipedia.org/wiki/Binary_search_algorithm
            // System.Collections.Generic.ArraySortHelper.InternalBinarySearch

            long low = 0L;
            long high = Count - 1;

            while (low <= high)
            {
                long mid = low + ((high - low) / 2);
                long start = CharPositionFromLine(mid);

                if (pos == start)
                    return mid;
                else if (start < pos)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            // After while exit, 'low' will point to the index where 'pos' should be
            // inserted (if we were creating a new line start). The line containing
            // 'pos' then would be 'low - 1'.
            return low - 1;
        }

        /// <summary>
        /// Tracks a new line with the given CHARACTER length.
        /// </summary>
        private void InsertPerLine(long index, long length)
        {
            MoveStep(index);

            PerLine data;
            long lineStart = 0;

            // Add the new line length to the existing line start
            data = perLineData[(int)index];
            lineStart = data.Start;
            data.Start += length;
            perLineData[(int)index] = data;

            // Insert the new line
            data = new PerLine(lineStart);
            perLineData.Insert((int)index, data);

            // Move the step
            stepLength += length;
            stepLine++;
        }

        private void MoveStep(long line)
        {
            if (stepLength == 0)
            {
                stepLine = line;
            }
            else if (stepLine < line)
            {
                PerLine data;
                while (stepLine < line)
                {
                    stepLine++;
                    data = perLineData[(int)stepLine];
                    data.Start += stepLength;
                    perLineData[(int)stepLine] = data;
                }
            }
            else if (stepLine > line)
            {
                PerLine data;
                while (stepLine > line)
                {
                    data = perLineData[(int)stepLine];
                    data.Start -= stepLength;
                    perLineData[(int)stepLine] = data;
                    stepLine--;
                }
            }
        }

        internal void RebuildLineData()
        {
            stepLine = 0;
            stepLength = 0;

            perLineData = new GapBuffer<PerLine>();
            perLineData.Add(new PerLine(0));
            perLineData.Add(new PerLine(0)); // Terminal

            // Fake an insert notification
            NativeMethods.SCNotification scn = new NativeMethods.SCNotification();
            long adjustedLines = scintilla.DirectMessage(NativeMethods.SCI_GETLINECOUNT).ToInt64() - 1;
            scn.linesAdded = new IntPtr(adjustedLines);
            scn.position = IntPtr.Zero;
            scn.length = scintilla.DirectMessage(NativeMethods.SCI_GETLENGTH);
            scn.text = scintilla.DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, scn.position, scn.length);
            TrackInsertText(scn);
        }

        private void scintilla_SCNotification(object sender, SCNotificationEventArgs e)
        {
            NativeMethods.SCNotification scn = e.SCNotification;
            switch (scn.nmhdr.code)
            {
                case NativeMethods.SCN_MODIFIED:
                    ScnModified(scn);
                    break;
            }
        }

        private void ScnModified(NativeMethods.SCNotification scn)
        {
            try
            {
                if ((scn.modificationType & NativeMethods.SC_MOD_DELETETEXT) > 0)
                {
                    TrackDeleteText(scn);
                }

                if ((scn.modificationType & NativeMethods.SC_MOD_INSERTTEXT) > 0)
                {
                    TrackInsertText(scn);
                }
            }
            catch (Exception)
            {
                // Never let a tracking fault leave the managed line mirror permanently
                // out of sync with native -- that would make every subsequent edit
                // re-fault (a persistent DoS on malformed or pathological content).
                // Resync the entire mirror from native instead.
                RebuildLineData();
            }
        }

        private void TrackDeleteText(NativeMethods.SCNotification scn)
        {
            long startLine = scintilla.DirectMessage(NativeMethods.SCI_LINEFROMPOSITION, scn.position).ToInt64();
            if (scn.linesAdded == IntPtr.Zero)
            {
                // That was easy
                int delta = GetCharCount(scn.text, scn.length.ToInt32(), scintilla.Encoding);
                AdjustLineLength(startLine, delta * -1);
            }
            else
            {
                // Adjust the existing line
                long lineByteStart = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(startLine)).ToInt64();
                long lineByteLength = scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(startLine)).ToInt64();
                AdjustLineLength(startLine, GetCharCount(lineByteStart, lineByteLength) - CharLineLength(startLine));

                // Bound the removal to the lines the mirror actually has after
                // startLine, so a line delta larger than the tracked count cannot
                // walk DeletePerLine off the end of perLineData.
                long linesRemoved = Math.Min(scn.linesAdded.ToInt64() * -1, (Count - 1) - startLine);
                for (long i = 0; i < linesRemoved; i++)
                {
                    // Deleted line
                    DeletePerLine(startLine + 1);
                }
            }
        }

        private void TrackInsertText(NativeMethods.SCNotification scn)
        {
            long startLine = scintilla.DirectMessage(NativeMethods.SCI_LINEFROMPOSITION, scn.position).ToInt64();
            if (scn.linesAdded == IntPtr.Zero)
            {
                // That was easy
                int delta = GetCharCount(scn.position.ToInt64(), scn.length.ToInt64());
                AdjustLineLength(startLine, delta);
            }
            else
            {
                long lineByteStart = 0;
                long lineByteLength = 0;

                // Adjust existing line
                lineByteStart = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(startLine)).ToInt64();
                lineByteLength = scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(startLine)).ToInt64();
                AdjustLineLength(startLine, GetCharCount(lineByteStart, lineByteLength) - CharLineLength(startLine));

                long linesAdded = scn.linesAdded.ToInt64();
                for (long i = 1; i <= linesAdded; i++)
                {
                    long line = startLine + i;

                    // Insert new line
                    lineByteStart += lineByteLength;
                    lineByteLength = scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(line)).ToInt64();
                    InsertPerLine(line, GetCharCount(lineByteStart, lineByteLength));
                }
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets a value indicating whether all the document lines are visible (not hidden).
        /// </summary>
        /// <returns>true if all the lines are visible; otherwise, false.</returns>
        public bool AllLinesVisible
        {
            get
            {
                return (scintilla.DirectMessage(NativeMethods.SCI_GETALLLINESVISIBLE) != IntPtr.Zero);
            }
        }

        /// <summary>
        /// Gets the number of lines.
        /// </summary>
        /// <returns>The number of lines in the <see cref="LineCollection" />.</returns>
        public long Count
        {
            get
            {
                // Subtract the terminal line
                return (perLineData.Count - 1);
            }
        }

        /// <summary>
        /// Gets the number of CHARACTERS in the document.
        /// </summary>
        internal long TextLength
        {
            get
            {
                // Where the terminal line begins
                return CharPositionFromLine(perLineData.Count - 1);
            }
        }

        /// <summary>
        /// Gets the <see cref="Line" /> at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the <see cref="Line" /> to get.</param>
        /// <returns>The <see cref="Line" /> at the specified index.</returns>
        public Line this[long index]
        {
            get
            {
                index = Helpers.Clamp(index, 0, Count - 1);
                return new Line(scintilla, index);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LineCollection" /> class.
        /// </summary>
        /// <param name="scintilla">The <see cref="Scintilla" /> control that created this collection.</param>
        public LineCollection(Scintilla scintilla)
        {
            this.scintilla = scintilla;
            this.scintilla.SCNotification += scintilla_SCNotification;

            this.perLineData = new GapBuffer<PerLine>();
            this.perLineData.Add(new PerLine(0));
            this.perLineData.Add(new PerLine(0)); // Terminal
        }

        #endregion Constructors

        #region Types

        /// <summary>
        /// Stuff we track for each line.
        /// </summary>
        private struct PerLine
        {
            /// <summary>
            /// The CHARACTER position where the line begins.
            /// </summary>
            public long Start;

            /// <summary>
            /// 1 if the line contains multibyte (Unicode) characters; -1 if not; 0 if undetermined.
            /// </summary>
            /// <remarks>Using an enum instead of Nullable because it uses less memory per line...</remarks>
            public ContainsMultibyte ContainsMultibyte;

            public PerLine(long start)
            {
                this.Start = start;
                this.ContainsMultibyte = ContainsMultibyte.Unkown;
            }
        }

        private enum ContainsMultibyte
        {
            No = -1,
            Unkown,
            Yes
        }

        #endregion Types
    }
}
