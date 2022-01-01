using System.Collections.Generic;

namespace CommandParser
{
    internal class CPAComparer : IComparer<CommandParameterAttribute>
    {
        //compares the importance of the attributes
        public int Compare(CommandParameterAttribute x, CommandParameterAttribute y)
        {
            if (x.importance == y.importance)
                return 0;

            if (x.importance == Importance.Critical)
                return -1;

            return 1;
        }
    }
}