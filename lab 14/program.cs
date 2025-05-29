using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniCompiler
{
    class Program
    {
        static int tempVarCounter = 1;

        static void Main(string[] args)
        {
            Console.Write("Enter an expression: ");
            string expr = Console.ReadLine().Replace(" ", "");

            string postfix = InfixToPostfix(expr);
            Console.WriteLine("\nPostfix: " + postfix);

            List<string> code = GenerateThreeAddressCode(postfix);
            Console.WriteLine("\nOptimized Three-Address Code:");
            foreach (var line in code)
                Console.WriteLine(line);
        }

        // Step 1: Convert Infix to Postfix using Shunting Yard algorithm
        static string InfixToPostfix(string infix)
        {
            StringBuilder output = new StringBuilder();
            Stack<char> stack = new Stack<char>();

            Dictionary<char, int> precedence = new Dictionary<char, int>
            {
                { '+', 1 }, { '-', 1 },
                { '*', 2 }, { '/', 2 }
            };

            for (int i = 0; i < infix.Length; i++)
            {
                char token = infix[i];
                if (char.IsLetterOrDigit(token))
                {
                    output.Append(token);
                }
                else if (precedence.ContainsKey(token))
                {
                    while (stack.Count > 0 && precedence.ContainsKey(stack.Peek()) &&
                           precedence[stack.Peek()] >= precedence[token])
                    {
                        output.Append(stack.Pop());
                    }
                    stack.Push(token);
                }
            }

            while (stack.Count > 0)
                output.Append(stack.Pop());

            return output.ToString();
        }

        // Step 2: Generate Optimized 3AC from Postfix
        static List<string> GenerateThreeAddressCode(string postfix)
        {
            Stack<string> stack = new Stack<string>();
            List<string> code = new List<string>();

            foreach (char token in postfix)
            {
                if (char.IsLetterOrDigit(token))
                {
                    stack.Push(token.ToString());
                }
                else
                {
                    string op2 = stack.Pop();
                    string op1 = stack.Pop();

                    // Constant Folding
                    if (IsNumber(op1) && IsNumber(op2))
                    {
                        int val = token switch
                        {
                            '+' => int.Parse(op1) + int.Parse(op2),
                            '-' => int.Parse(op1) - int.Parse(op2),
                            '*' => int.Parse(op1) * int.Parse(op2),
                            '/' => int.Parse(op1) / int.Parse(op2),
                            _ => throw new Exception("Unknown op")
                        };
                        stack.Push(val.ToString());
                    }
                    else
                    {
                        string temp = $"t{tempVarCounter++}";
                        code.Add($"{temp} = {op1} {token} {op2}");
                        stack.Push(temp);
                    }
                }
            }

            return code;
        }

        static bool IsNumber(string s)
        {
            return int.TryParse(s, out _);
        }
    }
}
