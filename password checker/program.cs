using System;
using System.Linq;
using System.Text.RegularExpressions;

class PasswordChecker
{
    static void Main()
    {
        // Example Registration Number (replace with actual registration number)
        string registrationNumber = "017";

        // Example Name (replace with your actual name)
        string name = "AkashaLoulak";

        // User Input for Password
        Console.Write("Enter your password: ");
        string password = Console.ReadLine();


        // Ensure the password is not empty
        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("You must enter a password.");
            return; // Exit the program if no password is entered
        }

        // Check if the password meets all the conditions
        if (IsPasswordValid(password, registrationNumber, name))
        {
            Console.WriteLine("Password is valid.");
        }
        else
        {
            Console.WriteLine("Password is invalid.");
        }

        Console.ReadKey();
    }

    static bool IsPasswordValid(string password, string registrationNumber, string name)
    {
        // Condition 1: Maximum length is 12 characters
        if (password.Length > 12)
        {
            Console.WriteLine("Password is too long. Maximum length is 12.");
            return false;
        }

        // Condition 2: Must contain at least 2 characters from the registration number
        if (!Regex.IsMatch(password, $"[{registrationNumber}]"))
        {
            Console.WriteLine("Password does not contain enough characters from the registration number.");
            return false;
        }

        // Condition 3: Must contain at least one uppercase alphabet
        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            Console.WriteLine("Password must contain at least one uppercase letter.");
            return false;
        }

        // Condition 4: At least 2 special characters in order
        if (!HasTwoSpecialCharactersInOrder(password))
        {
            Console.WriteLine("Password must contain at least two special characters in order.");
            return false;
        }

        // Condition 5: Must contain at least 4 lowercase letters from the name
        int lowercaseLettersFromName = password.Count(c => name.IndexOf(c.ToString(), StringComparison.OrdinalIgnoreCase) >= 0 && char.IsLower(c));
        if (lowercaseLettersFromName < 4)
        {
            Console.WriteLine("Password must contain at least 4 lowercase letters from the name.");
            return false;
        }

        return true;
    }

    static bool HasTwoSpecialCharactersInOrder(string password)
    {
        // Define a list of special characters in the order you need
        char[] specialCharacters = new char[] { '@', '#', '$', '%', '^', '&', '*' };

        // Check if there are two consecutive special characters in the password
        for (int i = 0; i < password.Length - 1; i++)
        {
            if (specialCharacters.Contains(password[i]) && specialCharacters.Contains(password[i + 1]))
            {
                return true;
            }
        }

        return false;
    }
}
