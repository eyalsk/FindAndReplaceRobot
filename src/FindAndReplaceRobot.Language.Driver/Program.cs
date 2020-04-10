namespace FindAndReplaceRobot.Language.Driver
{
    using System;
    using System.IO;

    internal static class Program
    {
        private static void Main()
        {
            var basePath = AppContext.BaseDirectory;
            var projectPath = basePath.Substring(0, basePath.IndexOf("bin"));
            var filePath = Path.Combine(projectPath, "test.farr");

            var text = File.ReadAllText(filePath);
            var scanner = new Scanner(text);

            while (true)
            {
                var ch = scanner.ReadChar();

                Console.WriteLine(ch.ToReadableString());

                if (!scanner.Next()) break;
            }
        }
    }
}
