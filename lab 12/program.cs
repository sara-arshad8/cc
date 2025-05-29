using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LexicalAnalyzerConsole
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

        static void Main(string[] args)
        {
            Console.WriteLine("Enter your code (press Enter twice to finish):");
            string userInput = ReadMultilineInput();

            AnalyzeInput(userInput);

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

        static void AnalyzeInput(string userInput)
        {
            List<string> keywordList = new List<string> { "int", "float", "begin", "end", "print", "if", "else", "while", "main", "new" };

            Regex variable_Reg = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");
            Regex constants_Reg = new Regex(@"^[0-9]+(\.[0-9]+)?([eE][+-]?[0-9]+)?$");
            Regex operators_Reg = new Regex(@"^[+\-*/=<>!&|]$");
            Regex punctuations_Reg = new Regex(@"^[.,;:{}()\[\]]$");

            List<string> lines = new List<string>(userInput.Split('\n'));

            int line_num = 0;
            int varCount = 1;
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
                        Console.Write($"<keyword, {token}> ");
                    }
                    else if (constants_Reg.IsMatch(token))
                    {
                        Console.Write($"<digit, {token}> ");
                    }
                    else if (operators_Reg.IsMatch(token))
                    {
                        Console.Write($"<op, {token}> ");
                    }
                    else if (punctuations_Reg.IsMatch(token))
                    {
                        Console.Write($"<punc, {token}> ");
                    }
                    else if (variable_Reg.IsMatch(token))
                    {
                        if (!SymbolExists(token))
                        {
                            // Try to find declaration
                            string type = i > 0 && keywordList.Contains(tokens[i - 1]) ? tokens[i - 1] : "unknown";
                            string value = (i + 2 < tokens.Count && tokens[i + 1] == "=") ? tokens[i + 2] : "";
                            SymbolTable.Add(new Symbol(symbolIndex, token, type, value, line_num));
                            Console.Write($"<var{varCount}, {symbolIndex}> ");
                            symbolIndex++;
                            varCount++;
                        }
                        else
                        {
                            Symbol sym = GetSymbol(token);
                            Console.Write($"<var{sym.Index}, {sym.Index}> ");
                        }
                    }
                    else
                    {
                        Console.Write($"<unknown, {token}> ");
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine("\nSymbol Table:");
            Console.WriteLine("------------");
            Console.WriteLine("Index\tName\tType\tValue\tLine");

            foreach (var entry in SymbolTable)
            {
                Console.WriteLine($"{entry.Index}\t{entry.Name}\t{entry.Type}\t{entry.Value}\t{entry.Line}");
            }
        }

        static List<string> Tokenize(string line)
        {
            List<string> tokens = new List<string>();
            string current = "";
            foreach (char c in line)
            {
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
    }
}
