namespace FindAndReplaceRobot.Language
{
    using System;
    using System.Diagnostics;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private readonly ReadOnlyMemory<char> _text;
        private int _offset;
        private int _prevIndex = -1;

        private enum ReadMode
        {
            Normal,
            Lookahead,
            Peeking
        }

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;

            TextLength = text.Length;

            Position = new Position(1, 0);
        }

        public Scanner(string text) : this(text.AsMemory()) { }

        public int TextLength { get; }

        public int CurrentIndex { get; private set; }

        public int AbsoluteIndex => CurrentIndex + _offset;

        public Position Position { get; private set; }

        public ReadOnlyMemory<char> GetSlice(Range range) => GetSlice(range, false, out _);

        public ReadOnlyMemory<char> GetSlice(Range range, out bool handledCRLF) => GetSlice(range, true, out handledCRLF);

        public void MoveAhead()
        {
            CurrentIndex = AbsoluteIndex < TextLength ? AbsoluteIndex : TextLength;

            _offset = 0;
        }

        public bool MoveNext() => CurrentIndex < TextLength && ++CurrentIndex < TextLength;

        public char ReadAhead()
        {
            if (_offset >= 0) _offset++;

            return GetChar(AbsoluteIndex, ref _offset, ReadMode.Lookahead);
        }

        public char PeekAhead(ref int offset)
        {
            if (offset <= 0) throw new ArgumentOutOfRangeException(nameof(offset));

            return GetChar(CurrentIndex + offset, ref offset, ReadMode.Peeking);
        }

        public void StepTo(int offset)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            _offset = offset;

            MoveAhead();
        }

        public char ReadChar()
        {
            var offset = 0;

            return GetChar(CurrentIndex, ref offset, ReadMode.Normal);
        }

        private char GetChar(int index, ref int offset, ReadMode mode)
        {
            var ch = index < TextLength ? _text.Span[index] : EndOfFile;

            if (TrySkipCarriageReturn(index, ref ch))
            {
                index++;

                if (mode == ReadMode.Normal)
                {
                    CurrentIndex++;
                }
                else if (mode == ReadMode.Lookahead || mode == ReadMode.Peeking)
                {
                    offset++;
                }
            }

            if (index > _prevIndex)
            {
                Position = ch == NewLine ? Position.NextLine() : Position.NextColumn();

                _prevIndex = index;
            }

            Debug.WriteLine($"{ch.ToReadableString()}\t\t[{index}+{offset}]\t\t[{Position.LineNumber}:{Position.ColumnNumber}]\t\t{mode}");

            return ch;

            bool TrySkipCarriageReturn(int index, ref char ch)
            {
                if (ch == Return)
                {
                    var nextChar = ++index < TextLength ? _text.Span[index] : ch;

                    if (nextChar == NewLine)
                    {
                        ch = nextChar;

                        return true;
                    }
                }

                return false;
            }
        }

        private ReadOnlyMemory<char> GetSlice(Range range, bool handleCRLF, out bool handledCRLF)
        {
            handledCRLF = false;

            if (handleCRLF)
            {
                var start = range.Start.Value;
                var end = range.End.Value + 1;

                if (end < TextLength && _text[start..end].Span is var span && span.Length > 1)
                {
                    var cr = span[^2];
                    var lf = span[^1];

                    if (cr == '\r' && lf == '\n')
                    {
                        handledCRLF = true;

                        return _text[start..(end - 2)];
                    }
                }
            }

            return _text[range];
        }
    }
}