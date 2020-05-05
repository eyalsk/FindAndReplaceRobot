namespace FindAndReplaceRobot.Language
{
    using System;
    using System.Collections.Generic;

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

        [Flags]
        private enum SectionMarker
        {
            None,
            Header = 1 << 0,
            Section = 1 << 1,
            Subsection = 1 << 2,
            Item = 1 << 3,
            Space = 1 << 4,
            Tab = 1 << 5
        }

        private static bool IsSpace(char ch) =>
            ch == Space || ch == Tab;

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

                // todo: It's an error to mix tabs and spaces so we should handle this here.

                switch (_scanner.ReadChar())
                {
                    case '@' when _marker == SectionMarker.Header:
                        return LexAnnotation();
                    case '[' when _marker == SectionMarker.Header:
                        return LexSection();
                    case Tab when (_marker & SectionMarker.Subsection) != 0:
                    case Space when (_marker & SectionMarker.Subsection) != 0:
                        return LexSubsection();
                    case NewLine:
                        SetSectionMarker();
                        break;
                    case EndOfFile:
                        return CreateToken(TokenKind.EndOfFile, ReadOnlyMemory<char>.Empty);
                }

                _scanner.MoveNext();
            }

            void SetSectionMarker()
            {
                var offset = 1;
                var nextChar = _scanner.PeekAhead(ref offset);

                if (nextChar == Space)
                {
                    offset++;
                    nextChar = _scanner.PeekAhead(ref offset);

                    if (nextChar == Space) _marker |= SectionMarker.Subsection | SectionMarker.Space;
                }
                else if (nextChar == Tab)
                {
                    _marker |= SectionMarker.Subsection | SectionMarker.Tab;
                }
                else if (nextChar == '@' || nextChar == '[')
                {
                    _marker = SectionMarker.Header;
                }
                else if ((_marker & (SectionMarker.Header | SectionMarker.Section | SectionMarker.Item)) != 0)
                {
                    _marker = SectionMarker.Item;
                }
                else
                {
                    _marker = SectionMarker.None;
                }
            }
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
            var context = TokenKind.None;
            int openingCharCount = 0;
            char? closingChar = null;

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (closingChar is null && (ch == '[' || ch == '"'))
                {
                    start = _scanner.CurrentIndex;

                    if (ch == '[')
                    {
                        context = TokenKind.Section;
                        closingChar = ']';
                    }
                    else
                    {
                        context = TokenKind.String;
                        closingChar = '"';
                    }
                }
                else if (closingChar == ch)
                {
                    if (closingChar == ')')
                    {
                        // todo: Add error "Annotation at '{position}' contains illegal closing parenthesis."
                    }
                    else
                    {
                        end = _scanner.CurrentIndex + 1;
                        closingChar = null;
                    }
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
                                context,
                                _scanner.GetSlice((start + 1)..(end - 1))));

                        context = TokenKind.None;
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
                else if (closingChar is null && !IsSpace(ch))
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
                        TokenKind.None,
                        SkipFirstSliceRest());

                    _scanner.MoveAhead();

                    _marker = SectionMarker.Section;

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

        private Token LexSubsection()
        {
            var ch = _scanner.ReadChar();
            var start = _scanner.CurrentIndex;
            var spaceOffset = (_marker & SectionMarker.Space) != 0 ? 2 : 1;
            var nextChar = _scanner.ReadAhead(spaceOffset);
            _scanner.MoveAhead();

            while (IsSpace(nextChar))
            {
                nextChar = _scanner.ReadAhead();
                _scanner.MoveAhead();
            }

            _marker = nextChar == '@' ? SectionMarker.Header : SectionMarker.Item;

            return CreateToken(start, TokenKind.Indent, ch == Space ? TokenKind.Space : TokenKind.Tab, SliceFrom(start));
        }

        private Token CreateToken(TokenKind kind, ReadOnlyMemory<char> value) =>
            new Token(_scanner.CurrentIndex, _scanner.AbsoluteIndex, kind, TokenKind.None, value);

        private Token CreateToken(int start, TokenKind kind, TokenKind context, ReadOnlyMemory<char> value) =>
            new Token(start, _scanner.AbsoluteIndex, kind, context, value);

        private Token CreateToken(int start, int end, TokenKind kind, TokenKind context, ReadOnlyMemory<char> value) =>
            new Token(start, end, kind, context, value);

        private ReadOnlyMemory<char> SliceFrom(Index start) =>
            _scanner.GetSlice(start.._scanner.AbsoluteIndex);

        private ReadOnlyMemory<char> SkipFirstSliceRest() =>
            _scanner.GetSlice((_scanner.CurrentIndex + 1).._scanner.AbsoluteIndex);
    }
}
