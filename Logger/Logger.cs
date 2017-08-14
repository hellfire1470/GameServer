using System;

public static class Logger
{
    private static bool _useHeader = true;

    private static string GetHeader()
    {
        if (_useHeader)
        {
            return "[" + DateTime.Now.Hour.ToString("D2") + ":" + DateTime.Now.Minute.ToString("D2") + ":" + DateTime.Now.Second.ToString("D2") + ":" + DateTime.Now.Millisecond.ToString("D3") + "] ";
        }
        return "";
    }

    public static void Log(string str, bool keepLine = false)
    {
        str = str.Insert(0, GetHeader());

        if (keepLine)
        {
            _useHeader = false;
        }
        else
        {
            _useHeader = true;
            str += "\n";
        }

        Console.Write(str);
    }

    public static void Error(string str)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(GetHeader() + "[Error] " + str);

        Console.ResetColor();
    }
}
