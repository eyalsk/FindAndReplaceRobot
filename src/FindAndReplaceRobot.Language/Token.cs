namespace FindAndReplaceRobot.Language
{
    using System;

    public sealed class Token
    {
        public Token(Range range, int depth, TokenKind kind, TokenKind context, ReadOnlyMemory<char> value)
        {
            if (range.Start.Value < 0) throw new ArgumentOutOfRangeException(nameof(range));
            if (range.End.Value < 0 || range.End.Value < range.Start.Value) throw new ArgumentOutOfRangeException(nameof(range));
            if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth));
            if (!Enum.IsDefined(typeof(TokenKind), kind)) throw new ArgumentException(nameof(kind));
            if (!Enum.IsDefined(typeof(TokenKind), context)) throw new ArgumentException(nameof(context));

            Range = range;
            Depth = depth;
            Kind = kind;
            Context = context;
            Value = value;
        }

        public Range Range { get; }

        public int Depth { get; }

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