namespace FindAndReplaceRobot.Language.Tokens
{
    using System;

    public sealed class Token
    {
        public Token(int start, int end, TokenKind kind, ReadOnlyMemory<char> value)
        {
            Start = start;
            End = end;
            Kind = kind;
            Value = value;
        }

        public int Start { get; }

        public int End { get; }

        public TokenKind Kind { get; }

        public ReadOnlyMemory<char> Value { get; }
    }
}
