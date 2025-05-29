using System;
using System.Linq;
using System.Text;

class RandomPasswordGenerator
{
    static void Main()
    {
        // Collecting user input for password generation
        Console.Write("Enter your First Name: ");
        string firstName = Console.ReadLine();

        Console.Write("Enter your Last Name: ");
        string lastName = Console.ReadLine();

        Console.Write("Enter your Registration Number: ");
        string registrationNumber = Console.ReadLine();

        Console.Write("Enter your Favorite Movie: ");
        string favoriteMovie = Console.ReadLine();

        Console.Write("Enter your Favorite Food: ");
        string favoriteFood = Console.ReadLine();

        // Generate and display the random password
        string password = GenerateRandomPassword(firstName, lastName, registrationNumber, favoriteMovie, favoriteFood);
        Console.WriteLine("\nGenerated Password: " + password);
        Console.ReadKey();
    }

    static string GenerateRandomPassword(string firstName, string lastName, string registrationNumber, string favoriteMovie, string favoriteFood)
    {
        Random random = new Random();
        StringBuilder passwordBuilder = new StringBuilder();

        // Add parts of user input for personalization
        passwordBuilder.Append(firstName.Substring(0, Math.Min(3, firstName.Length))); // First 3 characters of First Name
        passwordBuilder.Append(lastName.Substring(0, Math.Min(3, lastName.Length))); // First 3 characters of Last Name
        passwordBuilder.Append(registrationNumber.Substring(0, Math.Min(4, registrationNumber.Length))); // First 4 characters of Registration Number
        passwordBuilder.Append(favoriteMovie.Substring(0, Math.Min(3, favoriteMovie.Length))); // First 3 characters of Favorite Movie
        passwordBuilder.Append(favoriteFood.Substring(0, Math.Min(3, favoriteFood.Length))); // First 3 characters of Favorite Food

        // Add random digits
        for (int i = 0; i < 2; i++)
        {
            passwordBuilder.Append(random.Next(0, 10)); // Append a random digit
        }

        // Add random special characters
        char[] specialCharacters = { '!', '@', '#', '$', '%', '^', '&', '*' };
        passwordBuilder.Append(specialCharacters[random.Next(specialCharacters.Length)]); // Add a random special character

        // Add a random uppercase letter
        passwordBuilder.Append((char)random.Next('A', 'Z' + 1)); // Add a random uppercase letter

        // Add random lowercase letter
        passwordBuilder.Append((char)random.Next('a', 'z' + 1)); // Add a random lowercase letter

        // Shuffle the characters to ensure randomness
        string password = new string(passwordBuilder.ToString().OrderBy(c => random.Next()).ToArray());

        // Ensure the password has a minimum length of 12 characters
        while (password.Length < 12)
        {
            password += (char)random.Next('a', 'z' + 1); // Add random lowercase letter if password is too short
        }

        return password;
    }
}
