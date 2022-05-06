using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StringParser
{
    /// <summary>
    /// Specific info about a command, for allowing more commands
    /// </summary>
    public struct CollectedCommand
    {
        /// <summary>
        /// Name provided 
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Amount of arguments to invoke the method info
        /// </summary>
        public int ParameterCount { get; internal set; }

        internal CollectedCommand(string name, CommandAttribute cmdAttr, object instance, MethodInfo method)
        {
            Name = name;

            this.cmdAttr = cmdAttr;
            this.instance = instance;
            this.method = method;
            isIgnored = method.GetCustomAttribute<IgnoreAttribute>() != null;
            parameters = method.GetParameters();

            ParameterCount = parameters.Length;

            List<KeyValuePair<ParameterInfo, IEnumerable<CommandParameterAttribute>>> ls = new();
            foreach (ParameterInfo pi in parameters)
            {
                var cpa = pi.GetCustomAttributes<CommandParameterAttribute>();

                if (cpa == null)
                    continue;

                ls.Add(new(pi, cpa));
            }

            parameterAttributes = new Dictionary<ParameterInfo, IEnumerable<CommandParameterAttribute>>(ls);
            ls.GetEnumerator().Dispose();

        }

        /// <summary>
        /// The <see cref="CommandAttribute"/> of the command
        /// </summary>
        public readonly CommandAttribute cmdAttr;
        /// <summary>
        /// The instance of the type.
        /// </summary>
        public readonly object instance;
        /// <summary>
        /// The method to invoke.
        /// </summary>
        public readonly MethodInfo method;

        /// <summary>
        /// Is the method ignored.
        /// </summary>
        public readonly bool isIgnored;

        /// <summary>
        /// Parameters collected from the method.
        /// </summary>
        public readonly ParameterInfo[] parameters;

        /// <summary>
        /// <see cref="CommandParameterAttribute"/>(s) from the <seealso cref="method"/>.
        /// </summary>

        public readonly IReadOnlyDictionary<ParameterInfo, IEnumerable<CommandParameterAttribute>> parameterAttributes;


        /// <summary>
        /// Checks if the left and right have the same name and parameter count.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static bool operator ==(CollectedCommand left, CollectedCommand right)
        {
            //ensure that left and right are not null or white space names
            if (string.IsNullOrWhiteSpace(left.Name) || string.IsNullOrWhiteSpace(right.Name))
                return false;

            return left.Name.Equals(right.Name, StringComparison.OrdinalIgnoreCase)
                && left.ParameterCount == right.ParameterCount;
        }

        /// <summary>
        /// Checks if the left and right do not have the same name and parameter count.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static bool operator !=(CollectedCommand left, CollectedCommand right)
        {
            return !(left == right);
        }

        /// <summary>
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(CollectedCommand))
                return false;

            return this == (CollectedCommand)obj;
        }

    }

}
