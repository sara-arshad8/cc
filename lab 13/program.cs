using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace LexicalAnalyzerParser
{
    class Program
    {
        static List<Symbol> SymbolTable = new List<Symbol>();

        class Symbol
        {
            public int Index;
            public string Name;
            public string Type;
            public string Value;
            public int Line;

            public Symbol(int index, string name, string type, string value, int line)
            {
                Index = index;
                Name = name;
                Type = type;
                Value = value;
                Line = line;
            }
        }

        class Token
        {
            public string Type;
            public string Lexeme;
            public int Line;

            public Token(string type, string lexeme, int line)
            {
                Type = type;
                Lexeme = lexeme;
                Line = line;
            }

            public override string ToString()
            {
                return $"<{Type}, {Lexeme}>";
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Enter your code (press Enter twice to finish):");
            string userInput = ReadMultilineInput();

            var tokens = AnalyzeInput(userInput);
            Console.WriteLine("\nParsing...");
            Console.WriteLine("Parsing Steps:");
            Parse(tokens);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static string ReadMultilineInput()
        {
            string input = "";
            string line;
            while ((line = Console.ReadLine()) != null && line != "")
            {
                input += line + "\n";
            }
            return input;
        }

        static List<Token> AnalyzeInput(string userInput)
        {
            List<Token> tokensOutput = new List<Token>();
            List<string> keywordList = new List<string> { "int", "float", "begin", "end", "print", "if", "else", "while", "main", "new" };

            Regex variable_Reg = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");
            Regex constants_Reg = new Regex(@"^[0-9]+(\.[0-9]+)?([eE][+-]?[0-9]+)?$");
            Regex operators_Reg = new Regex(@"^[+\-*/=<>!&|]$");
            Regex punctuations_Reg = new Regex(@"^[.,;:{}()\[\]]$");

            List<string> lines = new List<string>(userInput.Split('\n'));

            int line_num = 0;
            int symbolIndex = 1;

            Console.WriteLine("\nTokens:");
            Console.WriteLine("-------");

            foreach (string rawLine in lines)
            {
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                line_num++;
                string line = rawLine.Trim();
                List<string> tokens = Tokenize(line);

                for (int i = 0; i < tokens.Count; i++)
                {
                    string token = tokens[i];
                    if (keywordList.Contains(token))
                    {
                        tokensOutput.Add(new Token("keyword", token, line_num));
                    }
                    else if (constants_Reg.IsMatch(token))
                    {
                        tokensOutput.Add(new Token("digit", token, line_num));
                    }
                    else if (operators_Reg.IsMatch(token))
                    {
                        tokensOutput.Add(new Token("op", token, line_num));
                    }
                    else if (punctuations_Reg.IsMatch(token))
                    {
                        tokensOutput.Add(new Token("punc", token, line_num));
                    }
                    else if (variable_Reg.IsMatch(token))
                    {
                        if (!SymbolExists(token))
                        {
                            string type = i > 0 && keywordList.Contains(tokens[i - 1]) ? tokens[i - 1] : "unknown";
                            string value = (i + 2 < tokens.Count && tokens[i + 1] == "=") ? tokens[i + 2] : "";
                            SymbolTable.Add(new Symbol(symbolIndex, token, type, value, line_num));
                            symbolIndex++;
                        }
                        tokensOutput.Add(new Token("var", token, line_num));
                    }
                    else
                    {
                        tokensOutput.Add(new Token("unknown", token, line_num));
                    }
                }
            }

            foreach (var token in tokensOutput)
                Console.Write($"{token} ");

            Console.WriteLine("\n\nSymbol Table:");
            Console.WriteLine("------------");
            Console.WriteLine("Index\tName\tType\tValue\tLine");
            foreach (var entry in SymbolTable)
                Console.WriteLine($"{entry.Index}\t{entry.Name}\t{entry.Type}\t{entry.Value}\t{entry.Line}");

            return tokensOutput;
        }

        static List<string> Tokenize(string line)
        {
            List<string> tokens = new List<string>();
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (char.IsWhiteSpace(c))
                {
                    if (current != "")
                    {
                        tokens.Add(current);
                        current = "";
                    }
                }
                else if ("+-*/=<>!&|".Contains(c) || ".,;:{}()[]".Contains(c))
                {
                    if (current != "")
                    {
                        tokens.Add(current);
                        current = "";
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    current += c;
                }
            }

            if (current != "")
                tokens.Add(current);

            return tokens;
        }

        static bool SymbolExists(string name)
        {
            foreach (var sym in SymbolTable)
            {
                if (sym.Name == name)
                    return true;
            }
            return false;
        }

        static Symbol GetSymbol(string name)
        {
            foreach (var sym in SymbolTable)
            {
                if (sym.Name == name)
                    return sym;
            }
            return null;
        }

        // Enhanced Parser with Stack-based Step Display
        static void Parse(List<Token> tokens)
        {
            if (tokens.Count == 0)
            {
                Console.WriteLine("Stack: [$] | Processing: $");
                Console.WriteLine("Parsing completed successfully. (No tokens to parse)");
                return;
            }

            Stack<string> parseStack = new Stack<string>();
            parseStack.Push("$");

            Queue<Token> inputQueue = new Queue<Token>(tokens);
            inputQueue.Enqueue(new Token("$", "$", 0)); // End marker

            int index = 0;

            void DisplayParsingStep(string processing, string action = "")
            {
                var stackArray = parseStack.ToArray().Reverse().ToArray();
                string stackStr = "[" + string.Join(" ", stackArray) + "]";

                // Format as table with fixed width columns
                Console.WriteLine($"| {stackStr,-25} | {processing,-15} | {action,-20} |");
            }

            void Match(string expectedType, string expectedLexeme = null)
            {
                if (index >= tokens.Count)
                {
                    string expected = expectedLexeme != null ? $"{expectedType}('{expectedLexeme}')" : expectedType;
                    Console.WriteLine($"\nSyntax Error: Expected {expected} but reached end of input");
                    Environment.Exit(1);
                }

                var currentToken = tokens[index];
                DisplayParsingStep(currentToken.Lexeme, $"Matching {expectedType}" + (expectedLexeme != null ? $"('{expectedLexeme}')" : ""));

                if (currentToken.Type == expectedType &&
                    (expectedLexeme == null || currentToken.Lexeme == expectedLexeme))
                {
                    parseStack.Push(currentToken.Lexeme);
                    index++;
                }
                else
                {
                    string expected = expectedLexeme != null ? $"{expectedType}('{expectedLexeme}')" : expectedType;
                    string actual = $"{currentToken.Type}('{currentToken.Lexeme}')";
                    Console.WriteLine($"\nSyntax Error: Expected {expected} but found {actual} at line {currentToken.Line}");
                    Environment.Exit(1);
                }
            }

            void Reduce(string nonTerminal, int popCount)
            {
                for (int i = 0; i < popCount; i++)
                {
                    if (parseStack.Count > 1) // Keep the $ marker
                        parseStack.Pop();
                }
                parseStack.Push(nonTerminal);
                DisplayParsingStep(index < tokens.Count ? tokens[index].Lexeme : "$", $"Reduce to {nonTerminal}");
            }

            void Expression()
            {
                if (index < tokens.Count && (tokens[index].Type == "digit" || tokens[index].Type == "var"))
                {
                    var currentToken = tokens[index];
                    DisplayParsingStep(currentToken.Lexeme, "Parsing Expression");
                    parseStack.Push(currentToken.Lexeme);
                    index++;
                    Reduce("E", 1); // Reduce variable/digit to Expression
                }
                else
                {
                    Console.WriteLine($"\nSyntax Error: Expected expression at line {tokens[index].Line}");
                    Environment.Exit(1);
                }
            }

            void Statement()
            {
                if (index >= tokens.Count) return;

                var currentToken = tokens[index];
                DisplayParsingStep(currentToken.Lexeme, "Parsing Statement");

                // Declaration statement: type var;
                if (currentToken.Type == "keyword" &&
                    (currentToken.Lexeme == "int" || currentToken.Lexeme == "float"))
                {
                    Match("keyword");
                    Match("var");
                    Match("punc", ";");
                    Reduce("S", 3); // Reduce declaration to Statement
                }
                // Assignment statement: var = expression;
                else if (index + 2 < tokens.Count &&
                         currentToken.Type == "var" &&
                         tokens[index + 1].Type == "op" &&
                         tokens[index + 1].Lexeme == "=")
                {
                    Match("var");
                    Match("op", "=");
                    Expression();
                    Match("punc", ";");
                    Reduce("S", 4); // Reduce assignment to Statement
                }
                // Print statement
                else if (currentToken.Type == "keyword" && currentToken.Lexeme == "print")
                {
                    Match("keyword", "print");
                    Match("punc", "(");
                    Expression();
                    Match("punc", ")");
                    Match("punc", ";");
                    Reduce("S", 5); // Reduce print to Statement
                }
                // Block statements
                else if (currentToken.Type == "keyword" && currentToken.Lexeme == "begin")
                {
                    Match("keyword", "begin");
                    while (index < tokens.Count &&
                           !(tokens[index].Type == "keyword" && tokens[index].Lexeme == "end"))
                    {
                        Statement();
                    }
                    Match("keyword", "end");
                    Reduce("S", 3); // Reduce block to Statement
                }
                else
                {
                    Console.WriteLine($"\nSyntax Error: Unknown statement starting with {currentToken.Type}('{currentToken.Lexeme}') at line {currentToken.Line}");
                    Environment.Exit(1);
                }
            }

            try
            {
                // Table header
                Console.WriteLine(new string('-', 70));
                Console.WriteLine($"| {"Stack",-25} | {"Processing",-15} | {"Action",-20} |");
                Console.WriteLine(new string('-', 70));

                DisplayParsingStep(tokens[0].Lexeme, "Starting Parse");

                while (index < tokens.Count)
                {
                    Statement();
                }

                // Final reduction to Program
                if (parseStack.Count > 2)
                {
                    var stackArray = parseStack.ToArray();
                    int statementCount = stackArray.Count(s => s == "S");
                    if (statementCount > 0)
                    {
                        Reduce("P", statementCount + 1); // Reduce all statements + $ to Program
                    }
                }

                DisplayParsingStep("$", "Final State");
                Console.WriteLine(new string('-', 70));
                Console.WriteLine("\nParsing completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nParsing failed with error: {ex.Message}");
            }
        }
    }
}
