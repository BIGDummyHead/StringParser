using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CommandParser.Helper
{
    /// <summary>
    /// Generate info about a module!
    /// </summary>
    public sealed class ModuleDescriptor
    {
        internal ModuleDescriptor()
        {

        }

        private Dictionary<string, SingularInfo> _info = new Dictionary<string, SingularInfo>();

        /// <summary>
        /// Dictionary containing names and info
        /// </summary>
        public IReadOnlyDictionary<string, SingularInfo> CommandDictionary => _info;

        /// <summary>
        /// Names of the Commands
        /// </summary>
        public IEnumerable<string> Names => CommandDictionary.Keys;
        /// <summary>
        /// Info about the commands
        /// </summary>
        public IEnumerable<SingularInfo> Commands => CommandDictionary.Values;

        /// <summary>
        /// Generates helpful info about a command module.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ModuleDescriptor GetDescriptor<T>() where T : BaseCommandModule
        {
            Type reg = typeof(T);

            MethodInfo[] methods = reg.GetMethods((BindingFlags)(-1));

            ModuleDescriptor descriptor = new ModuleDescriptor();

            foreach (MethodInfo mInfo in methods)
            {
                CommandAttribute cmd = mInfo.GetCustomAttribute<CommandAttribute>();

                if (cmd == null)
                    continue;
                else if (mInfo.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;

                DescriptionAttribute dA = mInfo.GetCustomAttribute<DescriptionAttribute>();

                string desc = string.Empty;

                if (dA != null)
                    desc = dA.description;

                descriptor._info.Add(cmd.CommandName, new SingularInfo(cmd.CommandName, desc));
            }

            return descriptor;
        }
    }

    /// <summary>
    /// Info about one command
    /// </summary>
    public readonly struct SingularInfo
    {
        /// <summary>
        /// Name of the command
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Description of the name if any
        /// </summary>
        public readonly string Description;

        //add anything else here.

        //add anything else here.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        public SingularInfo(string name, string desc)
        {
            Name = name;
            Description = desc;
        }

    }
}
