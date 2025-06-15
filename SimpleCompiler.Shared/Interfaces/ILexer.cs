using SimpleCompiler.Shared.Models;

namespace SimpleCompiler.Shared.Interfaces;

public interface ILexer
{
    IAsyncEnumerable<Token> Lex(string input);
}