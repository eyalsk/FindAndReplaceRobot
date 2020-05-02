namespace FindAndReplaceRobot.Language
{
    using System;

    public sealed class Token
    {
        public Token(int start, int end, TokenKind kind, TokenKind context, ReadOnlyMemory<char> value)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (end < 0 || end < start) throw new ArgumentOutOfRangeException(nameof(end));
            if (!Enum.IsDefined(typeof(TokenKind), kind)) throw new ArgumentException(nameof(kind));
            if (!Enum.IsDefined(typeof(TokenKind), context)) throw new ArgumentException(nameof(context));

            Start = start;
            End = end;
            Kind = kind;
            Context = context;
            Value = value;
        }

        public int Start { get; }

        public int End { get; }

        public TokenKind Kind { get; }

        public TokenKind Context { get; }

        public ReadOnlyMemory<char> Value { get; }

        public override string ToString() =>
            Kind == TokenKind.Space ||
            Kind == TokenKind.Tab ||
            Kind == TokenKind.NewLine ||
            Kind == TokenKind.EndOfFile ? string.Empty : Value.ToString();
    }
}