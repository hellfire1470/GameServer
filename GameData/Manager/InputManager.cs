using System;
namespace GameData.Manager
{
    public class InputManager
    {

        public InputManager()
        {
        }

        public static bool ContainsOnly(char[] chars, string str)
        {
            if (str.Length == 0) return false;
            char[] strInChars = str.ToLower().ToCharArray();
            foreach (char charInName in strInChars)
            {
                bool charIsValid = false;
                foreach (char allowedChar in chars)
                {
                    if (charInName == allowedChar)
                    {
                        charIsValid = true;
                        break;
                    }
                }
                if (!charIsValid)
                {
                    return false;
                }
            }
            return false;
        }
    }
}
