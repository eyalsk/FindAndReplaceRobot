namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using NUnit.Framework;
    using Shouldly;

    internal class ScannerTests
    {
        [Test]
        public void Should_return_end_of_file_when_empty()
        {
            var scanner = new Scanner("");

            scanner.ReadChar().ShouldBe('\0');
        }

        [Test]
        public void Should_return_1st_character()
        {
            var scanner = new Scanner("X");

            scanner.ReadChar().ShouldBe('X');
        }

        [Test]
        public void Should_go_to_next_character()
        {
            var scanner = new Scanner("XY");

            scanner.MoveNext();

            scanner.ReadChar().ShouldBe('Y');
        }

        [Test]
        public void Should_go_to_end_of_file()
        {
            var scanner = new Scanner("XY");

            scanner.MoveNext();
            scanner.MoveNext();

            scanner.ReadChar().ShouldBe('\0');
        }

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

            scanner.ReadAhead(offset).ShouldBe('\0');
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Should_not_throw_when_offset_above_or_equal_to_zero(int offset)
        {
            var scanner = new Scanner("XY");

            Should.NotThrow(() => scanner.ReadAhead(offset));
        }

        [Test]
        public void Should_throw_when_offset_below_zero()
        {
            var scanner = new Scanner("XY");

            Should.Throw<ArgumentOutOfRangeException>(() => scanner.ReadAhead(offset: -1));
        }

        [Test]
        public void Should_read_ahead_and_move_ahead_to_3rd_character()
        {
            var scanner = new Scanner("XYZ");

            scanner.ReadAhead(2);
            scanner.MoveAhead();
            scanner.ReadChar().ShouldBe('Z');
        }
    }
}