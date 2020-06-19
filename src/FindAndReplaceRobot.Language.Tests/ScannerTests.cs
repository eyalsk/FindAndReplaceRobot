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
        public void Should_read_characters_to_end_of_file()
        {
            var scanner = new Scanner("a\r\nb\nc\n\nd");

            var results = new List<(
                char ch,
                int currentIndex)>();

            while (true)
            {
                var ch = scanner.Read();

                results.Add((
                    ch,
                    scanner.CurrentIndex));

                if (ch == EndOfFile) break;
            }

            results.ShouldBe(new[] {
                ('a', 0),
                ('\r', 1),
                ('\n', 2),
                ('b', 3),
                ('\n', 4),
                ('c', 5),
                ('\n', 6),
                ('\n', 7),
                ('d', 8),
                (EndOfFile, 9)
            });
        }

        [Fact]
        public void Peek_0_should_throw_InvalidOperationException_when_there_is_no_previous_Read()
        {
            var scanner = new Scanner("a\r\nb\nc\n\nd");

            var ex = Should.Throw<InvalidOperationException>(() => scanner.Peek(0));
            ex.Message.ShouldBe("Peek(0) returns the value of the previous read, but there was no previous read.");
        }

        [Fact]
        public void Peek_0_should_return_the_same_char_as_the_previous_Read_but_without_changing_state()
        {
            var scanner = new Scanner("a\r\nb\nc\n\nd");

            while (true)
            {
                var read = scanner.Read();

                var indexBeforePeek = scanner.CurrentIndex;
                scanner.Peek(0).ShouldBe(read);
                scanner.CurrentIndex.ShouldBe(indexBeforePeek);

                if (read == EndOfFile) break;
            }
        }

        [Fact]
        public void Peek_1_should_return_the_same_char_as_the_next_Read_but_without_changing_state()
        {
            var scanner = new Scanner("a\r\nb\nc\n\nd");

            while (true)
            {
                var indexBeforePeek = scanner.CurrentIndex;
                var peeked = scanner.Peek(1);
                scanner.CurrentIndex.ShouldBe(indexBeforePeek);

                peeked.ShouldBe(scanner.Read());

                if (peeked == EndOfFile) break;
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Should_peek_and_not_throw_when_offset_above_or_equal_to_zero(int offset)
        {
            var text = Randomizer.GenerateString("🎲");
            var scanner = new Scanner(text);
            scanner.Read();

            Should.NotThrow(() => scanner.Peek(offset));
        }

        [Fact]
        public void Should_peek_and_throw_when_offset_below_zero()
        {
            var text = Randomizer.GenerateString("🎲");
            var scanner = new Scanner(text);

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.Peek(-1));
        }

        [Theory]
        [InlineData(0, 'a')]
        [InlineData(1, 'b')]
        [InlineData(2, 'c')]
        [InlineData(3, EndOfFile)]
        public void Should_peek_to_offset(int offset, char result)
        {
            var scanner = new Scanner(text: "abc");
            scanner.Read();

            scanner.Peek(offset).ShouldBe(result);
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
    }
}