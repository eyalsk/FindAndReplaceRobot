namespace FindAndReplaceRobot.Language.Tests.Extensions
{
    using FindAndReplaceRobot.Language;

    internal static class TokenExtension
    {
        public static string GetRangeAndValue(this Token token)
        {
            if (token.Value.Length == 0)
            {
                return $"Range:{token.Range}";
            }

            return $"Range:{token.Range}; Value:{token.Value}";
        }
    }
}
