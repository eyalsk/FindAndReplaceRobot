namespace FindAndReplaceRobot.Language.Tests.Utils
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    internal static class Randomizer
    {
        public static readonly Random _randomizer = new Random();
        public static readonly StringBuilder _sb = new StringBuilder();

        private readonly static Regex _itemParser = new Regex("🎲", RegexOptions.Compiled);

        public static string GenerateString(string pattern, int length = 8) =>
            _itemParser.Replace(pattern, _ => GetString(length));

        public static string GenerateOnlyLettersString(string pattern, int length = 8) =>
            _itemParser.Replace(pattern, _ =>
                GetString(length, "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ"));

        private static string GetString(
            int length,
            string chars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ1234567890")
        {
            _sb.Clear();

            for (int i = 0; i < length; i++)
            {
                int index = _randomizer.Next(0, chars.Length);

                _sb.Append(chars[index]);
            }

            return _sb.ToString();
        }
    }
}