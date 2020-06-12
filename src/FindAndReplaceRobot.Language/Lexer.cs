namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly static Func<char, char, bool> _isNextCharSame =
            (currentChar, nextChar) => currentChar == nextChar;

        private readonly static Func<char, char, bool> _isNextCharNotNewLineOrEOF =
            (_, nextChar) => !IsCharNewLineOrEOF(nextChar);

        private readonly Scanner _scanner;
        private TokenKind _prevKind;

        public Lexer(Scanner scanner)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        }

        private static bool IsCharNewLineOrEOF(char ch) =>
            ch == NewLine || ch == EndOfFile;

        private static bool IsCharIdentifier(char ch) =>
            char.IsLetterOrDigit(ch);

        private static bool IsCharLabel(char ch) =>
            ch == Space || IsCharIdentifier(ch);

        public Token ReadToken()
        {
            while (true)
            {
                switch (_scanner.ReadChar())
                {
                    case '@' when _prevKind != TokenKind.AtSign:
                        return CreateToken(TokenKind.AtSign);
                    case '(' when _prevKind != TokenKind.OpenParens:
                        return CreateToken(TokenKind.OpenParens);
                    case ')' when _prevKind != TokenKind.CloseParens:
                        return CreateToken(TokenKind.CloseParens);
                    case ',' when _prevKind != TokenKind.Comma:
                        return CreateToken(TokenKind.Comma);
                    case '[':
                        return LexQuotedLiteral(TokenKind.Label);
                    case '"':
                        return LexQuotedLiteral(TokenKind.String);
                    case '/':
                        return LexQuotedLiteral(TokenKind.Regex);
                    case Tab:
                    case Space:
                        // NYI
                        break;
                    case NewLine:
                        // NYI
                        break;
                    case EndOfFile:
                        return new Token(
                            _scanner.CurrentIndex.._scanner.CurrentIndex,
                            TokenKind.EndOfFile,
                            ReadOnlyMemory<char>.Empty);
                    default:
                        if (_prevKind == TokenKind.AtSign)
                        {
                            return LexIdentifier();
                        }
                        else
                        {
                            // NYI: return LexLiteral();
                        }
                        break;
                }

                _scanner.MoveNext();
            }

            Token CreateToken(TokenKind kind)
            {
                var range = _scanner.CurrentIndex..(_scanner.CurrentIndex + 1);

                _prevKind = kind;

                _scanner.MoveNext();

                return new Token(
                        range,
                        kind,
                        _scanner.GetSlice(range));
            }
        }

        private Token LexIdentifier()
        {
            var start = _scanner.CurrentIndex;
            bool isError;

            while (true)
            {
                var ch = _scanner.ReadChar();
                var isNewLineOrEOF = IsCharNewLineOrEOF(ch);

                if (isNewLineOrEOF || !IsCharIdentifier(ch))
                {
                    isError = !isNewLineOrEOF;

                    break;
                }

                _scanner.MoveNext();
            }

            var end = _scanner.CurrentIndex;

            if (isError)
            {
                end++;
            }
            else
            {
                end = _scanner.GetSliceEnding(start..end) == TextEndingFlags.CR
                        ? end - 1
                        : end;
            }

            _scanner.MoveNext();

            return new Token(
                        start..end,
                        isError ? TokenKind.Error : TokenKind.Identifier,
                        _scanner.GetSlice(start..end));
        }

        private Token LexQuotedLiteral(TokenKind kind)
        {
            var start = _scanner.CurrentIndex;
            var isError = false;
            char? closingChar = null;

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (kind == TokenKind.Label)
                {
                    if (ch == '[')
                    {
                        CheckNextChar(ch, _isNextCharSame);
                    }
                    else if (ch == ']')
                    {
                        CheckNextChar(ch, _isNextCharNotNewLineOrEOF);

                        break;
                    }
                    else if (!IsCharLabel(ch))
                    {
                        isError = true;
                    }
                }
                else if (kind == TokenKind.String)
                {
                    if (!HandleNextChar(ch, '"')) break;
                }
                else if (kind == TokenKind.Regex)
                {
                    if (!HandleNextChar(ch, '/')) break;
                }
                else if (IsCharNewLineOrEOF(ch))
                {
                    isError = true;
                }

                if (isError) break;

                _scanner.MoveNext();
            }

            var end = _scanner.CurrentIndex;
            var slice = (start, end);
            var isEmpty = start == end;

            if (end < _scanner.TextLength) end++;

            if (isError)
            {
                slice.end = end;
            }
            else
            {
                slice.start = isEmpty ? slice.end : slice.start + 1;
            }

            _scanner.MoveNext();

            return new Token(
                start..end,
                isError ? TokenKind.Error : kind,
                _scanner.GetSlice(slice.start..slice.end));

            void CheckNextChar(char ch, Func<char, char, bool> condition)
            {
                var offset = 1;
                var nextChar = _scanner.PeekAhead(ref offset);

                if (condition(ch, nextChar))
                {
                    isError = true;

                    _scanner.StepTo(offset);
                }
            }

            bool HandleNextChar(char ch, char quotedChar)
            {
                if (closingChar is null && ch == quotedChar)
                {
                    closingChar = ch;
                }
                else if (closingChar is object && ch == quotedChar)
                {
                    var offset = 1;
                    var nextChar = _scanner.PeekAhead(ref offset);

                    if(nextChar == ch || nextChar == '\\' || nextChar == Space || IsCharNewLineOrEOF(nextChar)) return false;
                }
                else if (ch == EndOfFile)
                {
                    isError = true;
                }

                return true;
            }
        }
    }
}