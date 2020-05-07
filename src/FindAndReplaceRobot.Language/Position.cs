namespace FindAndReplaceRobot.Language
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public readonly struct Position : IEquatable<Position>
    {
        public Position(int lineNumber, int columnNumber)
        {
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public int ColumnNumber { get; }

        public int LineNumber { get; }

        public Position NextColumn() =>
            new Position(LineNumber, ColumnNumber + 1);

        public Position NextLine() =>
            new Position(LineNumber + 1, 0);

        public bool Equals([AllowNull] Position other) =>
            (LineNumber == other.LineNumber) &&
            (ColumnNumber == other.ColumnNumber);

        public override bool Equals(object? other) =>
            other is Position position && Equals(position);

        public override int GetHashCode() =>
            HashCode.Combine(LineNumber, ColumnNumber);

        public static bool operator ==(Position left, Position right) =>
            Equals(left, right);

        public static bool operator !=(Position left, Position right) =>
            !Equals(left, right);

        public override string ToString() =>
            $"{LineNumber} {ColumnNumber}";
    }
}