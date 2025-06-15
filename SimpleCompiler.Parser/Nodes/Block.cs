using SimpleCompiler.Shared.Enums;

namespace SimpleCompiler.Parser.terminals;

public class Block
{
    public TokenType openCurlyBrace;
    public Expression expression;
    public TokenType closedCurlyBrace;
}