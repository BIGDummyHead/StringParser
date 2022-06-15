namespace StringParser;

/// <summary>
/// Logging levels
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Basic information sent for debugging purposes for the library
    /// </summary>
    Debug,
    /// <summary>
    /// Information sent for user
    /// </summary>
    Information,
    /// <summary>
    /// Warning sent for user. Usually sent by the library when it detects a problem that can be fixed.
    /// </summary>
    Warning,

    /// <summary>
    /// Error sent for user. Usually sent by the library when it detects an exception that was caught but not thrown.
    /// </summary>
    Error
}
