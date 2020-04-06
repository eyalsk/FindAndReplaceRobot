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
        private int _columnNumber;
        private int _skipIndex = -1;
        private int _skipOffset;
        private int _level = 1;

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

                var currentChar = _text.Span[_baseIndex];

                if (_skipIndex == _baseIndex)
                {
                    _level = _skipOffset + 1;
                    _baseIndex = _skipIndex + _skipOffset;
                    _skipIndex = -1;
                    _skipOffset = 0;
                }
                else
                {
                    if (TryNextChar(ref currentChar, Return) && currentChar == NewLine)
                    {
                        _baseIndex++;
                    }
                    
                    if (TryNextChar(ref currentChar, NewLine, out var index) && IsIndentChar(currentChar))
                    {
                        _skipIndex = index;

                        if (++index < _text.Length)
                        {
                            var ch = _text.Span[index];

                            while (IsIndentChar(ch))
                            {
                                _skipOffset++;

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
                }
            }

            return true;
        }

        public Character ReadChar()
        {
            Character? ch;

            if (_baseIndex < _length)
            {
                var currentChar = _text.Span[_baseIndex];

                switch (currentChar)
                {
                    case NewLine:
                        _lineNumber++;
                        _columnNumber = 0;
                        _level = 1;
                        break;
                    case Space when _columnNumber == 0:
                    case Tab when _columnNumber == 0:
                        _columnNumber++;
                        break;
                    default:
                        _columnNumber += _level;
                        break;
                }

                ch = new Character(currentChar, new Position(_baseIndex, _lineNumber, _columnNumber, _level));
            }
            else
            {
                ch = new Character(EndOfFile, new Position(_baseIndex, _lineNumber + 1));
            }

            return ch.Value;
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