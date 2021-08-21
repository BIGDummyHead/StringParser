using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace CommandParser
{
    /// <summary>
    /// A handler to invoke commands.
    /// </summary>
    public sealed class CommandHandler
    {
        private Dictionary<string, CommandAttribute> _commands = new Dictionary<string, CommandAttribute>();
        private Dictionary<string, object> _instances = new Dictionary<string, object>();
        private Dictionary<string, MethodInfo> _methods = new Dictionary<string, MethodInfo>();
        private Dictionary<Type, BaseCommandModule> modules = new Dictionary<Type, BaseCommandModule>();

        /// <summary>
        /// Commands being invoked.
        /// </summary>
        public IReadOnlyDictionary<string, CommandAttribute> Commands => _commands;

        /// <summary>
        /// Options for your Handler
        /// </summary>
        public HandlerConfig Options { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public CommandHandler(HandlerConfig config)
        {
            Options = config;
        }

        /// <summary>
        /// Create a command handler with the <see cref="HandlerConfig.Default"/>
        /// </summary>
        public CommandHandler() : this(HandlerConfig.Default)
        {

        }

        /// <summary>
        /// Invoke a command, must start with the prefix - name - arguments
        /// <para>Example: !name arg1 arg2</para>
        /// </summary>
        /// <param name="invoker"></param>
        /// <exception cref="Exceptions.InvalidConversionException"></exception>
        public void Invoke(string invoker)
        {
            string[] words = GetWords(invoker);

            if (words.Length < 1)
            {
                Options.ErrorWriter.WriteLine("Words length invalid");
                return;
            }
            else if (!words[0].StartsWith(Options.Prefix))
            {
                Options.ErrorWriter.WriteLine("Prefix invalid");
                return;
            }

            string commandName = Options.HasPrefix ? words[0][1..] : words[0];

            if (!Commands.ContainsKey(commandName, Options.comp))
            {
                Options.ErrorWriter.WriteLine($"'{commandName}' is not registered");
                return;
            }
            string[] arguments = words[1..];

            MethodInfo method = _methods.GetValue(commandName, Options.comp);
            ParameterInfo[] ps = method.GetParameters();
            if (arguments.Length < ps.Length)
            {
                Options.ErrorWriter.WriteLine("Arguments do not match length");
                return;
            }

            if (arguments.Length > ps.Length)
            {
                if (ps[^1].GetCustomAttribute<RemainingTextAttribute>() != null)
                {
                    string[] aa = arguments[(ps.Length - 1)..];

                    string a = string.Empty;

                    foreach (string i in aa)
                    {
                        a += " ";
                        a += i;
                    }

                    a = a.Trim();

                    List<string> wri = new List<string>();
                    for (int i = 0; i < ps.Length - 1; i++)
                    {
                        wri.Add(arguments[i]);
                    }

                    wri.Add(a);

                    arguments = wri.ToArray();
                }
            }


            List<object> methodInvoke = new List<object>();
            for (int i = 0; i < arguments.Length; i++)
            {
                bool converted = ConvertString(arguments[i], ps[i].ParameterType, out object o, out string er);

                if (!converted)
                {
                    Options.ErrorWriter.WriteLine(er);
                }
                else
                    methodInvoke.Add(o);
            }

            IEnumerable<BaseCommandAttribute> cmdAttrs = method.GetCustomAttributes<BaseCommandAttribute>();
            object clsInstance = _instances.GetValue(commandName, StringComparison.OrdinalIgnoreCase);
            object[] invokes = methodInvoke.ToArray();

            bool cont = true;

            if (method.GetParameters().Length == invokes.Length)
            {
                foreach (BaseCommandAttribute attr in cmdAttrs)
                {
                    bool im = attr.BeforeCommandExecute(clsInstance, invokes);

                    if (!im)
                        cont = false;
                }

                if (cont)
                {
                    modules[clsInstance.GetType()].OnCommandExecute(method, clsInstance, invokes);

                    method.Invoke(clsInstance, invokes);

                    foreach (BaseCommandAttribute attr in cmdAttrs)
                    {
                        attr.AfterCommandExecute(clsInstance, invokes);
                    }
                }
            }
        }

        private bool ConvertString(string from, Type info, out object conversion, out string error)
        {
            try
            {
                if (info == typeof(string))
                {
                    error = string.Empty;
                    conversion = from;
                    return true;
                }

                if (info.IsClass)
                {
                    if (info.GetConstructor(new Type[] { typeof(string) }) == null)
                        throw new Exceptions.InvalidConversionException(info, typeof(string));

                    error = string.Empty;
                    conversion = Activator.CreateInstance(info, from);
                    return true;
                }

                TypeConverter con = TypeDescriptor.GetConverter(info);

                error = string.Empty;
                conversion = con.ConvertFromString(from);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;

            }

            conversion = null;
            return false;
        }

        private string[] GetWords(string literal)
        {
            string emp = string.Empty;
            List<string> iz = new List<string>();
            foreach (char item in literal)
            {
                if (item.Equals(' '))
                {
                    iz.Add(emp);
                    emp = "";
                }
                else
                    emp += item;
            }

            if (emp != string.Empty)
                iz.Add(emp);

            return iz.ToArray();
        }

        /// <summary>
        /// Register a type with <see cref="CommandAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exceptions.InvalidModuleException"></exception>
        /// <exception cref="Exceptions.CommandExistException"></exception>
        public void Register<T>() where T : BaseCommandModule
        {
            Register(typeof(T));
        }

        /// <summary>
        /// Register a type with <see cref="CommandAttribute"/>, must inherit <seealso cref="BaseCommandModule"/>
        /// </summary>
        /// <param name="reg"></param>
        /// <exception cref="Exceptions.InvalidModuleException"></exception>
        /// <exception cref="Exceptions.CommandExistException"></exception>
        public void Register(Type reg)
        {
            if (!reg.Inherits(typeof(BaseCommandModule)))
                throw new Exceptions.InvalidModuleException(reg, $"does not inherit '{typeof(BaseCommandModule).Name}.");
            else if (reg.GetConstructor(Array.Empty<Type>()) == null || reg.IsAbstract)
                throw new Exceptions.InvalidModuleException(reg, "does not have an empty constructor, or an instance of it can not be made.");

            var z = reg.GetMethods((BindingFlags)(-1));

            foreach (var x in z)
            {
                CommandAttribute cmd = x.GetCustomAttribute<CommandAttribute>();
                IgnoreAttribute ig = x.GetCustomAttribute<IgnoreAttribute>();

                if (cmd is not null && ig is null)
                {
                    object i = Activator.CreateInstance(x.DeclaringType);
                    AddCommand(cmd, i, x);
                    modules.Add(reg, (BaseCommandModule)i);
                }
            }
        }

        private void AddCommand(CommandAttribute cmd, object instance, MethodInfo info)
        {
            if (cmd.UsingMethodName)
                cmd.CommandName = info.Name;

            string name = cmd.CommandName;
            //both _commands and _instances contain the same keys
            if (Commands.ContainsKey(name))
                throw new Exceptions.CommandExistException(name);

            _commands.Add(name, cmd);
            _instances.Add(name, instance);
            _methods.Add(name, info);
        }

    }

}
