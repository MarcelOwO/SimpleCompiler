using SimpleCompiler.Shared.Enums;

namespace SimpleCompiler.Shared.Models;

public class Token(TokenType type, int line, object? value = null)
{
    public readonly TokenType Type = type;
    public readonly int Line = line;
    public readonly object? Value = value;
}