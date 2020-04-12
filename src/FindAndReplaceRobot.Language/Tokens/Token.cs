namespace FindAndReplaceRobot.Language.Tokens
{
    using System;

    public sealed class Token
    {
        public Token(int start, TokenKind kind, ReadOnlyMemory<char> value)
        {
            Start = start;
            Kind = kind;
            Value = value;
        }

        public int Start { get; }

        public int Length => Value.Length;

        public TokenKind Kind { get; }

        public ReadOnlyMemory<char> Value { get; }

        public override string ToString() => $"{Start} {Length} {Kind} {Value}";
    }
}
