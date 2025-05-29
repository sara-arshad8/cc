#region Semantic Analyzer with SDT
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SemanticAnalyzerLab
{
    class Program
    {
        static List<List<string>> Symboltable = new List<List<string>>();
        static List<string> finalArray = new List<string>();
        static Regex variable_Reg = new Regex(@"^[A-Za-z_][A-Za-z0-9]*$");

        static void Main(string[] args)
        {
            InitializeSymbolTable();
            InitializeFinalArray();
            PrintLexerOutput();

            for (int i = 0; i < finalArray.Count; i++)
            {
                if (finalArray[i] == "=")
                {
                    SyntaxDirectedTranslation(i - 1); // variable name is before =
                }
            }

            Console.WriteLine("\nFinal Symbol Table:");
            foreach (var entry in Symboltable)
            {
                Console.WriteLine($"Name: {entry[0]}, Type: {entry[2]}, Value: {entry[3]}");
            }

            Console.WriteLine("\nSemantic Analysis with SDT Completed.");
            Console.ReadLine();
        }

        static void InitializeSymbolTable()
        {
            Symboltable.Add(new List<string> { "x", "id", "int", "0" });
            Symboltable.Add(new List<string> { "y", "id", "int", "0" });
            Symboltable.Add(new List<string> { "i", "id", "int", "0" });
            Symboltable.Add(new List<string> { "l", "id", "char", "0" });
        }

        static void InitializeFinalArray()
        {
            finalArray.AddRange(new string[] {
                "int", "main", "(", ")", "{",
                "int", "x", ";",
                "x", ";",
                "x", "=", "2", "+", "5", "+", "(", "4", "*", "8", ")", "+", "l", "/", "9", ";",
                "if", "(", "x", "+", "y", ")", "{",
                "if", "(", "x", "!=", "4", ")", "{",
                "x", "=", "6", ";",
                "y", "=", "10", ";",
                "i", "=", "11", ";",
                "}", "}", "}"
            });
        }

        static void PrintLexerOutput()
        {
            Console.WriteLine("Tokenizing...");
            int row = 1, col = 1;
            foreach (string token in finalArray)
            {
                if (token == "int") Console.WriteLine($"INT ({row},{col})");
                else if (token == "main") Console.WriteLine($"MAIN ({row},{col})");
                else if (token == "(") Console.WriteLine($"LPAREN ({row},{col})");
                else if (token == ")") Console.WriteLine($"RPAREN ({row},{col})");
                else if (token == "{") Console.WriteLine($"LBRACE ({row},{col})");
                else if (token == "}") Console.WriteLine($"RBRACE ({row},{col})");
                else if (token == ";") Console.WriteLine($"SEMI ({row},{col})");
                else if (token == "=") Console.WriteLine($"ASSIGN ({row},{col})");
                else if (token == "+") Console.WriteLine($"PLUS ({row},{col})");
                else if (token == "-") Console.WriteLine($"MINUS ({row},{col})");
                else if (token == "*") Console.WriteLine($"TIMES ({row},{col})");
                else if (token == "/") Console.WriteLine($"DIV ({row},{col})");
                else if (token == "!=") Console.WriteLine($"NEQ ({row},{col})");
                else if (Regex.IsMatch(token, @"^[0-9]+$")) Console.WriteLine($"INT_CONST ({row},{col}): {token}");
                else if (Regex.IsMatch(token, @"^[0-9]+\.[0-9]+$")) Console.WriteLine($"FLOAT_CONST ({row},{col}): {token}");
                else if (Regex.IsMatch(token, @"^[a-zA-Z]$")) Console.WriteLine($"CHAR_CONST ({row},{col}): {token}");
                else if (variable_Reg.Match(token).Success) Console.WriteLine($"ID ({row},{col}): {token}");
                else Console.WriteLine($"UNKNOWN ({row},{col}): {token}");

                col += token.Length + 1;
                if (token == ";") { row++; col = 1; }
            }
            Console.WriteLine("EOF ({0},{1})", row, col);
        }

        static void SyntaxDirectedTranslation(int variableIndex)
        {
            string varName = finalArray[variableIndex];
            int assignIndex = variableIndex + 1;
            int i = assignIndex + 1;

            List<string> expressionTokens = new List<string>();

            while (i < finalArray.Count && finalArray[i] != ";")
            {
                expressionTokens.Add(finalArray[i]);
                i++;
            }

            int result = EvaluateExpression(expressionTokens);
            int symbolIndex = FindSymbol(varName);

            if (symbolIndex != -1)
            {
                Symboltable[symbolIndex][3] = result.ToString();
                Console.WriteLine($"Updated {varName} = {result}");
            }
        }

        static int EvaluateExpression(List<string> tokens)
        {
            // Convert infix to postfix and then evaluate
            List<string> postfix = InfixToPostfix(tokens);
            return EvaluatePostfix(postfix);
        }

        static List<string> InfixToPostfix(List<string> tokens)
        {
            List<string> output = new List<string>();
            Stack<string> stack = new Stack<string>();

            Dictionary<string, int> precedence = new Dictionary<string, int>
            {
                { "+", 1 },
                { "-", 1 },
                { "*", 2 },
                { "/", 2 }
            };

            foreach (string token in tokens)
            {
                if (Regex.IsMatch(token, @"^[0-9]+$") || variable_Reg.Match(token).Success)
                {
                    output.Add(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Pop(); // pop '('
                }
                else if (precedence.ContainsKey(token))
                {
                    while (stack.Count > 0 && precedence.ContainsKey(stack.Peek()) &&
                           precedence[token] <= precedence[stack.Peek()])
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
            }

            while (stack.Count > 0)
            {
                output.Add(stack.Pop());
            }

            return output;
        }

        static int EvaluatePostfix(List<string> postfix)
        {
            Stack<int> stack = new Stack<int>();

            foreach (string token in postfix)
            {
                if (Regex.IsMatch(token, @"^[0-9]+$"))
                {
                    stack.Push(int.Parse(token));
                }
                else if (variable_Reg.Match(token).Success)
                {
                    int idx = FindSymbol(token);
                    if (idx != -1) stack.Push(int.Parse(Symboltable[idx][3]));
                }
                else
                {
                    int b = stack.Pop();
                    int a = stack.Pop();

                    switch (token)
                    {
                        case "+": stack.Push(a + b); break;
                        case "-": stack.Push(a - b); break;
                        case "*": stack.Push(a * b); break;
                        case "/": stack.Push(a / b); break;
                    }
                }
            }

            return stack.Pop();
        }

        static int FindSymbol(string name)
        {
            for (int i = 0; i < Symboltable.Count; i++)
            {
                if (Symboltable[i][0] == name)
                    return i;
            }
            return -1;
        }
    }
}
#endregion
                     
