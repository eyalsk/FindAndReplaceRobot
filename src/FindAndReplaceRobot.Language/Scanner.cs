namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private ReadOnlyMemory<char> _text;
        private readonly int _length;
        private int _lineNumber = 1;
        private int _columnNumber = 0;
        private int _index = -1;
        
        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;
            _length = text.Length;
        }

        public bool Next()
        {
            bool canIncrement = _index < _length;

            if (canIncrement)
            {
                _index++;

                if (_index == _length) return false;

                SkipCarriageReturn();
            }

            return canIncrement;

            void SkipCarriageReturn()
            {
                var currentChar = _text.Span[_index];

                if (currentChar == Return)
                {
                    currentChar = _text.Span[_index + 1];

                    if (currentChar == NewLine)
                    {
                        _index++;
                    }
                }
            }
        }

        public Character GetCurrentValue()
        {
            Character? ch;

            int level = 1;

            if (_index < _length)
            {
                var currentChar = _text.Span[_index];

                if (currentChar == NewLine)
                {
                    _lineNumber++;
                    _columnNumber = 0;
                    level = 1;
                }
                else if (currentChar == Tab || currentChar == Space)
                {
                    // todo: look how deep

                    level++;
                }
                else
                {
                    _columnNumber++;
                }

                ch = new Character(currentChar, new Position(_index, _lineNumber, _columnNumber));
            }
            else
            {
                ch = new Character(EndOfFile, new Position(_index, _lineNumber + 1));
            }

            return ch.Value;
        }

        /*public Character Lookahead(int offset)
        {
        }*/
    }
}