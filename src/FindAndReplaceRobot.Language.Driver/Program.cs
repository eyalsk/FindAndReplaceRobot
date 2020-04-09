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

            Console.WriteLine($"Index\tLine Number\tColumn Number\tCharacter");

            while (true)
            {
                var ch = scanner.ReadChar();
                var pos = scanner.Position;

                Console.WriteLine($"{pos.Index}\t{pos.LineNumber}\t\t{pos.ColumnNumber}\t\t{ch.ToReadableString()}");

                if (scanner.Next()) break;
            }
        }
    }
}
