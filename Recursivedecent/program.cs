using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        Console.WriteLine("Enter Java-like code (end input with a blank line):");
        string input = "";
        string line;
        while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
        {
            input += line + "\n";
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("No input provided.");
            return;
        }

        Lexer lexer = new Lexer(input);
        Parser parser = new Parser(lexer);
        try
        {
            parser.ParseProgram();
            Console.WriteLine("✅ Input is syntactically correct.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Syntax Error: {ex.Message}");
        }
    }
}

public class Lexer
{
    private string input;
    private int position;
    public string CurrentToken { get; private set; }

    // Fixed regex pattern to properly handle all tokens including keywords, identifiers, and symbols
    private static readonly Regex tokenRegex = new Regex(
        @"\G\s*(" +
        @"int|float|void|boolean|String|class|public|private|static|" + // Keywords
        @"[a-zA-Z_][a-zA-Z0-9_]*|" +  // Identifiers
        @"\{|\}|\(|\)|,|;|=|\+|-|\*|/)" + // Symbols
        @"", RegexOptions.Compiled);

    public Lexer(string input)
    {
        this.input = input;
        this.position = 0;
        NextToken();
    }

    public void NextToken()
    {
        // Skip whitespace first
        while (position < input.Length && char.IsWhiteSpace(input[position]))
        {
            position++;
        }

        if (position >= input.Length)
        {
            CurrentToken = null;
            return;
        }

        var match = tokenRegex.Match(input, position);
        if (!match.Success)
        {
            throw new Exception($"Invalid token at position {position}: '{input[position]}'");
        }

        CurrentToken = match.Groups[1].Value;
        position = match.Index + match.Length;
    }

    public bool Match(string token)
    {
        if (CurrentToken == token)
        {
            NextToken();
            return true;
        }
        return false;
    }

    public void Expect(string token)
    {
        if (!Match(token))
        {
            throw new Exception($"Expected '{token}', but found '{CurrentToken ?? "EOF"}'");
        }
    }

    public bool IsType()
    {
        return CurrentToken == "int" || CurrentToken == "float" || CurrentToken == "void" ||
               CurrentToken == "boolean" || CurrentToken == "String";
    }

    public bool IsModifier()
    {
        return CurrentToken == "public" || CurrentToken == "private" || CurrentToken == "static";
    }

    public bool IsIdentifier()
    {
        if (string.IsNullOrEmpty(CurrentToken))
            return false;

        return Regex.IsMatch(CurrentToken, @"^[a-zA-Z_][a-zA-Z0-9_]*$") &&
               !IsType() && !IsModifier() && CurrentToken != "class";
    }
}

public class Parser
{
    private Lexer lexer;

    public Parser(Lexer lexer)
    {
        this.lexer = lexer;
    }

    public void ParseProgram()
    {
        while (lexer.CurrentToken != null)
        {
            ParseDeclaration();
        }
    }

    private void ParseDeclaration()
    {
        // Handle modifiers (public, private, static)
        while (lexer.IsModifier())
        {
            lexer.NextToken(); // consume modifier
        }

        // Check for class declaration
        if (lexer.Match("class"))
        {
            ParseClassDeclaration();
            return;
        }

        // Parse type for variable or method declaration
        if (!lexer.IsType())
        {
            throw new Exception($"Expected type declaration, found '{lexer.CurrentToken ?? "EOF"}'");
        }

        string type = ParseType();

        if (!lexer.IsIdentifier())
            throw new Exception("Expected identifier after type");

        string id = lexer.CurrentToken;
        lexer.NextToken();

        if (lexer.Match(";"))
        {
            // Variable declaration: type identifier;
            return;
        }
        else if (lexer.Match("("))
        {
            // Method declaration: type identifier(params) { }
            ParseParamList();
            lexer.Expect(")");
            lexer.Expect("{");
            ParseMethodBody();
            lexer.Expect("}");
        }
        else if (lexer.Match("="))
        {
            // Variable declaration with initialization: type identifier = value;
            ParseExpression();
            lexer.Expect(";");
        }
        else
        {
            throw new Exception("Expected ';', '=', or '(' after identifier");
        }
    }

    private void ParseClassDeclaration()
    {
        if (!lexer.IsIdentifier())
            throw new Exception("Expected class name");

        lexer.NextToken(); // consume class name
        lexer.Expect("{");

        // Parse class members
        while (lexer.CurrentToken != null && lexer.CurrentToken != "}")
        {
            ParseDeclaration();
        }

        lexer.Expect("}");
    }

    private void ParseMethodBody()
    {
        // Simple method body parsing - just consume tokens until closing brace
        int braceCount = 0;
        while (lexer.CurrentToken != null)
        {
            if (lexer.CurrentToken == "{")
            {
                braceCount++;
                lexer.NextToken();
            }
            else if (lexer.CurrentToken == "}")
            {
                if (braceCount == 0)
                    break;
                braceCount--;
                lexer.NextToken();
            }
            else
            {
                lexer.NextToken();
            }
        }
    }

    private void ParseParamList()
    {
        // Empty parameter list is valid
        if (lexer.CurrentToken == ")")
            return;

        if (lexer.IsType())
        {
            ParseParam();
            while (lexer.Match(","))
            {
                ParseParam();
            }
        }
    }

    private void ParseParam()
    {
        if (!lexer.IsType())
            throw new Exception("Expected parameter type");

        ParseType();

        if (!lexer.IsIdentifier())
            throw new Exception("Expected parameter name");

        lexer.NextToken();
    }

    private string ParseType()
    {
        if (lexer.IsType())
        {
            string type = lexer.CurrentToken;
            lexer.NextToken();
            return type;
        }
        throw new Exception($"Expected type, found '{lexer.CurrentToken ?? "EOF"}'");
    }

    private void ParseExpression()
    {
        // Simple expression parsing - handle identifiers, numbers, and basic operators
        if (lexer.IsIdentifier())
        {
            lexer.NextToken();
        }
        else if (Regex.IsMatch(lexer.CurrentToken ?? "", @"^\d+$"))
        {
            lexer.NextToken(); // consume number
        }
        else if (lexer.CurrentToken == "\"")
        {
            // Handle string literals (simplified)
            lexer.NextToken();
        }
        else
        {
            throw new Exception($"Expected expression, found '{lexer.CurrentToken ?? "EOF"}'");
        }

        // Handle binary operators
        while (lexer.CurrentToken == "+" || lexer.CurrentToken == "-" ||
               lexer.CurrentToken == "*" || lexer.CurrentToken == "/")
        {
            lexer.NextToken(); // consume operator
            ParseExpression(); // parse right operand
            break; // Simple handling for now
        }
    }
}
