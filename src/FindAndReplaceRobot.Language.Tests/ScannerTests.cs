namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using System.Collections.Generic;

    using FindAndReplaceRobot.Language.Tests.Utils;

    using Shouldly;

    using Xunit;

    using static InvisibleCharacters;

    public sealed class ScannerTests
    {
        [Fact]
        public void Should_peek_ahead_and_not_throw_when_start_above_zero()
        {
            var text = Randomizer.GenerateString("🎲");
            var scanner = new Scanner(text);
            var offset = 1;

            Should.NotThrow(() => scanner.PeekAhead(ref offset));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_peek_ahead_and_throw_when_offset_below_or_equal_to_zero(int offset)
        {
            var text = Randomizer.GenerateString("🎲");
            var scanner = new Scanner(text);

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.PeekAhead(ref offset));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        public void Should_step_to_and_not_throw_when_offset_above_or_equal_zero(int offset)
        {
            var text = Randomizer.GenerateString("🎲");
            var scanner = new Scanner(text);

            Should.NotThrow(() => scanner.StepTo(offset));
        }

        [Fact]
        public void Should_step_to_and_throw_when_offset_below_or_equal_to_zero()
        {
            var text = Randomizer.GenerateString("🎲");
            var scanner = new Scanner(text);

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.StepTo(offset: -1));
        }

        [Fact]
        public void Should_peek_ahead_and_step_to_3rd_character()
        {
            var text = Randomizer.GenerateString("🎲XZ", 2);
            var scanner = new Scanner(text);
            var offset = 2;

            scanner.PeekAhead(ref offset);
            scanner.StepTo(offset);
            scanner.ReadChar().ShouldBe('X');
        }

        [Fact]
        public void Should_peek_ahead_to_3rd_character()
        {
            var text = Randomizer.GenerateString("🎲X", 2);
            var scanner = new Scanner(text);
            var offset = 2;

            scanner.PeekAhead(ref offset).ShouldBe('X');
        }

        [Fact]
        public void Should_read_characters_to_end_of_file()
        {
            var scanner = new Scanner("a\r\nb\nc\n\nd");

            var results = new List<(
                char ch,
                int currentIndex,
                Position pos)>();

            while (true)
            {
                var ch = scanner.ReadChar();

                results.Add((
                    ch,
                    scanner.CurrentIndex,
                    scanner.Position));

                scanner.MoveNext();

                if (ch == EndOfFile) break;
            }

            results.ShouldBe(new[] {
                ('a', 0, new Position(1, 1)),
                ('\n', 2, new Position(2, 0)),
                ('b', 3, new Position(2, 1)),
                ('\n', 4, new Position(3, 0)),
                ('c', 5, new Position(3, 1)),
                ('\n', 6, new Position(4, 0)),
                ('\n', 7, new Position(5, 0)),
                ('d', 8, new Position(5, 1)),
                (EndOfFile, 9, new Position(5, 2))
            });
        }

        [Fact]
        public void Should_peek_ahead_to_end_of_file()
        {
            var scanner = new Scanner("a\r\nbc\n");

            var results = new List<(
                char ch,
                Position pos)>();

            var offset = 1;

            while (true)
            {
                var ch = scanner.PeekAhead(ref offset);

                results.Add((
                    ch,
                    scanner.Position));

                offset++;

                if (ch == EndOfFile) break;
            }

            results.ShouldBe(new[] {
                ('\n', new Position(2, 0)),
                ('b', new Position(2, 1)),
                ('c', new Position(2, 2)),
                ('\n', new Position(3, 0)),
                (EndOfFile, new Position(3, 1))
            });
        }

        [Theory]
        [InlineData("a", 0, 1)]
        [InlineData("ab", 0, 2)]
        [InlineData("b", 1, 2)]
        public void Should_get_slice(string slice, int start, int end)
        {
            var scanner = new Scanner("ab");
            var results = scanner.GetSlice(start..end).ToString();

            results.ShouldBe(slice);
        }

        [Theory]
        [InlineData("🎲", TextEndingFlags.None)]
        [InlineData("🎲\r", TextEndingFlags.CR)]
        [InlineData("🎲\r\n", TextEndingFlags.CRLF)]
        [InlineData("🎲\n", TextEndingFlags.LF)]
        public void Should_get_slice_ending(string pattern, TextEndingFlags textEnding)
        {
            var text = Randomizer.GenerateString(pattern);
            var scanner = new Scanner(text);
            var results = scanner.GetSliceEnding(..);

            results.ShouldBe(textEnding);
        }
    }
}