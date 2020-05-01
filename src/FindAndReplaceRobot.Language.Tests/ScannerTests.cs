namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    using Shouldly;

    using static InvisibleCharacters;

    internal class ScannerTests
    {
        [Test]
        public void Should_read_ahead_and_not_throw_when_start_above_zero()
        {
            var scanner = new Scanner("XY");

            Should.NotThrow(() => scanner.ReadAhead(start: 1));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Should_read_ahead_and_throw_when_start_below_or_equal_to_zero(int start)
        {
            var scanner = new Scanner("XY");

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.ReadAhead(start));
        }

        [Test]
        public void Should_peek_ahead_and_not_throw_when_offset_above_zero()
        {
            var scanner = new Scanner("XY");
            var offset = 1;

            Should.NotThrow(() => scanner.PeekAhead(ref offset));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Should_peek_ahead_and_throw_when_offset_below_or_equal_to_zero(int offset)
        {
            var scanner = new Scanner("XY");

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.PeekAhead(ref offset));
        }

        [Test]
        public void Should_read_ahead_and_move_ahead_to_3rd_character()
        {
            var scanner = new Scanner("XYZ");

            scanner.ReadAhead(2);
            scanner.MoveAhead();
            scanner.ReadChar().ShouldBe('Z');
        }

        [Test]
        public void Should_peek_ahead_to_3rd_character()
        {
            var scanner = new Scanner("XYZ");
            var offset = 2;

            scanner.PeekAhead(ref offset).ShouldBe('Z');
        }

        [Test]
        public void Should_read_characters_to_end_of_file()
        {
            var scanner = new Scanner("a\r\nb\nc\n\nd");

            var results = new List<(
                char ch,
                int currentIndex,
                int absoluteIndex,
                Position pos)>();

            while (true)
            {
                var ch = scanner.ReadChar();

                results.Add((
                    ch,
                    scanner.CurrentIndex,
                    scanner.AbsoluteIndex,
                    scanner.Position));

                scanner.MoveNext();

                if (ch == EndOfFile) break;
            }

            results.ShouldBe(new[] {
                ('a', 0, 0, new Position(1, 1)),
                ('\n', 2, 2, new Position(2, 0)),
                ('b', 3, 3, new Position(2, 1)),
                ('\n', 4, 4, new Position(3, 0)),
                ('c', 5, 5, new Position(3, 1)),
                ('\n', 6, 6, new Position(4, 0)),
                ('\n', 7, 7, new Position(5, 0)),
                ('d', 8, 8, new Position(5, 1)),
                (EndOfFile, 9, 9, new Position(5, 2))
            });
        }

        [Test]
        public void Should_read_ahead_to_end_of_file()
        {
            var scanner = new Scanner("a\r\nb\nc!ab\n\nc");

            var results = new List<(
                char ch,
                int currentIndex,
                int absoluteIndex,
                Position pos)>();

            while (true)
            {
                var ch = scanner.ReadAhead();

                results.Add((
                    ch,
                    scanner.CurrentIndex,
                    scanner.AbsoluteIndex,
                    scanner.Position));

                if (ch == '!') scanner.MoveAhead();

                if (ch == EndOfFile) break;
            }

            results.ShouldBe(new[] {
                ('\n', 0, 2, new Position(2, 0)),
                ('b', 0, 3, new Position(2, 1)),
                ('\n', 0, 4, new Position(3, 0)),
                ('c', 0, 5, new Position(3, 1)),
                ('!', 0, 6, new Position(3, 2)),
                ('a', 6, 7, new Position(3, 3)),
                ('b', 6, 8, new Position(3, 4)),
                ('\n', 6, 9, new Position(4, 0)),
                ('\n', 6, 10, new Position(5, 0)),
                ('c', 6, 11, new Position(5, 1)),
                (EndOfFile, 6, 12, new Position(5, 2))
            });
        }

        [Test]
        public void Should_peek_ahead_to_end_of_file()
        {
            var scanner = new Scanner("a\r\nbc\n");

            var results = new List<(
                char ch,
                int currentIndex,
                int absoluteIndex,
                Position pos)>();

            var offset = 1;

            while (true)
            {
                var ch = scanner.PeekAhead(ref offset);

                results.Add((
                    ch,
                    scanner.CurrentIndex,
                    scanner.AbsoluteIndex,
                    scanner.Position));

                offset++;

                if (ch == EndOfFile) break;
            }

            results.ShouldBe(new[] {
                ('\n', 0, 0, new Position(2, 0)),
                ('b', 0, 0, new Position(2, 1)),
                ('c', 0, 0, new Position(2, 2)),
                ('\n', 0, 0, new Position(3, 0)),
                (EndOfFile, 0, 0, new Position(3, 1))
            });
        }
    }
}