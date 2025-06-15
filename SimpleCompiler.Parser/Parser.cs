using SimpleCompiler.Shared.Interfaces;
using SimpleCompiler.Shared.Models;

namespace SimpleCompiler.Parser;

public enum ParseState
{
    Statement
}

public class Parser : IParser
{
    private readonly ParseState _state = ParseState.Statement;

    public async Task ParseAsync(Token token)
    {
        switch (_state)
        {
            case ParseState.Statement:
                
                

                break;
        }
    }
}