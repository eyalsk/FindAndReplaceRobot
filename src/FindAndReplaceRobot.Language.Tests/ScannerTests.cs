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
        public void Should_read_ahead_to_2nd_character()
        {
            var scanner = new Scanner("XY");

            scanner.ReadAhead(offset: 1).ShouldBe('Y');
        }

        [TestCase(2)]
        [TestCase(3)]
        public void Should_read_ahead_to_end_of_file(int offset)
        {
            var scanner = new Scanner("XY");

            scanner.ReadAhead(offset).ShouldBe(EndOfFile);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Should_read_ahead_and_not_throw_when_offset_above_or_equal_to_zero(int offset)
        {
            var scanner = new Scanner("XY");

            Should.NotThrow(() => scanner.ReadAhead(offset));
        }

        [Test]
        public void Should_read_ahead_and_throw_when_offset_below_zero()
        {
            var scanner = new Scanner("XY");

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.ReadAhead(offset: -1));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Should_peek_ahead_and_not_throw_when_offset_above_or_equal_to_zero(int offset)
        {
            var scanner = new Scanner("XY");

            Should.NotThrow(() => scanner.PeekAhead(offset));
        }

        [Test]
        public void Should_peek_ahead_and_throw_when_offset_below_zero()
        {
            var scanner = new Scanner("XY");

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.PeekAhead(offset: -1));
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

            scanner.PeekAhead(2).ShouldBe('Z');
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
                ('a', 0, 0, new Position(0, 1, 1)),
                ('\n', 2, 2, new Position(2, 2, 0)),
                ('b', 3, 3, new Position(3, 2, 1)),
                ('\n', 4, 4, new Position(4, 3, 0)),
                ('c', 5, 5, new Position(5, 3, 1)),
                ('\n', 6, 6, new Position(6, 4, 0)),
                ('\n', 7, 7, new Position(7, 5, 0)),
                ('d', 8, 8, new Position(8, 5, 1)),
                (EndOfFile, 9, 9, new Position(9, 5, 2))
            });
        }
    }
}