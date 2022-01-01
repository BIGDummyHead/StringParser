namespace CommandParser
{
    //an importance enum with 4 levels
    /// <summary>
    /// Determine the level of importance of the parameter
    /// </summary>
    public enum Importance : int
    {
        /// <summary>
        /// The parameter is not important
        /// </summary>
        Low = 0,
        /// <summary>
        /// The parameter is important
        /// </summary>
        Medium = 10,
        /// <summary>
        /// The parameter is very important
        /// </summary>
        High = 20,
        /// <summary>
        /// The parameter is extremely important
        /// </summary>
        Critical = 30
    }
}