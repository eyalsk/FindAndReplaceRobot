namespace FindAndReplaceRobot.Language.Tests.Utils
{
    using System.Text.RegularExpressions;

    using NUnit.Framework;

    internal static class Randomizer
    {
        private readonly static Regex _itemParser =
            new Regex("🎲", RegexOptions.Compiled);

        public static string GenerateString(string pattern, int length = 8) =>
            _itemParser.Replace(pattern, _ => TestContext.CurrentContext.Random.GetString(length));
    }
}