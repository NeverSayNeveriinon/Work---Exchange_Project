using System.Text;

namespace Core.Helpers;

public static class Generator
{
    static Random res = new Random(); 

    public static string RandomString()
    {
        // String that contain both alphabets and numbers 
        String str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; 
        int size = 10; 
  
        // Initializing the empty string 
        StringBuilder randomstring = new StringBuilder(); 
  
        for (int i = 0; i < size; i++) 
        { 
            // Selecting a index randomly 
            int x = res.Next(str.Length); 
  
            // Appending the character at the  
            // index to the random alphanumeric string. 
            randomstring.Append(str[x]); 
        }

        return randomstring.ToString();
    }
}