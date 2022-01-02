namespace CommandParser.Interfaces;

/// <summary>
/// A logging interface
/// </summary>
public interface ILog
{
    /// <summary>
    /// Any log Level message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="lvl"></param>
    void Log(string message, LogLevel lvl);

    /// <summary>
    /// When the <see cref="LogLevel"/> is of Debug
    /// </summary>
    /// <param name="message"></param>
    void LogDebug(string message);

    /// <summary>
    /// When the <see cref="LogLevel"/> is of Information 
    /// </summary>
    /// <param name="message"></param>
    void LogInfo(string message);

    /// <summary>
    /// When the <see cref="LogLevel"/> is a Warning 
    /// </summary>
    /// <param name="message"></param>
    void LogWarning(string message);

    /// <summary>
    /// When the <see cref="LogLevel"/> is an Error
    /// </summary>
    /// <param name="message"></param>
    void LogError(string message);
}
