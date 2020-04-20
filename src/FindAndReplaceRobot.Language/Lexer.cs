namespace FindAndReplaceRobot.Language
{
    using System;
    using FindAndReplaceRobot.Language.Tokens;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private SectionMarker _marker = SectionMarker.Header;

        private enum SectionMarker
        {
            None,
            Header,
            Subsection,
            Item
        }

        public Lexer(Scanner scanner) =>
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));

        public Token? ReadToken()
        {
            while (true)
            {
                switch (_scanner.ReadChar())
                {
                    case '@' when _marker == SectionMarker.Header:
                        return LexAnnotation();
                    case '[' when _marker == SectionMarker.Header:
                        return LexSection();
                    case Tab when _marker == SectionMarker.Subsection:
                    case Space when _marker == SectionMarker.Subsection:
                        return LexIndentation();
                    case NewLine:
                        return LexNewLine();
                    case EndOfFile:
                        return null;
                }

                _scanner.MoveNext();
            }
        }

        private static bool IsIndentChar(char currentChar) => currentChar == Space || currentChar == Tab;

        private Token LexNewLine()
        {
            SetSectionMarker();

            var token = new Token(_scanner.CurrentPosition, TokenKind.NewLine, SliceOne());

            _scanner.MoveNext();

            return token;

            void SetSectionMarker()
            {
                var offset = 1;
                var nextChar = _scanner.ReadAhead();

                if (IsIndentChar(nextChar))
                {
                    nextChar = _scanner.ReadAhead(++offset);

                    if (IsIndentChar(nextChar)) _marker = SectionMarker.Subsection;
                }
                else if (nextChar == '@' || nextChar == '[')
                {
                    _marker = SectionMarker.Header;
                }
                else
                {
                    _marker = SectionMarker.None;
                }

                _scanner.Reset();
            }
        }

        private Token LexIndentation()
        {
            var ch = _scanner.ReadChar();
            var start = _scanner.CurrentPosition;
            var nextChar = _scanner.ReadAhead(2);

            _scanner.MoveAhead();

            while (IsIndentChar(nextChar))
            {
                nextChar = _scanner.ReadAhead();
                _scanner.MoveAhead();
            }

            SetSectionMarker(nextChar);

            return new Token(start, ch == Space ? TokenKind.Space : TokenKind.Tab, SliceFrom(start));

            void SetSectionMarker(char nextChar) =>
                _marker = nextChar == '@' ? SectionMarker.Header : SectionMarker.Item;
        }

        private Token LexAnnotation()
        {
            for (int offset = 1; ; offset++)
            {
                var ch = _scanner.ReadAhead(offset);

                if (ch == '(' || ch == NewLine || ch == EndOfFile)
                {
                    var token = new Token(_scanner.CurrentPosition, TokenKind.Annotation, SkipFirstSliceRest());

                    _scanner.MoveAhead();

                    if (ch == '(')
                    {
                        if (_scanner.ReadAhead(++offset) != ')')
                        {
                            _scanner.Reset();

                            // todo: LexAnnotationArguments()
                        }
                        else
                        {
                            _scanner.MoveAhead();
                        }
                    }

                    return token;
                }
                else if (!char.IsLetter(ch))
                {
                    _scanner.MoveAhead();
                }
            }
        }

        private Token LexSection()
        {
            for (int offset = 1; ; offset++)
            {
                var ch = _scanner.ReadAhead(offset);

                if (ch == ']')
                {
                    var token = new Token(_scanner.CurrentPosition, TokenKind.Section, SkipFirstSliceRest());

                    _scanner.MoveAhead();

                    return token;
                }
            }
        }

        private ReadOnlyMemory<char> SliceOne() => _scanner.GetSlice(_scanner.CurrentPosition..(_scanner.CurrentPosition + 1));
        private ReadOnlyMemory<char> SliceFrom(Index start) => _scanner.GetSlice(start.._scanner.AbsolutePosition);
        private ReadOnlyMemory<char> SkipFirstSliceRest() => _scanner.GetSlice((_scanner.CurrentPosition + 1).._scanner.AbsolutePosition);
    }
}
