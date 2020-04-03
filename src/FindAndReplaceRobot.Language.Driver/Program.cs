namespace FindAndReplaceRobot.Language.Driver
{
    using System;
    using System.IO;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            var basePath = AppContext.BaseDirectory;
            var projectPath = basePath.Substring(0, basePath.IndexOf("bin"));
            var filePath = Path.Combine(projectPath, "test.farr");

            var text = File.ReadAllText(filePath).AsMemory();
            var scanner = new Scanner(text);

            Console.WriteLine($"Index\tLine Number\tColumn Number\tLevel\tCharacter");

            while (scanner.Next())
            {
                var ch = scanner.ReadChar();

                if (ch.Value != InvisibleCharacters.EndOfFile)
                {
                    Console.WriteLine($"{ch.Position.Index}\t{ch.Position.LineNumber}\t\t{ch.Position.ColumnNumber}\t\t{new string('*', ch.Position.Level)}\t{ch}");
                }
            }

            {
                var ch = scanner.ReadChar();
                Console.WriteLine($"{ch.Position.Index}\t{ch.Position.LineNumber}\t\t{ch.Position.ColumnNumber}\t\t{new string('*', ch.Position.Level)}\t{ch}");
            }
        }
    }
}
