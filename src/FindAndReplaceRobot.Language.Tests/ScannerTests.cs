namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using NUnit.Framework;
    using Shouldly;

    internal class ScannerTests
    {
        [Test]
        public void Should_read_1st_character()
        {
            var scanner = new Scanner("X");

            scanner.ReadChar().ShouldBe('X');
        }

        [Test]
        public void Should_go_to_next_character()
        {
            var scanner = new Scanner("XY");

            scanner.Next();

            scanner.ReadChar().ShouldBe('Y');
        }

        [Test]
        public void Should_go_to_end_of_file()
        {
            var scanner = new Scanner("XY");

            scanner.Next();
            scanner.Next();

            scanner.ReadChar().ShouldBe('\0');
        }

        [Test]
        public void Should_go_to_end_of_file_when_empty()
        {
            var scanner = new Scanner("");

            scanner.Next();

            scanner.ReadChar().ShouldBe('\0');
        }

        [Test]
        public void Should_go_to_next_character_when_lookahead()
        {
            var scanner = new Scanner("XY");

            scanner.ReadAhead(offset: 1).ShouldBe('Y');
        }

        [Test]
        public void Should_go_to_end_of_file_when_lookahead_looks_beyond_text_length()
        {
            var scanner = new Scanner("XY");

            scanner.ReadAhead(offset: 2).ShouldBe('\0');
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Should_not_throw_when_lookahead_offset_above_or_equal_to_zero(int offset)
        {
            var scanner = new Scanner("XY");

            Should.NotThrow(() => scanner.ReadAhead(offset));
        }

        [Test]
        public void Should_not_throw_when_lookahead_offset_below_zero()
        {
            var scanner = new Scanner("XY");

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.ReadAhead(offset: -1));
        }
    }
}