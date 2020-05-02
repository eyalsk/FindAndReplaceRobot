namespace FindAndReplaceRobot.Language.Driver
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using FindAndReplaceRobot.Language;

    internal static class Program
    {
        private static void Main()
        {
            var basePath = AppContext.BaseDirectory;
            var projectPath = basePath.Substring(0, basePath.IndexOf("bin"));
            var filePath = Path.Combine(projectPath, "test.farr");

            var text = File.ReadAllText(filePath);
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new ConsoleTraceListener());

            while (true)
            {
                var token = lexer.ReadToken();

                if (token.Kind == TokenKind.EndOfFile) break;

                Console.WriteLine("--------------------------------------------------------------------------------------------------------");
                Console.WriteLine($" {token}\n\t\t[{token.Start}-{token.End}]\t\t{token.Kind}|{token.Context}");
                Console.WriteLine("--------------------------------------------------------------------------------------------------------");
            }
        }
    }
}
