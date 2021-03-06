﻿/*

 Copyright (c) 2013 DEVSENSE
  
 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 
 You must not remove this notice from this software.

*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PHP.Core.Text
{
    #region ILineBreaks

    /// <summary>
    /// Manages information about line breaks in the document.
    /// </summary>
    public interface  ILineBreaks
    {
        /// <summary>
        /// Gets amount of line breaks.
        /// </summary>
        /// <remarks>Lines count equals <see cref="Count"/> + <c>1</c>.</remarks>
        int Count { get; }

        /// <summary>
        /// Gets length of document.
        /// </summary>
        int TextLength { get; }

        /// <summary>
        /// Gets position of <paramref name="index"/>-th line end, including its break characters.
        /// </summary>
        /// <param name="index">Index of te line.</param>
        /// <returns>Position of the line end.</returns>
        int EndOfLineBreak(int index);

        /// <summary>
        /// Gets line number from <paramref name="position"/> within document.
        /// </summary>
        /// <param name="position">Position within document.</param>
        /// <returns>Line number.</returns>
        /// <exception cref="ArgumentOutOfRangeException">In case <paramref name="position"/> is out of line number range.</exception>
        int GetLineFromPosition(int position);
    }

    #endregion

    #region LineBreaks

    public abstract class LineBreaks : ILineBreaks
    {
        #region ILineBreaks Members

        public abstract int Count { get; }

        public abstract int EndOfLineBreak(int index);

        public int TextLength
        {
            get { return _textLength; }
        }

        /// <summary>
        /// Gets line number from <paramref name="position"/> within document.
        /// </summary>
        /// <param name="position">Position within document.</param>
        /// <returns>Line number.</returns>
        /// <exception cref="ArgumentOutOfRangeException">In case <paramref name="position"/> is out of text document range.</exception>
        public int GetLineFromPosition(int position)
        {
            if (position < 0 || position > this.TextLength)
                throw new ArgumentOutOfRangeException("position");
            
            //
            if (position == this.TextLength)
                return this.LinesCount - 1;
            
            // binary search
            int a = 0;
            int b = this.Count;
            while (a < b)
            {
                int x = (a + b) / 2;
                if (position < this.EndOfLineBreak(x))
                    b = x;
                else
                    a = x + 1;
            }
            return a;
        }

        #endregion

        private readonly int _textLength;

        protected LineBreaks(int textLength)
        {
            _textLength = textLength;
        }

        public static LineBreaks/*!*/Create(string text, List<int>/*!*/lineEnds)
        {
            if (text == null || lineEnds == null) throw new ArgumentNullException();
            if (lineEnds.Count == 0 || lineEnds.Last() <= ushort.MaxValue)
            {
                return new ShortLineBreaks(text.Length, lineEnds);
            }
            else
            {
                return new IntLineBreaks(text.Length, lineEnds);
            }
        }

        public static LineBreaks/*!*/Create(string text)
        {
	        return Create(text, CalculateLineEnds(text));
        }

        /// <summary>
        /// Amount of lines in the document.
        /// </summary>
        public int LinesCount { get { return this.Count + 1; } }

        /// <summary>
        /// Gets list of line ends position.
        /// </summary>
        /// <param name="text">Document text.</param>
        /// <returns>List of line ends position.</returns>
        private static List<int>/*!*/CalculateLineEnds(string text)
        {
            List<int> list = new List<int>();
            if (text != null)
            {
                int i = 0;
                while (i < text.Length)
                {
                    int len = TextUtils.LengthOfLineBreak(text, i);
                    if (len == 0)
                    {
                        i++;
                    }
                    else
                    {
                        i += len;
                        list.Add(i);
                    }
                }
            }
            return list;
        }

#if DEBUG

        [Test]
        private static void Test()
        {
            string sample = "Hello World";
            foreach (var nl in new[] { "\r", "\r\n", "\n", "\u0085", "\u2028", "\u2029" })
            {
                for (int linecount = 0; linecount < 512; linecount += 17)
                {
                    // construct sample text with {linecount} lines
                    string text = string.Empty;
                    for (var line = 0; line < linecount; line++)
                        text += sample + nl;
                    text += sample;

                    // test LineBreaks
                    var linebreaks = LineBreaks.Create(text);
                    for (int i = 0; i <= text.Length; i += 7)
                        Debug.Assert(linebreaks.GetLineFromPosition(i) == (i / (sample.Length + nl.Length)));
                }
            }
        }

#endif
    }

    #endregion

    #region ShortLineBreaks

    /// <summary>
    /// Optimized generalization of <see cref="LineBreaks"/> using <see cref="ushort"/> internally.
    /// </summary>
    internal sealed class ShortLineBreaks : LineBreaks
    {
        private readonly ushort[]/*!*/_lineEnds;

        public ShortLineBreaks(int textLength, List<int> lineEnds)
            :base(textLength)
        {
            var count = lineEnds.Count;
            if (count == 0)
            {
                _lineEnds = ArrayUtils.EmptyUShorts;
            }
            else
            {
                _lineEnds = new ushort[count];
                for (int i = 0; i < count; i++)
                    _lineEnds[i] = (ushort)lineEnds[i];
            }
        }

        public override int Count
        {
            get { return _lineEnds.Length; }
        }

        public override int EndOfLineBreak(int index)
        {
            return (int)_lineEnds[index];
        }
    }

    #endregion

    #region IntLineBreaks

    /// <summary>
    /// Generalization of <see cref="LineBreaks"/> using <see cref="int"/> internally.
    /// </summary>
    internal sealed class IntLineBreaks : LineBreaks
    {
        private readonly int[]/*!*/_lineEnds;

        public IntLineBreaks(int textLength, List<int> lineEnds)
            : base(textLength)
        {
            var count = lineEnds.Count;
            if (count == 0)
            {
                _lineEnds = ArrayUtils.EmptyIntegers;
            }
            else
            {
                _lineEnds = lineEnds.ToArray();
            }
        }

        public override int Count
        {
            get { return _lineEnds.Length; }
        }

        public override int EndOfLineBreak(int index)
        {
            return (int)_lineEnds[index];
        }
    }

    #endregion
}
