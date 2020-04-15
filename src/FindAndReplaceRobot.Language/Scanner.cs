namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private ReadOnlyMemory<char> _text;
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

        public ReadOnlyMemory<char> GetSlice(Range range) => _text[range].TrimEnd('\r');

        public void MoveAhead()
        {
            CurrentPosition += _offset;
            _offset = 0;
        }

        public bool MoveNext() => CurrentPosition < TextLength && ++CurrentPosition < TextLength;

        public char ReadAhead(int offset = 1, bool skipReturns = true)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            _offset = offset;

            var ch = AbsolutePosition < TextLength ? _text.Span[AbsolutePosition] : EndOfFile;

            if (skipReturns && TrySkipCarriageReturn(AbsolutePosition, ref ch)) _offset++;

            return ch;
        }

        public char ReadChar(bool skipReturns = true)
        {
            var ch = CurrentPosition < TextLength ? _text.Span[CurrentPosition] : EndOfFile;

            if (skipReturns && TrySkipCarriageReturn(CurrentPosition, ref ch)) CurrentPosition++;

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