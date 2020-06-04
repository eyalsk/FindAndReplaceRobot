namespace FindAndReplaceRobot.Language.Tests.Extensions
{
    using FindAndReplaceRobot.Language;

    internal static class TokenExtension
    {
        public static string GetRangeAndValue(this Token token) =>
            $"Range:{token.Range}; Value:{token.Value}";
    }
}
