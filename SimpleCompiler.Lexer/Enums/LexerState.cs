namespace SimpleCompiler.Lexer.Enums;

public enum LexerState
{
    None,
    SimpleComment,
    Identifier,
    Number,
    String,
    MultiLineComment,
    Symbol,
}