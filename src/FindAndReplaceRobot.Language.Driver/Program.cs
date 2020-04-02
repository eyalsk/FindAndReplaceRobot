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
            
            while(scanner.Next())
            {
                var ch = scanner.GetCurrentValue();

                if (ch.Value != InvisibleCharacters.EndOfFile)
                {
                    Console.WriteLine($"{ch.Position.Index}:{ch.Position.LineNumber}:{ch.Position.ColumnNumber}, {ch}");
                }
            }

            {
                var ch = scanner.GetCurrentValue();
                Console.WriteLine($"{ch.Position.Index}:{ch.Position.LineNumber}:{ch.Position.ColumnNumber}, {ch}");
            }
        }
    }
}
