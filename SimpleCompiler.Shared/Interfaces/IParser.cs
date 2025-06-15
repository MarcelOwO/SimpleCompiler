using SimpleCompiler.Shared.Models;

namespace SimpleCompiler.Shared.Interfaces;

public interface IParser
{
    Task ParseAsync(Token token);
}