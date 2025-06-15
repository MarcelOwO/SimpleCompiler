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
            { '}', TokenType.ClosedCurlyBracket },
            { '(', TokenType.OpenBracket },
            { ')', TokenType.ClosedBracket },
            { ',', TokenType.Comma },
            { '+', TokenType.Plus },
            { '-', TokenType.Minus },
            { '*', TokenType.Keyword },
            { '/', TokenType.Divide },
        };

    private readonly Dictionary<char, TokenType> _doubleCharTokens = new()
    {
        { '=', TokenType.Equal },
        { '!', TokenType.Not },
        { '<', TokenType.Smaller },
        { '>', TokenType.Larger },

        { '|', TokenType.Error },
        { '&', TokenType.Error },
        { '/', TokenType.Error }
    };

    private readonly Dictionary<string, TokenType> _possibleDoubleSymbols = new()
    {
        { "==", TokenType.Equal },
        { "!=", TokenType.NotEqual },
        { "<=", TokenType.SmallerOrEqual },
        { ">=", TokenType.LargerOrEqual },
        { "||", TokenType.Or },
        { "&&", TokenType.And },
    };

    public async IAsyncEnumerable<Token> Lex(string input)
    {
        await foreach (var c in ReadChars(input))
        {
            switch (_lexerState)
            {
                case LexerState.None:
                    foreach (var p in NoneLexState(c)) yield return p;
                    break;
                case LexerState.Symbol:
                    foreach (var p1 in DoubleSymbolLexState(c)) yield return p1;
                    break;
                case LexerState.Identifier:
                    foreach (var p2 in IdentifierLexState(c)) yield return p2;
                    continue;
                case LexerState.Number:
                    foreach (var p3 in NumberLexState(c)) yield return p3;
                    break;
                case LexerState.String:
                    foreach (var p4 in StringLexState(c)) yield return p4;
                    break;
                case LexerState.SimpleComment:
                    foreach (var p5 in SimpleCommentLexState(c)) yield return p5;
                    continue;
                case LexerState.MultiLineComment:
                    foreach (var p6 in MultiLineCommentLexState(c)) yield return p6;
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private IEnumerable<Token> SimpleCommentLexState(char c)
    {
        if (c != '\n')
        {
            _sb.Append(c);
            yield break;
        }

        yield return new Token(TokenType.Comment, _lineNumber, _sb.ToString());
        Reset();
        _lineNumber++;
    }

    private void Reset()
    {
        _sb.Clear();
        _lexerState = LexerState.None;
    }

    private IEnumerable<Token> MultiLineCommentLexState(char c)
    {
        if (_sb.ToString().EndsWith("*\\"))
        {
            yield return new Token(TokenType.Comment, _lineNumber, _sb.ToString());
            Reset();
        }

        _sb.Append(c);
    }

    private IEnumerable<Token> StringLexState(char c)
    {
        if (c == '"')
        {
            yield return new Token(TokenType.String, _lineNumber, _sb.ToString());
            Reset();
            yield break;
        }

        _sb.Append(c);
    }

    private IEnumerable<Token> NumberLexState(char c)
    {
        if (char.IsDigit(c))
        {
            _sb.Append(c);
            yield break;
        }

        yield return new Token(TokenType.Number, _lineNumber, _sb.ToString());
        Reset();

        var token = CheckFirstChar(c);

        if (token != null)
        {
            yield return token;
        }
    }

    private IEnumerable<Token> IdentifierLexState(char c)
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

        yield return new Token(type, _lineNumber, _sb.ToString());

        Reset();

        var token = CheckFirstChar(c);

        if (token != null)
        {
            yield return token;
        }
    }

    private IEnumerable<Token> DoubleSymbolLexState(char c)
    {
        if (_sb.ToString().Length != 1)
        {
            Console.WriteLine(_sb.ToString());
            throw new Exception("Should not be more then one character in _sb");
        }

        var previousChar = _sb.ToString()[0];
        var currentChar = c;
        var doubleChar = previousChar.ToString() + currentChar.ToString();


        if (_possibleDoubleSymbols.TryGetValue(doubleChar, out var doubleSymbolToken))
        {
            Reset();
            yield return new Token(doubleSymbolToken, _lineNumber, doubleChar);
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


        if (_doubleCharTokens.TryGetValue(previousChar, out var tokenType))
        {
            Reset();
            yield return new Token(tokenType, previousChar);
        }

        Reset();

        var token = CheckFirstChar(c);
        if (token != null)
        {
            yield return token;
        }
    }

    private IEnumerable<Token> NoneLexState(char c)
    {
        var token = CheckFirstChar(c);
        
        if (token != null)
        {
            yield return token;
        }
    }

    private Token? CheckFirstChar(char c)
    {
        var charType = GetCharType(c);

        if (charType == CharType.Whitespace)
        {
            return null;
        }

        if (c == '\n')
        {
            _lineNumber++;
            return null;
        }


        if (_doubleCharTokens.ContainsKey(c))
        {
            _lexerState = LexerState.Symbol;
            _sb.Append(c);
            return null;
        }

        if (_singleCharToken.TryGetValue(c, out var value))
        {
            return new Token(value, c);
        }

        if (charType == CharType.Letter)
        {
            _lexerState = LexerState.Identifier;
            _sb.Append(c);
            return null;
        }

        if (charType == CharType.Digit)
        {
            _lexerState = LexerState.Number;
            _sb.Append(c);
            return null;
        }
        //Console.WriteLine("Unknown character:" + c);
        return null;
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