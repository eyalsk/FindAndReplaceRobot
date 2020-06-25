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
                var ch = scanner.Peek();

                results.Add((
                    ch,
                    scanner.CurrentIndex));

                if (ch == EndOfFile) break;

                scanner.Consume();
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