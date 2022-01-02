using CommandParser.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser
{
    /// <summary>
    /// A handler to invoke commands.
    /// </summary>
    public sealed class CommandHandler
    {
        internal readonly Dictionary<CommandInfo, CommandAttribute> _commands = new Dictionary<CommandInfo, CommandAttribute>();
        internal readonly Dictionary<CommandInfo, object> _instances = new Dictionary<CommandInfo, object>();
        internal readonly Dictionary<CommandInfo, MethodInfo> _methods = new Dictionary<CommandInfo, MethodInfo>();
        internal readonly Dictionary<Type, BaseCommandModule> modules = new Dictionary<Type, BaseCommandModule>();
        internal readonly List<ConverterHelper> helpers = new List<ConverterHelper>();


        /// <summary>
        /// Commands being invoked.
        /// </summary>
        public IReadOnlyDictionary<CommandInfo, CommandAttribute> Commands => _commands;


        /// <summary>
        /// Types of registered modules.
        /// </summary>
        public IEnumerable<Type> Modules => modules.Keys;


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
        public async Task Invoke(string invoker)
        {
            string[] words = invoker.Split(' ');

            if (words.Length < 1)
            {
                Options.ToLog("Not enough arguments sent", LogLevel.Information);
                return;
            }
            else if (!words[0].StartsWith(Options.Prefix))
            {
                Options.ToLog("Prefix invalid", LogLevel.Information);
                return;
            }

            string commandName = Options.HasPrefix ? words[0][1..] : words[0];
            string[] stringArguments = words[1..];

            CommandInfo mockInfo = new CommandInfo(commandName, stringArguments.Length);

            CommandInfo comparingResult = Commands.Keys.FirstOrDefault(x => x.Name.Equals(commandName, Options.Comp));

            if (comparingResult == default)
            {
                Options.ToLog($"'{commandName}' is not registered", LogLevel.Warning);
                return;
            }
            else
                mockInfo.Name = comparingResult.Name;

            List<object> methodInvoke = new List<object>(); //this list is responsible for the method invoking
            MethodInfo invokeableMethod = null;

            foreach (KeyValuePair<CommandInfo, MethodInfo> validMethods in _methods) //never use return in here!
            {
                MethodInfo method = validMethods.Value;

                ParameterInfo[] methodParameters = method.GetParameters();
                if (stringArguments.Length < methodParameters.Length)
                {
                    Options.ToLog("Arguments do not match length", LogLevel.Warning);
                    continue;
                }

                for (int i = 0; i < methodParameters.Length; i++)
                {
                    ParameterInfo loopX = methodParameters[i];

                    foreach (CommandParameterAttribute pinvokes in loopX.GetCustomAttributes<CommandParameterAttribute>().OrderByDescending(x => x.importance))
                    {
                        pinvokes.Handler = this;
                        //we should call this method in here because it can effect the total outcome of the command invokemennt
                        stringArguments = await pinvokes.OnCollect(loopX, stringArguments, methodParameters);
                    }
                }


                if (stringArguments.Length != methodParameters.Length) //catches a possible exception
                {
                    Options.ToLog("Slip. Argument length does not match the Parameter Info Length!", LogLevel.Error);
                    return;
                }

                for (int i = 0; i < stringArguments.Length; i++)
                {
                    bool converted = ConvertString(stringArguments[i], methodParameters[i].ParameterType, out object convertedObject, out string possibleError);
                    if (!converted)
                    {
                        Options.ToLog(possibleError, LogLevel.Error);
                        methodInvoke.Clear(); //clear invoke list
                        continue;
                    }
                    else
                        methodInvoke.Add(convertedObject);
                }

                invokeableMethod = method;
                mockInfo = new CommandInfo(mockInfo.Name, stringArguments.Length);
            }

            if (invokeableMethod == null)
            {
                Options.ToLog("Could not find any commands to invoke", LogLevel.Warning);
                return;
            }

            IEnumerable<BaseCommandAttribute> baseCommandAttributes = invokeableMethod.GetCustomAttributes<BaseCommandAttribute>();
            object moduleInstance = _instances.GetValue(mockInfo);
            object[] methodInvokeArray = methodInvoke.ToArray();

            if (invokeableMethod.GetParameters().Length == methodInvokeArray.Length)
            {
                int yea = 0, nei = 0;

                foreach (BaseCommandAttribute attr in baseCommandAttributes)
                {
                    if (await attr.BeforeCommandExecute(moduleInstance, methodInvokeArray))
                        yea++;
                    else
                        nei++;
                }

                bool cont = Options.ByPopularVote && yea > nei;

                if (!cont)
                    cont = nei > 0;

                if (cont)
                {
                    object returnInstance = invokeableMethod.Invoke(moduleInstance, methodInvokeArray);

                    await modules[moduleInstance.GetType()].OnCommandExecute(invokeableMethod, moduleInstance, methodInvokeArray, returnInstance);

                    foreach (BaseCommandAttribute attr in baseCommandAttributes)
                    {
                        await attr.AfterCommandExecute(moduleInstance, methodInvokeArray, returnInstance);
                    }
                }
            }
            else
            {
                Options.ToLog("Parameter length did not match the Invoking array that would have been supplied.", LogLevel.Error);
            }
        }

        /// <summary>
        /// Allows you to cast a string to a Type. Using 
        /// </summary>
        /// <typeparam name="T">Any type that can be handled by the <see cref="IConverter{T}"/> or simple conversions</typeparam>
        /// <param name="parse">Formatted string.</param>
        /// <param name="converted">The casted string</param>
        /// <returns>True if the cast was successful</returns>
        public bool CastString<T>(string parse, out T converted)
        {
            bool ret = ConvertString(parse, typeof(T), out object conversion, out _);

            converted = (T)conversion;
            return ret;
        }

        private bool ConvertString(string from, Type info, out object conversion, out string error)
        {
            if(UseConverter(info, from, out conversion))
            {
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
        /// <returns></returns>
        public bool CanConvert(Type c)
        {
            return helpers.Any(x => x.ConversionType == c);
        }

        /// <summary>
        /// Checks if a type can be converted from string, by checking registered conversion types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool CanConvert<T>()
        {
            return CanConvert(typeof(T));
        }

        /// <summary>
        /// Uses a converter in the registration
        /// </summary>
        public bool UseConverter(Type type, string parse, out object converted)
        {
            if (!CanConvert(type))
            {
                Options.ToLog($"Cannot convert {type.FullName}.", LogLevel.Error);

                converted = default;
                return false;
            }

            converted = helpers.FirstOrDefault(x => x.ConversionType == type).Convert(parse);
            return true;
        }

        /// <summary>
        /// Uses a converter in the registration
        /// </summary>
        public bool UseConverter<T>(string parse, out T converted)
        {
            bool ret = UseConverter(typeof(T), parse, out object con);

            converted = default;

            if (ret)
                converted = (T)con;

            return ret;
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
        if (CanConvert<T>())
        {
            Options.ToLog($"Handler for {typeof(T).FullName} already exist.", LogLevel.Error);
            return;
        }

        helpers.Add(ConverterHelper.Create(converter));
    }

    /// <summary>
    /// Register a lambda converter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="converter"></param>
    public void RegisterConverter<T>(Func<string, T> converter)
    {

        ConverterHelper helper = ConverterHelper.Create(converter);

        if (CanConvert<T>())
        {
            Options.ToLog($"Handler for {typeof(T).FullName} already exist.", LogLevel.Error);
            return;
        }

        helpers.Add(helper);
    }

    /// <summary>
    /// Gets rid of a converter of a specific type
    /// </summary>
    /// <param name="converter"></param>

    public void UnRegisterConverter(Type converter)
    {
        if (!CanConvert(converter))
        {
            Options.ToLog($"Handler does not exist for {converter.FullName}.", LogLevel.Warning);
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
    public string Name { get; internal set; }
    /// <summary>
    /// Amount of arguments to invoke the method info
    /// </summary>
    public int ParameterCount { get; internal set; }

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
        if (string.IsNullOrEmpty(left.Name) || string.IsNullOrEmpty(right.Name))
            return false;

        return left.Name.Equals(right.Name, StringComparison.OrdinalIgnoreCase) && left.ParameterCount == right.ParameterCount;
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
