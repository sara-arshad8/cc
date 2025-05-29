using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

class LL1Parser
{
    private static Dictionary<string, Dictionary<string, List<string>>> parsingTable = new Dictionary<string, Dictionary<string, List<string>>>
    {
        { "S", new Dictionary<string, List<string>>
            {
                { "(", new List<string> { "(", "S_inner", ")", "S'" } } } // S_inner handles the three options
        },
        { "S_inner", new Dictionary<string, List<string>>
            {
                { "e", new List<string> { "C", "x", "y" } },
                { "d", new List<string> { "d", "y" } },
                { "b", new List<string> { "b" } }
            }
        },
        { "S'", new Dictionary<string, List<string>>
            {
                { "x", new List<string> { "x", "y", "S'" } },
                { ")", new List<string> { "ε" } },
                { "$", new List<string> { "ε" } }
            }
        },
        { "C", new Dictionary<string, List<string>> { { "e", new List<string> { "e", "C'" } } } },
        { "C'", new Dictionary<string, List<string>>
            {
                { "m", new List<string> { "m", "C'" } },
                { "x", new List<string> { "ε" } },
                { "y", new List<string> { "ε" } },
                { ")", new List<string> { "ε" } }
            }
        }
    };

    private static Stack<string> stack = new Stack<string>();

    public static bool Parse(List<string> tokens)
    {
        tokens.Add("$"); // End marker
        stack.Clear();
        stack.Push("$");
        stack.Push("S");

        int index = 0;
        Console.WriteLine("Parsing Steps:");

        while (stack.Count > 0)
        {
            string top = stack.Pop();
            Console.WriteLine($"Stack: [{string.Join(" ", stack)}] | Processing: {top}");

            if (top == "ε") // Skip epsilon transitions
                continue;

            string currentToken = index < tokens.Count ? tokens[index] : "$";

            if (top == currentToken) // Terminal match
            {
                index++;
            }
            else if (parsingTable.ContainsKey(top)) // Non-terminal
            {
                if (parsingTable[top].ContainsKey(currentToken))
                {
                    List<string> production = parsingTable[top][currentToken];
                    for (int i = production.Count - 1; i >= 0; i--)
                    {
                        if (production[i] != "ε")
                        {
                            stack.Push(production[i]);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Unexpected token '{currentToken}' at position {index + 1}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"Syntax Error: Expected '{top}', but found '{currentToken}'");
                return false;
            }
        }
        return index == tokens.Count;
    }

    public static void Main()
    {
        Console.WriteLine("Enter an expression using '(', ')', 'e', 'm', 'x', 'y', 'd', 'b':");
        string input = Console.ReadLine();
        List<string> tokens = LexicalAnalysis(input);

        if (tokens == null)
        {
            Console.WriteLine("Lexical Analysis Failed! Invalid input.");
            return;
        }

        Console.WriteLine("Tokens: " + string.Join(" ", tokens));
        bool isValid = Parse(tokens);
        Console.WriteLine("Parsing Result: " + (isValid ? "Valid" : "Invalid"));

        Console.ReadKey();
    }

    private static List<string> LexicalAnalysis(string input)
    {
        List<string> tokens = new List<string>();
        string tokenRegex = @"^[xydbem()]$"; // Valid single-character tokens

        foreach (char ch in input.Replace(" ", ""))
        {
            if (Regex.IsMatch(ch.ToString(), tokenRegex))
            {
                tokens.Add(ch.ToString());
            }
            else
            {
                Console.WriteLine($"Invalid token found: '{ch}'");
                return null; // Stop processing on invalid character
            }
        }
        return tokens;
    }
}
