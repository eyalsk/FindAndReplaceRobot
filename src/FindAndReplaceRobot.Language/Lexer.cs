namespace FindAndReplaceRobot.Language
{
    using System;
    using FindAndReplaceRobot.Language.Tokens;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private TokenKind _tokenMarker;

        public Lexer(Scanner scanner) =>
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));

        public Token? ReadToken()
        {
            while (true)
            {
                switch (_scanner.ReadChar())
                {
                    case '(' when _tokenMarker == TokenKind.AnnotationArgument:
                    case ',' when _tokenMarker == TokenKind.AnnotationArgument:
                    case '@' when _tokenMarker == TokenKind.None:
                        _tokenMarker = TokenKind.Annotation;
                        return LexAnnotation();
                    case '[' when _tokenMarker == TokenKind.None:
                        _tokenMarker = TokenKind.Section;
                        return LexSection();
                    case '/' when _tokenMarker == TokenKind.Section:
                        _tokenMarker = TokenKind.Regex;
                        break;
                    case Tab when _tokenMarker == TokenKind.Indent:
                    case Space when _tokenMarker == TokenKind.Indent:
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
            _tokenMarker = TokenKind.None;

            DetermineIndentations();

            var token = new Token(_scanner.CurrentPosition, TokenKind.Newline, SliceOne());

            _scanner.MoveNext();

            return token;

            void DetermineIndentations()
            {
                var offset = 1;

                if (_scanner.ReadAhead() is var nextChar && IsIndentChar(nextChar))
                {
                    nextChar = _scanner.ReadAhead(++offset);

                    if (IsIndentChar(nextChar)) _tokenMarker = TokenKind.Indent;
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

            _tokenMarker = TokenKind.None;

            return new Token(start, ch == Space ? TokenKind.Space : TokenKind.Tab, SliceFrom(start));
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

                    if (ch == '(' && _scanner.ReadAhead(offset) != ')')
                    {
                        _tokenMarker = TokenKind.AnnotationArgument;

                        _scanner.MoveAhead();
                    }

                    return token;
                }
                else if (!char.IsLetter(ch))
                {
                    return new Token(_scanner.CurrentPosition, TokenKind.Error, ReadOnlyMemory<char>.Empty);
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
