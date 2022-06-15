namespace StringParser.Interfaces;

/// <summary>
/// A logging interface
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Any log Level message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="lvl"></param>
    void Log(string message, LogLevel lvl);
}


