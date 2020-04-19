namespace FindAndReplaceRobot.Language.Tokens
{
    using System;

    public sealed class Token
    {
        public Token(int start, TokenKind kind, ReadOnlyMemory<char> value)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (!Enum.IsDefined(typeof(TokenKind), kind)) throw new ArgumentException(nameof(kind));

            Start = start;
            Kind = kind;
            Value = value;
        }

        public int Start { get; }

        public int Length => Value.Length;

        public TokenKind Kind { get; }

        public ReadOnlyMemory<char> Value { get; }

        public override string ToString() =>
            Kind == TokenKind.Space ||
            Kind == TokenKind.Tab ||
            Kind == TokenKind.Newline ||
            Kind == TokenKind.EndOfLine ? string.Empty : Value.ToString();
    }
}
