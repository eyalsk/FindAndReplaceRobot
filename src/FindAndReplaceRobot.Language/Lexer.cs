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
                    case Tab:
                    case Space:
                        if (_marker == SectionMarker.Subsection)
                        {
                            SetSubsectionMarker();
                        }
                        else
                        {
                            SkipLeadingSpaces();
                        }
                        break;
                    case NewLine:
                        SetSectionMarker();
                        break;
                    case EndOfFile:
                        return CreateToken(
                                    _scanner.CurrentIndex.._scanner.CurrentIndex,
                                    TokenKind.EndOfFile,
                                    TokenKind.None,
                                    ReadOnlyMemory<char>.Empty);
                    default:
                        if (_marker == SectionMarker.Header || _marker == SectionMarker.Item)
                        {
                            if (LexItem() == TextEndingFlags.LF)
                            {
                                goto case NewLine;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        break;
                }

                _scanner.MoveNext();
            }
        }

        private Token LexAnnotation()
        {
            var start = _scanner.CurrentIndex;
            var spaceOffset = 0;

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (ch == '(' || IsNewLineOrEOF(ch))
                {
                    var end = _scanner.GetSliceEnding(start.._scanner.CurrentIndex) == TextEndingFlags.CR
                                ? _scanner.CurrentIndex - 1
                                : _scanner.CurrentIndex;

                    end -= spaceOffset;

                    var token = CreateToken(
                                    start..end,
                                    TokenKind.Annotation,
                                    TokenKind.None,
                                    _scanner.GetSlice((start + 1)..end));

                    if (ch == '(') LexAnnotationArguments();

                    return token;
                }
                else if (IsSpace(ch))
                {
                    spaceOffset++;
                }
                else if (!char.IsLetter(ch))
                {
                    // todo: Add error "Invalid annotation identifier at {position}. Annotation identifier can only contain letters."

                    spaceOffset = 0;
                }

                _scanner.MoveNext();
            }
        }

        private void LexAnnotationArguments()
        {
            var start = _scanner.CurrentIndex;
            var end = -1;
            var context = TokenKind.None;
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
                else if (ch == '(')
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
            var start = _scanner.CurrentIndex;

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (ch == ']')
                {
                    var token = CreateToken(
                                    start..(_scanner.CurrentIndex + 1),
                                    TokenKind.Section,
                                    TokenKind.None,
                                    _scanner.GetSlice((start + 1).._scanner.CurrentIndex));

                    _scanner.MoveNext();

                    return token;
                }
                else if (IsNewLineOrEOF(ch))
                {
                    // todo: Add error

                    var token = CreateToken(
                                    start..(_scanner.CurrentIndex + 1),
                                    TokenKind.Section,
                                    TokenKind.Error,
                                    _scanner.GetSlice((start + 1).._scanner.CurrentIndex));

                    _scanner.MoveNext();

                    return token;
                }

                _scanner.MoveNext();
            }
        }

        private TextEndingFlags LexItem()
        {
            var start = _scanner.CurrentIndex;
            var end = -1;
            var offset = 1;
            var spaceOffset = 0;
            var context = TokenKind.None;
            var hasOperator = false;

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (ch == '-' && _scanner.PeekAhead(ref offset) == '>')
                {
                    var range = _scanner.CurrentIndex..(_scanner.CurrentIndex + 2);

                    _pendingTokens.Enqueue(
                        CreateToken(
                            range,
                            TokenKind.Operator,
                            TokenKind.None,
                            _scanner.GetSlice(range)));

                    _scanner.StepTo(offset);

                    end = _scanner.CurrentIndex - 1;
                    context = TokenKind.LHS;
                    hasOperator = true;
                }
                else if (IsNewLineOrEOF(ch))
                {
                    end = _scanner.GetSliceEnding(start.._scanner.CurrentIndex) == TextEndingFlags.CR
                                ? _scanner.CurrentIndex - 1
                                : _scanner.CurrentIndex;

                    context = hasOperator ? TokenKind.RHS : TokenKind.LHS;
                }
                else if (ch == '"')
                {
                    // todo: Lex string
                }
                else if (ch == '/')
                {
                    // todo: Lex regex
                }
                else if (!IsSpace(ch))
                {
                    spaceOffset = 0;
                }
                else
                {
                    spaceOffset++;
                }

                if (start > -1 && (context == TokenKind.LHS || context == TokenKind.RHS))
                {
                    end -= spaceOffset;

                    _pendingTokens.Enqueue(
                        CreateToken(
                            start..end,
                            TokenKind.Value,
                            context,
                            _scanner.GetSlice(start..end)));

                    start = (hasOperator ? end + 3 : 0) + spaceOffset;
                    spaceOffset = 0;
                    context = TokenKind.None;
                }

                if (ch == NewLine)
                {
                    return TextEndingFlags.LF;
                }
                else if (ch == EndOfFile)
                {
                    return TextEndingFlags.EOF;
                }

                _scanner.MoveNext();
            }
        }

        private void SetSectionMarker()
        {
            var offset = 1;
            var ch = _scanner.PeekAhead(ref offset);
            var isBlank = _marker == SectionMarker.Blank;

            if (ch == '@' || ch == '[')
            {
                _marker = SectionMarker.Header;
            }
            else if (ch == NewLine)
            {
                _marker = SectionMarker.Blank;
            }
            else if (IsSpace(ch))
            {
                _marker = SectionMarker.Subsection;
            }
            else
            {
                _marker = SectionMarker.Item;
            }

            if (!isBlank && (_marker == SectionMarker.Header || _marker == SectionMarker.Item))
            {
                _nesting = (-1, 1);
            }
        }

        private void SetSubsectionMarker()
        {
            var offset = 1;
            var ch = SkipLeadingSpaces(ref offset);

            if (ch == '@')
            {
                _marker = SectionMarker.Header;
            }
            else if (IsNewLineOrEOF(ch))
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

                _scanner.StepTo(--offset);
            }
            else
            {
                _scanner.StepTo(offset);
            }
        }

        private char SkipLeadingSpaces(ref int offset)
        {
            var ch = _scanner.PeekAhead(ref offset);

            while (IsSpace(ch))
            {
                offset++;

                ch = _scanner.PeekAhead(ref offset);
            }

            return ch;
        }

        private void SkipLeadingSpaces()
        {
            var offset = 1;

            SkipLeadingSpaces(ref offset);

            _scanner.StepTo(offset);
        }

        private Token CreateToken(Range range, TokenKind kind, TokenKind context, ReadOnlyMemory<char> value) =>
            new Token(range, _nesting.depth, kind, context, value);
    }
}
