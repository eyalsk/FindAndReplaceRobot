namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private ReadOnlyMemory<char> _text;
        private readonly int _length;
        private int _baseIndex;
        private int _lineNumber = 1;
        private int _columnNumber = 1;

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;
            _length = text.Length;
        }

        public Scanner(string text) : this(text.AsMemory())
        {
        }

        internal Position Position => new Position(_baseIndex, _lineNumber, _columnNumber);

        public bool Next()
        {
            if (_baseIndex < _length)
            {
                var currentChar = _text.Span[_baseIndex];

                if (TryNextChar(ref currentChar, Return) && currentChar == NewLine)
                {
                    _baseIndex++;
                }

                if (currentChar == NewLine)
                {
                    _lineNumber++;
                    _columnNumber = 1;
                }

                if (TryNextChar(ref currentChar, NewLine, out var index) && IsIndentChar(currentChar))
                {
                    _baseIndex = index;

                    if (++index < _text.Length)
                    {
                        var ch = _text.Span[index];

                        while (IsIndentChar(ch))
                        {
                            _baseIndex++;
                            _columnNumber++;

                            if (++index < _text.Length)
                            {
                                ch = _text.Span[index];
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                _baseIndex++;

                return true;
            }

            return false;
        }

        public char ReadChar() => _baseIndex < _length ? _text.Span[_baseIndex] : EndOfFile;

        public char ReadAhead(int offset = 1) {
            offset = offset >= 0 ? offset : throw new ArgumentOutOfRangeException("offset");

            return _baseIndex + offset is var position &&
                    position < _length ? _text.Span[position] : EndOfFile;
        }
            
        private static bool IsIndentChar(char currentChar) => currentChar == Space || currentChar == Tab;

        private bool TryNextChar(ref char currentChar, char nextChar, out int index)
        {
            index = _baseIndex + 1;

            var canGetNextChar = index < _text.Length && currentChar == nextChar;

            if (canGetNextChar)
            {
                currentChar = _text.Span[index];
            }

            return canGetNextChar;
        }

        private bool TryNextChar(ref char currentChar, char nextChar) => TryNextChar(ref currentChar, nextChar, out _);
    }
}