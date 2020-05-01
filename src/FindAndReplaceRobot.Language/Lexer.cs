namespace FindAndReplaceRobot.Language
{
    using System;
    using System.Collections.Generic;

    using FindAndReplaceRobot.Language.Tokens;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private readonly Queue<Token> _pendingTokens;
        private SectionMarker _marker = SectionMarker.Header;

        public Lexer(Scanner scanner)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _pendingTokens = new Queue<Token>();
        }

        private enum SectionMarker
        {
            None,
            Header,
            Subsection,
            Item
        }

        private static bool IsSpaceChar(char currentChar) =>
            currentChar == Space || currentChar == Tab;

        private static bool IsNewLineOrEOF(char ch) =>
            ch == NewLine || ch == EndOfFile;

        public Token ReadToken()
        {
            while (true)
            {
                if (_pendingTokens.Count > 0)
                {
                    return _pendingTokens.Dequeue();
                }

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
                        return CreateToken(TokenKind.EndOfLine, ReadOnlyMemory<char>.Empty);
                }

                _scanner.MoveNext();
            }
        }

        private Token LexNewLine()
        {
            SetSectionMarker();

            var token = CreateToken(TokenKind.NewLine, SliceOne());

            _scanner.MoveNext();

            return token;

            void SetSectionMarker()
            {
                var offset = 1;
                var nextChar = _scanner.PeekAhead(ref offset);

                if (IsSpaceChar(nextChar))
                {
                    offset++;
                    nextChar = _scanner.PeekAhead(ref offset);

                    if (IsSpaceChar(nextChar)) _marker = SectionMarker.Subsection;
                }
                else if (nextChar == '@' || nextChar == '[')
                {
                    _marker = SectionMarker.Header;
                }
                else
                {
                    _marker = SectionMarker.None;
                }
            }
        }

        private Token LexIndentation()
        {
            var ch = _scanner.ReadChar();
            var start = _scanner.CurrentIndex;
            var nextChar = _scanner.ReadAhead(2);
            _scanner.MoveAhead();

            while (IsSpaceChar(nextChar))
            {
                nextChar = _scanner.ReadAhead();
                _scanner.MoveAhead();
            }

            SetSectionMarker(nextChar);

            return CreateToken(start, ch == Space ? TokenKind.Space : TokenKind.Tab, SliceFrom(start));

            void SetSectionMarker(char nextChar) =>
                _marker = nextChar == '@' ? SectionMarker.Header : SectionMarker.Item;
        }

        private Token LexAnnotation()
        {
            while (true)
            {
                var ch = _scanner.ReadAhead();

                if (ch == '(' || IsNewLineOrEOF(ch))
                {
                    var token = CreateToken(TokenKind.Annotation, SkipFirstSliceRest());

                    _scanner.MoveAhead();

                    LexAnnotationArguments();

                    return token;
                }
                else if (!char.IsLetter(ch))
                {
                    // todo: Add error "Invalid annotation identifier at {position}. Annotation identifier can only contain letters."

                    _scanner.MoveAhead();
                }
            }
        }

        private void LexAnnotationArguments()
        {
            var start = _scanner.CurrentIndex;
            var end = _scanner.AbsoluteIndex;
            int openingCharCount = 0;
            char? closingChar = null;

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (closingChar is null && (ch == '[' || ch == '"'))
                {
                    start = _scanner.CurrentIndex;
                    closingChar = ch;
                }
                else if ((closingChar == '[' && ch == ']') || (closingChar == '"' && ch == '"'))
                {
                    end = _scanner.CurrentIndex + 1;
                    closingChar = null;
                }
                else if (closingChar is null && (ch == ',' || ch == ')'))
                {
                    _scanner.MoveNext();

                    if (end > start)
                    {
                        _pendingTokens.Enqueue(
                            CreateToken(
                                start,
                                end,
                                TokenKind.AnnotationArgument,
                                _scanner.GetSlice(start..end)));
                    }
                    else
                    {
                        break;
                    }

                    if (ch == ')') closingChar = ch;
                }
                else if (ch == '(' && ++openingCharCount > 1)
                {
                    // todo: Add error "Annotation at '{position}' contains illegal opening parenthesis."
                }
                else if (IsNewLineOrEOF(ch))
                {
                    if (closingChar is null)
                    {
                        // todo: Add error "Annotation at '{position}' does not contain closing parenthesis."
                    }

                    break;
                }
                else if (closingChar is object && ch == '[')
                {
                    // todo: Add error "Annotation argument contains unescaped opening bracket at '{position}'."
                }
                else if (closingChar is null && !IsSpaceChar(ch))
                {
                    // todo: Add error "Annotation argument contains unquoted characters at '{position}'."
                }

                _scanner.MoveNext();
            }
        }

        private Token LexSection()
        {
            while (true)
            {
                var ch = _scanner.ReadAhead();

                if (ch == ']')
                {
                    var token = CreateToken(
                        _scanner.CurrentIndex,
                        _scanner.AbsoluteIndex + 1,
                        TokenKind.Section,
                        SkipFirstSliceRest());

                    _scanner.MoveAhead();

                    return token;
                }
                else if (IsNewLineOrEOF(ch))
                {
                    _scanner.MoveAhead();

                    // todo: Add error

                    return CreateToken(TokenKind.Error, ReadOnlyMemory<char>.Empty);
                }
            }
        }

        private Token CreateToken(TokenKind kind, ReadOnlyMemory<char> value) =>
            new Token(_scanner.CurrentIndex, _scanner.AbsoluteIndex, kind, value);

        private Token CreateToken(int start, TokenKind kind, ReadOnlyMemory<char> value) =>
            new Token(start, _scanner.AbsoluteIndex, kind, value);

        private Token CreateToken(int start, int end, TokenKind kind, ReadOnlyMemory<char> value) =>
            new Token(start, end, kind, value);

        private ReadOnlyMemory<char> SliceOne() => 
            _scanner.GetSlice(_scanner.CurrentIndex..(_scanner.CurrentIndex + 1));

        private ReadOnlyMemory<char> SliceFrom(Index start) => 
            _scanner.GetSlice(start.._scanner.AbsoluteIndex);

        private ReadOnlyMemory<char> SkipFirstSliceRest() => 
            _scanner.GetSlice((_scanner.CurrentIndex + 1).._scanner.AbsoluteIndex);
    }
}
