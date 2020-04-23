namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private readonly ReadOnlyMemory<char> _text;
        private int _offset;

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;

            TextLength = text.Length;
        }

        public Scanner(string text) : this(text.AsMemory()) { }

        public int TextLength { get; }

        public int CurrentPosition { get; private set; }

        public int AbsolutePosition => CurrentPosition + _offset;

        public ReadOnlyMemory<char> GetSlice(Range range) => _text[range];

        public void MoveAhead()
        {
            CurrentPosition = AbsolutePosition < TextLength ? AbsolutePosition : TextLength;

            _offset = 0;
        }

        public bool MoveNext() => CurrentPosition < TextLength && ++CurrentPosition < TextLength;

        public char ReadAhead(int offset = 1, bool skipReturns = true)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            _offset = offset;

            return GetChar(AbsolutePosition, skipReturns);
        }

        public char PeekAhead(int offset = 1, bool skipReturns = true)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            return GetChar(CurrentPosition + offset, skipReturns);
        }

        public char ReadChar(bool skipReturns = true) => GetChar(CurrentPosition, skipReturns);

        private char GetChar(int index, bool skipReturns = true)
        {
            var ch = index < TextLength ? _text.Span[index] : EndOfFile;

            if (skipReturns && TrySkipCarriageReturn(index, ref ch))
            {
                if (index == CurrentPosition)
                {
                    CurrentPosition++;
                }
                else if (index == AbsolutePosition)
                {
                    _offset++;
                }
            }

            return ch;
        }

        private bool TrySkipCarriageReturn(int pos, ref char ch)
        {
            if (ch == Return)
            {
                var index = pos + 1;
                var nextChar = index < TextLength ? _text.Span[index] : ch;

                if (nextChar == NewLine)
                {
                    ch = nextChar;

                    return true;
                }
            }

            return false;
        }
    }
}