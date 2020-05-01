namespace FindAndReplaceRobot.Language.Tokens
{
    using System;

    public sealed class Token
    {
        public Token(int start, int end, TokenKind kind, ReadOnlyMemory<char> value)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (end < 0 || end < start) throw new ArgumentOutOfRangeException(nameof(end));
            if (!Enum.IsDefined(typeof(TokenKind), kind)) throw new ArgumentException(nameof(kind));

            Start = start;
            End = end;
            Kind = kind;
            Value = value;
        }

        public int Start { get; }

        public int End { get; }

        public TokenKind Kind { get; }

        public ReadOnlyMemory<char> Value { get; }

        public override string ToString() =>
            Kind == TokenKind.Space ||
            Kind == TokenKind.Tab ||
            Kind == TokenKind.NewLine ||
            Kind == TokenKind.EndOfFile ? string.Empty : Value.ToString();
    }
}