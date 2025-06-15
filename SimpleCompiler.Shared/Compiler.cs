using SimpleCompiler.Shared.Interfaces;

namespace SimpleCompiler.Shared;

public class Compiler(ILexer lexer, IParser parser)
{
    public async Task Compile(string input, string output)
    {
        await foreach (var token in  lexer.Lex(input))
        {
            Console.WriteLine($"<{token.Type},{token.Value}> at Line: {token.Line}");
            await parser.ParseAsync(token);
        };
    }
}