using System.Text;

namespace Core.Helpers;

public static class Generator
{
    private static readonly Random _random = new Random(); 

    public static string RandomString()
    {
        // String that contains both alphabets and numbers 
        string allCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; 
        int size = 10; 
  
        // Initializing the empty string 
        StringBuilder randomstring = new StringBuilder(); 
  
        for (int i = 0; i < size; i++) 
        { 
            // Selecting an index randomly 
            int x = _random.Next(allCharacters.Length); 
  
            // Appending the 'character at the x index of 'allCharacters'' to the random alphanumeric string. 
            randomstring.Append(allCharacters[x]); 
        }

        return randomstring.ToString();
    }
}