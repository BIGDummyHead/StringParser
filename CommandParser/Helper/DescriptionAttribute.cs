using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandParser.Helper
{
    /// <summary>
    /// Provide a description about a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute
    {
        /// <summary>
        /// The description of the command
        /// </summary>
        public readonly string description;

        /// <summary>
        /// </summary>
        public DescriptionAttribute(string description)
        {
            this.description = description; 
        }
    }
}
