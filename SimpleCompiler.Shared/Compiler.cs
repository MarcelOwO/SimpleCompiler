
using Antlr4.Runtime;

namespace SimpleCompiler.Shared;

public class Compiler()
{
    public async Task Compile(string input, string output)
    {
        var inputStream = new AntlrInputStream(new StreamReader(input));
        var lexer = new SimpleLexer(inputStream);
        var parser = new SimpleParser(new CommonTokenStream(lexer));
        var tree = parser.program();
        Console.WriteLine(tree.ToStringTree(parser));
    }
    
}