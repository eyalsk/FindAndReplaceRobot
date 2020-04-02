namespace FindAndReplaceRobot.Language
{
    using System;

    public struct Position
    {
        public Position(int index = 0, int lineNumber = 1, int columnNumber = 0, int level = 1)
        {
            if (index < 0) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (lineNumber < 1) {
                throw new ArgumentOutOfRangeException(nameof(lineNumber));
            }

            if (columnNumber < 0) {
                throw new ArgumentOutOfRangeException(nameof(columnNumber));
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            Index = index;

            LineNumber = lineNumber;

            ColumnNumber = columnNumber;

            Level = level;
        }

        public int ColumnNumber { get; }

        public int Index { get; }

        public int LineNumber { get; }

        public int Level { get; }
    }
}