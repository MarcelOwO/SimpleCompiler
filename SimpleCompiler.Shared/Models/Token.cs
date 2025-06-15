using SimpleCompiler.Shared.Enums;

namespace SimpleCompiler.Shared.Models;

public  class Token(TokenType type, object? value) 
{
    public readonly TokenType Type = type;
    public readonly object? Value = value;
}