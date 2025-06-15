using System.Diagnostics;
using System.Text;
using SimpleCompiler.Lexer.Enums;
using SimpleCompiler.Shared.Enums;
using SimpleCompiler.Shared.Interfaces;
using SimpleCompiler.Shared.Models;

namespace SimpleCompiler.Lexer;

public class Lexer : ILexer
{
    private int _lineNumber = 0;
    private readonly StringBuilder _sb = new();
    private LexerState _lexerState = LexerState.None;

    private readonly HashSet<string> _keywords =
    [
        "String", "Console.WriteLine", "boolean", "class", "do", "else", "false", "if", "int", "public", "return",
        "static", "true", "void", "case", "switch", "default"
    ];

    private readonly Dictionary<char, TokenType> _singleCharToken =
        new()
        {
            { ';', TokenType.SemiColon },
            { '{', TokenType.OpenCurlyBracket },
            { '}', TokenType.OpenCurlyBracket },
            { '(', TokenType.OpenBracket },
            { ')', TokenType.ClosedBracket },
            { ',', TokenType.Comma },
            { '+', TokenType.Plus },
            { '-', TokenType.Minus },
            { '*', TokenType.Keyword },
            { '/', TokenType.Divide },
        };

    private readonly List<char> _doubleCharTokens = ['=', '!', '<', '>', '|', '&', '/'];

    public async IAsyncEnumerable<Token> Lex(string input)
    {
        await foreach (var c in ReadChars(input))
        {
            switch (_lexerState)
            {
                case LexerState.None:
                    await foreach (var p in NoneLexState(c)) yield return p;
                    break;
                case LexerState.Symbol:
                    await foreach (var p1 in DoubleSymbolLexState(c)) yield return p1;
                    break;
                case LexerState.Identifier:
                    await foreach (var p2 in IdentifierLexState(c)) yield return p2;
                    continue;
                case LexerState.Number:
                    await foreach (var p3 in NumberLexState(c)) yield return p3;
                    break;
                case LexerState.String:
                    await foreach (var p4 in StringLexState(c)) yield return p4;
                    break;
                case LexerState.SimpleComment:
                    await foreach (var p5 in SimpleCommentLexState(c)) yield return p5;
                    continue;
                case LexerState.MultiLineComment:
                    await foreach (var p6 in MultiLineCommentLexState(c)) yield return p6;
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private async IAsyncEnumerable<Token> SimpleCommentLexState(char c)
    {
        if (c != '\n')
        {
            _sb.Append(c);
            yield break;
        }

        yield return new Token(TokenType.Comment, _sb.ToString());
        _sb.Clear();
        _lexerState = LexerState.None;
        _lineNumber++;
    }

    private async IAsyncEnumerable<Token> MultiLineCommentLexState(char c)
    {
        if (_sb.ToString().EndsWith("*\\"))
        {
            _lexerState = LexerState.None;
            yield return new Token(TokenType.Comment, _sb.ToString());
            _sb.Clear();
        }

        _sb.Append(c);
    }

    private async IAsyncEnumerable<Token> StringLexState(char c)
    {
        if (c == '"')
        {
            _lexerState = LexerState.None;
            yield return new Token(TokenType.String, _sb.ToString());
            _sb.Clear();
            yield break;
        }

        _sb.Append(c);
    }

    private async IAsyncEnumerable<Token> NumberLexState(char c)
    {
        if (char.IsDigit(c))
        {
            _sb.Append(c);
            yield break;
        }

        yield return new Token(TokenType.Number, _sb.ToString());
        _sb.Clear();
        _lexerState = LexerState.None;
        await foreach (var token in CheckFirstChar(c)) yield return token;
    }

    private async IAsyncEnumerable<Token> IdentifierLexState(char c)
    {
        if (c == '_')
        {
            _sb.Append(c);
            yield break;
        }

        if (char.IsLetterOrDigit(c))
        {
            _sb.Append(c);
            yield break;
        }

        var type = _keywords.Contains(_sb.ToString()) ? TokenType.Keyword : TokenType.Identifier;

        yield return new Token(type, _sb.ToString());
        _sb.Clear();
        _lexerState = LexerState.None;

        await foreach (var token in CheckFirstChar(c)) yield return token;
    }

    private async IAsyncEnumerable<Token> DoubleSymbolLexState(char c)
    {
        if (c == '=' && _sb.ToString() == "=")
        {
            yield return DoubleSymbolToken('=', '=');
            yield break;
        }

        if (c == '|' && _sb.ToString() == "|")
        {
            yield return DoubleSymbolToken('|', '|');
            yield break;
        }

        if (c == '&' && _sb.ToString() == "&")
        {
            yield return DoubleSymbolToken('&', '&');
            yield break;
        }

        if (c == '=' && _sb.ToString() == "!")
        {
            yield return DoubleSymbolToken('!', '=');
            yield break;
        }

        if (c == '=' && _sb.ToString() == ">")
        {
            yield return DoubleSymbolToken('>', '=');
            yield break;
        }

        if (c == '=' && _sb.ToString() == "<")
        {
            yield return DoubleSymbolToken('<', '=');
            yield break;
        }

        if (c == '/' && _sb.ToString() == "/")
        {
            _lexerState = LexerState.SimpleComment;
            _sb.Clear();
            _sb.Append("//");
            yield break;
        }

        if (c == '*' && _sb.ToString() == "/")
        {
            _lexerState = LexerState.MultiLineComment;
            _sb.Clear();
            _sb.Append("/*");
            yield break;
        }

        yield return new Token(TokenType.Error, _sb.ToString());

        await foreach (var token in CheckFirstChar(c)) yield return token;
    }

    private async IAsyncEnumerable<Token> NoneLexState(char c)
    {
        await foreach (var token in CheckFirstChar(c)) yield return token;
    }

    private Token DoubleSymbolToken(char first, char second)
    {
        _sb.Clear();
        _lexerState = LexerState.None;
        return new Token(TokenType.Keyword, first + second);
    }

    private async IAsyncEnumerable<Token> CheckFirstChar(char c)
    {
        var charType = GetCharType(c);

        if (charType == CharType.Whitespace)
        {
            yield break;
        }
        if (c == '\n')
        {
            _lineNumber++;
            yield break;
        }


        if (_doubleCharTokens.Contains(c))
        {
            _lexerState = LexerState.Symbol;
            _sb.Append(c);
            yield break;
        }

        if (_singleCharToken.TryGetValue(c, out var value))
        {
            yield return new Token(value, c);
        }

        if (charType == CharType.Letter)
        {
            _lexerState = LexerState.Identifier;
            _sb.Append(c);
            yield break;
        }

        if (charType == CharType.Digit)
        {
            _lexerState = LexerState.Number;
            _sb.Append(c);
            yield break;
        }

        Console.WriteLine("Unknown character: " + c);
    }

    private async IAsyncEnumerable<char> ReadChars(string input)
    {
        foreach (var c in await File.ReadAllTextAsync(input))
        {
            yield return c;
        }
    }

    private static CharType GetCharType(char c)
    {
        if (char.IsDigit(c))
        {
            return CharType.Digit;
        }

        if (char.IsSymbol(c))
        {
            return CharType.Symbol;
        }

        if (char.IsLetter(c))
        {
            return CharType.Letter;
        }

        if (char.IsWhiteSpace(c))
        {
            return CharType.Whitespace;
        }

        return CharType.None;
    }
}