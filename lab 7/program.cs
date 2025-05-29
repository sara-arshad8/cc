using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FirstFollowSetsApp
{
    class Program
    {
        static Dictionary<string, List<string[]>> productionRules = new Dictionary<string, List<string[]>>();
        static Dictionary<string, HashSet<string>> firstSets = new Dictionary<string, HashSet<string>>();
        static Dictionary<string, HashSet<string>> followSets = new Dictionary<string, HashSet<string>>();

        static void Main(string[] args)
        {
            Console.WriteLine("Enter production rules (e.g., S->A B). Type 'end' to finish:");

            string input;
            bool isValid = true;
            string startSymbol = null;

            while (true)
            {
                input = Console.ReadLine();
                if (input.Trim().ToLower() == "end") break;
                if (string.IsNullOrWhiteSpace(input)) continue;

                var temp = input.Split(new[] { "->" }, StringSplitOptions.None);
                if (temp.Length != 2)
                {
                    Console.WriteLine("Invalid rule format. Use format: S->A B");
                    isValid = false;
                    continue;
                }

                string lhs = temp[0].Trim();
                string rhs = temp[1].Trim();

                if (startSymbol == null)
                    startSymbol = lhs; // first non-terminal is assumed start symbol

                if (!Regex.IsMatch(lhs, @"^[A-Z][A-Za-z0-9']*$"))
                {
                    Console.WriteLine($"Error: Invalid non-terminal '{lhs}'. Must start with a capital letter.");
                    isValid = false;
                    continue;
                }

                var alternatives = rhs.Split('|');
                foreach (var alt in alternatives)
                {
                    var symbols = alt.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (!productionRules.ContainsKey(lhs))
                        productionRules[lhs] = new List<string[]>();

                    productionRules[lhs].Add(symbols);
                }
            }

            if (!isValid)
            {
                Console.WriteLine("\nFix the errors in the production rules and try again.");
                return;
            }

            // Compute FIRST sets
            foreach (var rule in productionRules.Keys)
            {
                ComputeFirst(rule);
            }

            // Initialize FOLLOW sets
            foreach (var nt in productionRules.Keys)
            {
                followSets[nt] = new HashSet<string>();
            }

            // Add $ to FOLLOW(start symbol)
            followSets[startSymbol].Add("$");

            // Compute FOLLOW sets
            foreach (var rule in productionRules.Keys)
            {
                ComputeFollow(rule);
            }

            // Display FIRST sets
            Console.WriteLine("\nFIRST Sets:");
            foreach (var entry in firstSets)
            {
                Console.WriteLine($"First({entry.Key}) = {{ {string.Join(", ", entry.Value)} }}");
            }

            // Display FOLLOW sets
            Console.WriteLine("\nFOLLOW Sets:");
            foreach (var entry in followSets)
            {
                Console.WriteLine($"Follow({entry.Key}) = {{ {string.Join(", ", entry.Value)} }}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static HashSet<string> ComputeFirst(string symbol)
        {
            if (firstSets.ContainsKey(symbol))
                return firstSets[symbol];

            var result = new HashSet<string>();

            if (!productionRules.ContainsKey(symbol))
            {
                result.Add(symbol); // It's a terminal
                return result;
            }

            foreach (var production in productionRules[symbol])
            {
                for (int i = 0; i < production.Length; i++)
                {
                    var current = production[i];

                    if (current == "~")
                    {
                        result.Add("~");
                        break;
                    }

                    var firstOfCurrent = ComputeFirst(current);
                    result.UnionWith(firstOfCurrent);
                    result.Remove("~");

                    if (!firstOfCurrent.Contains("~"))
                        break;

                    if (i == production.Length - 1)
                        result.Add("~");
                }
            }

            firstSets[symbol] = result;
            return result;
        }

        static void ComputeFollow(string nonTerminal)
        {
            foreach (var head in productionRules.Keys)
            {
                foreach (var production in productionRules[head])
                {
                    for (int i = 0; i < production.Length; i++)
                    {
                        if (production[i] == nonTerminal)
                        {
                            bool followedBySomething = false;

                            for (int j = i + 1; j < production.Length; j++)
                            {
                                followedBySomething = true;
                                var nextSymbol = production[j];

                                var firstOfNext = ComputeFirst(nextSymbol);
                                followSets[nonTerminal].UnionWith(firstOfNext);
                                followSets[nonTerminal].Remove("~");

                                if (firstOfNext.Contains("~"))
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            // if nonTerminal is at end or all following symbols can be epsilon
                            if (!followedBySomething || (i + 1 < production.Length && ComputeFirst(production[i + 1]).Contains("~")))
                            {
                                if (nonTerminal != head) // prevent self loop
                                    followSets[nonTerminal].UnionWith(followSets[head]);
                            }
                        }
                    }
                }
            }
        }
    }
}
