namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private ReadOnlyMemory<char> _text;
        private readonly int _length;
        private int _baseIndex = -1;
        private int _lineNumber = 1;
        private int _columnNumber = 0;
        private int _skipIndex = 0;
        private int _skipOffset = 0;

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;
            _length = text.Length;
        }

        public bool Next()
        {
            if (_baseIndex < _length)
            {
                _baseIndex++;

                if (_baseIndex == _length) return false;

                SkipCharacters();
            }

            return true;

            void SkipCharacters()
            {
                var currentChar = _text.Span[_baseIndex];

                if (_skipIndex == _baseIndex)
                {
                    _baseIndex = _skipIndex + _skipOffset;
                    _skipIndex = _skipOffset = 0;
                }
                else
                {
                    SkipCarriageReturn(ref currentChar);
                    SkipLeadingSpaces(ref currentChar);
                }
            }

            void SkipCarriageReturn(ref char currentChar)
            {
                if (currentChar == Return)
                {
                    currentChar = _text.Span[_baseIndex + 1];

                    if (currentChar == NewLine)
                    {
                        _baseIndex++;
                    }
                }
            }

            void SkipLeadingSpaces(ref char currentChar)
            {
                if (currentChar == NewLine)
                {
                    currentChar = _text.Span[_baseIndex + 1];

                    if (currentChar == Space || currentChar == Tab)
                    {
                        _skipOffset += 2;
                        _skipIndex = _baseIndex + _skipOffset;
                        
                        var whitespace = _text.Span[++_skipOffset];

                        while (char.IsWhiteSpace(whitespace))
                        {
                            whitespace = _text.Span[++_skipOffset];
                        }
                    }
                }
            }
        }
        
        public Character ReadChar()
        {
            Character? ch;

            if (_baseIndex < _length)
            {
                var level = 1;
                var currentChar = _text.Span[_baseIndex];

                switch (currentChar)
                {
                    case NewLine:
                        _lineNumber++;
                        _columnNumber = 0;
                        break;
                    case Space when _columnNumber == 0:
                    case Tab when _columnNumber == 0:
                        _columnNumber++;

                        var index = _baseIndex;
                        var whitespace = _text.Span[++index];

                        while (char.IsWhiteSpace(whitespace))
                        {
                            level++;
                            _columnNumber++;
                            whitespace = _text.Span[++index];
                        }
                        break;
                    default:
                        _columnNumber++;
                        break;
                }

                ch = new Character(currentChar, new Position(_baseIndex, _lineNumber, _columnNumber, level));
            }
            else
            {
                ch = new Character(EndOfFile, new Position(_baseIndex, _lineNumber + 1));
            }

            return ch.Value;
        }
    }
}