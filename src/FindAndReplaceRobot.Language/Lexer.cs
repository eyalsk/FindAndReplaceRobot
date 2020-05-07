namespace FindAndReplaceRobot.Language
{
    using System;
    using System.Collections.Generic;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private readonly Queue<Token> _pendingTokens;
        private (int column, int depth) _nesting = (-1, 1);
        private SectionMarker _marker = SectionMarker.Header;

        public Lexer(Scanner scanner)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _pendingTokens = new Queue<Token>();
        }

        private enum SectionMarker
        {
            Blank,
            Header,
            Subsection,
            Item
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
                    case Tab when _marker == SectionMarker.Subsection:
                    case Space when _marker == SectionMarker.Subsection:
                        SetSubsectionMarker();
                        break;
                    case NewLine:
                        SetSectionMarker();
                        break;
                    case EndOfFile:
                        return CreateToken(TokenKind.EndOfFile, ReadOnlyMemory<char>.Empty);
                }

                _scanner.MoveNext();
            }
        }

        private Token LexAnnotation()
        {
            while (true)
            {
                var ch = _scanner.ReadAhead();

                if (ch == '(' || IsNewLineOrEOF(ch))
                {
                    var value = SkipFirstSliceRest(out var handledCRLF);
                    var end = handledCRLF ? _scanner.AbsoluteIndex - 1 : _scanner.AbsoluteIndex;
                    var token = CreateToken(_scanner.CurrentIndex..end, TokenKind.Annotation, TokenKind.None, value);

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
                                start..end,
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
                        _scanner.CurrentIndex..(_scanner.AbsoluteIndex + 1),
                        TokenKind.Section,
                        TokenKind.None,
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

        private void SetSectionMarker()
        {
            var offset = 1;
            var nextChar = _scanner.PeekAhead(ref offset);

            if (nextChar == '@' || nextChar == '[')
            {
                _marker = SectionMarker.Header;
            }
            else if (nextChar == NewLine)
            {
                _marker = SectionMarker.Blank;
            }
            else if (_marker == SectionMarker.Item && IsSpace(nextChar))
            {
                _marker = SectionMarker.Subsection;
            }
            else
            {
                _marker = SectionMarker.Item;
            }

            if (_marker == SectionMarker.Header || _marker == SectionMarker.Item)
            {
                _nesting = (-1, 1);
            }
        }

        private void SetSubsectionMarker()
        {
            var offset = 1;
            var nextChar = _scanner.PeekAhead(ref offset);

            while (IsSpace(nextChar))
            {
                offset++;
                nextChar = _scanner.PeekAhead(ref offset);
            }

            if (nextChar == '@')
            {
                _marker = SectionMarker.Header;
            }
            else if (IsNewLineOrEOF(nextChar))
            {
                _marker = SectionMarker.Blank;
            }
            else
            {
                _marker = SectionMarker.Item;
            }

            if (_marker != SectionMarker.Blank)
            {
                if (_nesting.column < _scanner.Position.ColumnNumber)
                {
                    _nesting.depth++;
                }
                else if (_nesting.column > _scanner.Position.ColumnNumber)
                {
                    _nesting.depth--;
                }

                _nesting.column = _scanner.Position.ColumnNumber;

                _scanner.StepAhead(--offset);
            }
            else
            {
                _scanner.StepAhead(offset);
            }
        }

        private Token CreateToken(TokenKind kind, ReadOnlyMemory<char> value) =>
            new Token(_scanner.CurrentIndex.._scanner.AbsoluteIndex, _nesting.depth, kind, TokenKind.None, value);

        private Token CreateToken(Range range, TokenKind kind, TokenKind context, ReadOnlyMemory<char> value) =>
            new Token(range, _nesting.depth, kind, context, value);

        private ReadOnlyMemory<char> SkipFirstSliceRest() =>
            _scanner.GetSlice((_scanner.CurrentIndex + 1).._scanner.AbsoluteIndex);

        private ReadOnlyMemory<char> SkipFirstSliceRest(out bool handledCRLF) =>
            _scanner.GetSlice((_scanner.CurrentIndex + 1).._scanner.AbsoluteIndex, out handledCRLF);
    }
}
