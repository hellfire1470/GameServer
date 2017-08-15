using System;

public static class Logger
{
    private static bool _useHeader = true;
    private static string _logDir;
    private static string _logFile;
    private static bool _checkPath = false;

    public static string LogDir { 
        get {
            return _logDir;
        }
        set {
            _checkPath = true;
            _logDir = value;
        } 
    }

    public static string LogFile {
        get {
            return _logFile;
        }
        set {
            _checkPath = true;
            _logFile = value;
        }
    }

    private static void CheckLogPath()
    {
        if(_checkPath)
        {
            _checkPath = false;
            if (!System.IO.Directory.Exists(LogDir))
			{
                System.IO.Directory.CreateDirectory(LogDir);
			}
            if (!System.IO.File.Exists(LogDir + LogFile))
			{
                System.IO.FileStream stream = System.IO.File.Create(LogDir + LogFile);
                stream.Close();
			}
        }
    }

    public static bool LogToFile{ get; set; } = false;

    private static string GetHeader()
    {
        if (_useHeader)
        {
            return "[" + DateTime.Now.ToString("HH:mm:ss:fff") + "] ";
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

        LogInFile(str);
        Console.Write(str);
    }

    private static void LogInFile(string str)
    {
        if (LogToFile && LogDir != "" && LogFile != "")
        {
            CheckLogPath();
            System.IO.StreamWriter writer = System.IO.File.AppendText(LogDir + LogFile);
            writer.Write(str);
            writer.Close();
        }
    }

    public static void Error(string str)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        str = GetHeader() + "[Error] " + str;

        LogInFile(str);
        Console.WriteLine(str);

        Console.ResetColor();
    }

}
