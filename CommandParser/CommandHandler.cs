using CommandParser.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CommandParser
{
    /// <summary>
    /// A handler to invoke commands.
    /// </summary>
    public sealed class CommandHandler
    {
        private readonly Dictionary<CommandInfo, CommandAttribute> _commands = new Dictionary<CommandInfo, CommandAttribute>();
        private readonly Dictionary<CommandInfo, object> _instances = new Dictionary<CommandInfo, object>();
        private readonly Dictionary<CommandInfo, MethodInfo> _methods = new Dictionary<CommandInfo, MethodInfo>();
        private readonly Dictionary<Type, BaseCommandModule> modules = new Dictionary<Type, BaseCommandModule>();
        private readonly List<ConverterHelper> helpers = new List<ConverterHelper>();


        /// <summary>
        /// Commands being invoked.
        /// </summary>
        public IReadOnlyDictionary<CommandInfo, CommandAttribute> Commands => _commands;


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

            helpers.Add(ConverterHelper.Create(delegate (string parse) { return parse; })); //add in basic converters here

            helpers.Add(ConverterHelper.Create(delegate (string parse)
            {
                if (int.TryParse(parse, out int result))
                    return result;

                return 0;
            }));

            helpers.Add(ConverterHelper.Create(delegate (string parse)
            {
                if (double.TryParse(parse, out double result))
                    return result;
                
                return 0;
            }));

            helpers.Add(ConverterHelper.Create(delegate (string parse)
            {
                if (float.TryParse(parse, out float result))
                    return result;

                return 0;
            }));
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
            string[] words = invoker.Split(' ');

            if (words.Length < 1)
            {
                Options.SendMessage("Words length invalid");
                return;
            }
            else if (!words[0].StartsWith(Options.Prefix))
            {
                Options.SendMessage("Prefix invalid");
                return;
            }

            string commandName = Options.HasPrefix ? words[0][1..] : words[0];
            string[] arguments = words[1..];

            CommandInfo _cmdInfo = new CommandInfo(commandName, arguments.Length);

            if (Commands.Keys.FirstOrDefault(x => x.Name == _cmdInfo.Name) == default)
            {
                Options.SendMessage($"'{commandName}' is not registered");
                return;
            }

            List<object> methodInvoke = new List<object>();
            MethodInfo success = null;

            foreach (KeyValuePair<CommandInfo, MethodInfo> cM in _methods)
            {
                MethodInfo method = cM.Value;

                ParameterInfo[] ps = method.GetParameters();
                if (arguments.Length < ps.Length)
                {
                    Options.SendMessage("Arguments do not match length");
                    continue;
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
                    else
                        continue;
                }


                for (int i = 0; i < arguments.Length; i++)
                {
                    bool converted = ConvertString(arguments[i], ps[i].ParameterType, out object o, out string er);

                    if (!converted)
                    {
                        Options.SendMessage(er);
                        methodInvoke.Clear();
                        continue;
                    }
                    else
                        methodInvoke.Add(o);
                }

                success = method;
                _cmdInfo = new CommandInfo(_cmdInfo.Name, arguments.Length);
            }

            if (success == null)
            {
                Options.SendMessage("Could not find any commands to invoke");
                return;
            }

            IEnumerable<BaseCommandAttribute> cmdAttrs = success.GetCustomAttributes<BaseCommandAttribute>();
            object clsInstance = _instances.GetValue(_cmdInfo);
            object[] invokes = methodInvoke.ToArray();

            bool cont = true;

            if (success.GetParameters().Length == invokes.Length)
            {
                foreach (BaseCommandAttribute attr in cmdAttrs)
                {
                    bool im = attr.BeforeCommandExecute(clsInstance, invokes);

                    if (!im)
                        cont = false;
                }

                if (cont)
                {
                    modules[clsInstance.GetType()].OnCommandExecute(success, clsInstance, invokes);

                    success.Invoke(clsInstance, invokes);

                    foreach (BaseCommandAttribute attr in cmdAttrs)
                    {
                        attr.AfterCommandExecute(clsInstance, invokes);
                    }
                }
            }
        }

        private bool ConvertString(string from, Type info, out object conversion, out string error)
        {
            if (Contains(info, out int index))
            {
                ConverterHelper handler = helpers[index];

                conversion = handler.Convert(from);
                error = string.Empty;

                return true;
            }

            try
            {
                if (info.IsClass) //checks for default constructors with strings
                {
                    if (info.GetConstructor(new Type[] { typeof(string) }) == null)
                        throw new Exceptions.InvalidConversionException(typeof(string), info);

                    error = string.Empty;
                    conversion = Activator.CreateInstance(info, from);
                    return true;
                }

                //trys to convert string to type
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

        /// <summary>
        /// Checks if a type can be converted from string, by checking registered conversion types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool CanConvert<T>()
        {
            return helpers.Any(x => x.ConversionType == typeof(T));
        }














        //registration and such below//












        /// <summary>
        /// Register a type with <see cref="CommandAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exceptions.InvalidModuleException"></exception>
        /// <exception cref="Exceptions.CommandExistException"></exception>
        public void RegisterModule<T>() where T : BaseCommandModule
        {
            RegisterModule(typeof(T));
        }

        /// <summary>
        /// Register a type with <see cref="CommandAttribute"/>, must inherit <seealso cref="BaseCommandModule"/>
        /// </summary>
        /// <param name="reg"></param>
        /// <exception cref="Exceptions.InvalidModuleException"></exception>
        /// <exception cref="Exceptions.CommandExistException"></exception>
        public void RegisterModule(Type reg)
        {
            if (!reg.Inherits(typeof(BaseCommandModule)))
                throw new Exceptions.InvalidModuleException(reg, $"does not inherit '{typeof(BaseCommandModule).Name}.");
            else if (reg.GetConstructor(Array.Empty<Type>()) == null || reg.IsAbstract)
                throw new Exceptions.InvalidModuleException(reg, "does not have an empty constructor, or an instance of it can not be made.");

            MethodInfo[] typeMethods = reg.GetMethods((BindingFlags)(-1)); //get all methods of all kinds

            //create an instance for invoking later on down the line
            object i = Activator.CreateInstance(reg);

            foreach (MethodInfo commandMethod in typeMethods)
            {
                CommandAttribute cmd = commandMethod.GetCustomAttribute<CommandAttribute>();
                IgnoreAttribute ig = commandMethod.GetCustomAttribute<IgnoreAttribute>(); //check if we should ignore adding in this command

                if (cmd is not null && ig is null)
                {
                    AddCommand(cmd, i, commandMethod);
                }
            }

            modules.Add(reg, (BaseCommandModule)i);
        }


        /// <summary>
        /// Register a generic converter that can convert a string type into your T type.
        /// </summary>
        /// <typeparam name="T">Conversion choice</typeparam>
        /// <param name="converter">Provided handler</param>
        public void RegisterConverter<T>(IConverter<T> converter)
        {
            if (Contains(typeof(T)))
            {
                Options.SendMessage($"Handler for {typeof(T).FullName} already exist.");
                return;
            }

            helpers.Add(ConverterHelper.Create(converter));
        }

        /// <summary>
        /// Gets rid of a converter of a specific type
        /// </summary>
        /// <param name="converter"></param>

        public void UnRegisterConverter(Type converter)
        {
            if (!Contains(converter))
            {
                Options.SendMessage($"Handler does not exist for {converter.FullName}.");
                return;
            }

            helpers.Remove(helpers.FirstOrDefault(x => x.ConversionType == converter));
        }

        /// <summary>
        /// Gets rid of a converter of specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnRegisterConverter<T>() => UnRegisterConverter(typeof(T));

        /// <summary>
        /// Fully unregisters a module
        /// </summary>
        /// <param name="unreg"></param>
        public void UnRegisterModule(Type unreg)
        {
            if (!unreg.Inherits(typeof(BaseCommandModule)))
                return;
            else if (!modules.ContainsKey(unreg))
                return;

            foreach (MethodInfo method in unreg.GetMethods((BindingFlags)(-1)))
            {
                CommandAttribute cmdAttr = method.GetCustomAttribute<CommandAttribute>();

                if (cmdAttr == null)
                    continue;
                else if (method.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;

                CommandInfo inf = new CommandInfo(cmdAttr.CommandName, method.GetParameters().Length);

                foreach (KeyValuePair<CommandInfo, CommandAttribute> item in _commands)
                {
                    if (item.Key == inf)
                    {
                        inf = item.Key;
                        break;
                    }
                }

                _commands.Remove(inf);
                _instances.Remove(inf);
                _methods.Remove(inf);

                modules.Remove(unreg);
            }
        }

        /// <summary>
        /// Fully unregisters a module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnRegisterModule<T>() => UnRegisterModule(typeof(T));


        bool Contains(Type a, out int index) //ensures same message is displayed fairly.
        {
            index = 0;
            foreach (ConverterHelper item in helpers)
            {
                if (item.ConversionType == a)
                {
                    return true;
                }

                index++;
            }

            index = -1;
            return false;
        }

        bool Contains(Type a)
        {
            return Contains(a, out _);
        }

        private void AddCommand(CommandAttribute cmd, object instance, MethodInfo info)
        {
            if (cmd.UsingMethodName)
                cmd.CommandName = info.Name;

            CommandInfo commandInfo = new(cmd.CommandName, info.GetParameters().Length);

            //both _commands and _instances contain the same keys
            foreach (var command in Commands)
            {
                if (command.Key == commandInfo)
                    throw new Exceptions.CommandExistException(commandInfo.Name);
            }

            _commands.Add(commandInfo, cmd);
            _instances.Add(commandInfo, instance);
            _methods.Add(commandInfo, info);
        }

        //registration and such above//

    }

    /// <summary>
    /// Specific info about a command, for allowing more commands
    /// </summary>
    public struct CommandInfo
    {
        /// <summary>
        /// Name provided 
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Amount of arguments to invoke the method info
        /// </summary>
        public int ParameterCount { get; private set; }

        internal CommandInfo(string name, int count)
        {
            ParameterCount = count;
            Name = name;
        }

        /// <summary>
        /// Checks if the left and right have the same name and parameter count.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static bool operator ==(CommandInfo left, CommandInfo right)
        {
            return left.Name == right.Name && left.ParameterCount == right.ParameterCount;
        }

        /// <summary>
        /// Checks if the left and right do not have the same name and parameter count.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static bool operator !=(CommandInfo left, CommandInfo right)
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
            if (obj.GetType() != typeof(CommandInfo))
                return false;

            return this == (CommandInfo)obj;
        }

    }
}
