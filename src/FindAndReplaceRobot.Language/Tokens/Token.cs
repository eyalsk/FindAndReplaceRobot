namespace FindAndReplaceRobot.Language.Tokens
{
    using System;
    using System.Collections.Immutable;

    public sealed class Token
    {
        public Token(int start, TokenKind kind, ReadOnlyMemory<char> value, ImmutableArray<Token> tokens = default)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (!Enum.IsDefined(typeof(TokenKind), kind)) throw new ArgumentException(nameof(kind));

            Start = start;
            Kind = kind;
            Value = value;
            Tokens = tokens;
        }

        public int Start { get; }

        public int Length => Value.Length;

        public TokenKind Kind { get; }

        public ReadOnlyMemory<char> Value { get; }

        public ImmutableArray<Token> Tokens { get; }
    }
}
