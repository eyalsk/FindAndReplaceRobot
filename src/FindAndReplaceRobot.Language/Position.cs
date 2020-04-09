[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FindAndReplaceRobot.Language.Driver")]

namespace FindAndReplaceRobot.Language
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    internal readonly struct Position : IEquatable<Position>
    {
        public Position(int index = 0, int lineNumber = 1, int columnNumber = 1)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (lineNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lineNumber));
            }

            if (columnNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columnNumber));
            }

            Index = index;

            LineNumber = lineNumber;

            ColumnNumber = columnNumber;
        }

        public int ColumnNumber { get; }

        public int Index { get; }

        public int LineNumber { get; }

        public bool Equals([AllowNull] Position other) =>
            (Index == other.Index) &&
            (LineNumber == other.LineNumber) &&
            (ColumnNumber == other.ColumnNumber);

        public override bool Equals(object? other) => other is Position position && Equals(position);

        public override int GetHashCode() => HashCode.Combine(Index, LineNumber, ColumnNumber);

        public static bool operator ==(Position left, Position right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !Equals(left, right);
        }
    }
}