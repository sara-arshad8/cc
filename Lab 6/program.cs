using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FirstSetsConsoleApp
{
    class Program
    {
        static Dictionary<string, List<string[]>> productionRules = new Dictionary<string, List<string[]>>();
        static Dictionary<string, HashSet<string>> firstSets = new Dictionary<string, HashSet<string>>();

        static void Main(string[] args)
        {
            Console.WriteLine("Enter production rules (e.g., S->A B). Type 'end' to finish:");

            string input;
            bool isValid = true;

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

                // Validate non-terminal
                if (!Regex.IsMatch(lhs, @"^[A-Z][A-Za-z0-9']*$"))
                {
                    Console.WriteLine($"Error: Invalid non-terminal '{lhs}'. Must start with a capital letter.");
                    isValid = false;
                    continue;
                }

                // Split alternatives
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
                var first = ComputeFirst(rule);
                firstSets[rule] = first;
            }

            // Display FIRST sets
            Console.WriteLine("\nFIRST Sets:");
            foreach (var entry in firstSets)
            {
                Console.WriteLine($"First({entry.Key}) = {{ {string.Join(", ", entry.Value)} }}");
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
                    result.Remove("~"); // Temporarily remove epsilon

                    if (!firstOfCurrent.Contains("~"))
                        break;

                    // If it's the last symbol and all previous had epsilon, add epsilon
                    if (i == production.Length - 1)
                        result.Add("~");
                }
            }

            firstSets[symbol] = result;
            return result;
        }
    }
}
