using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser;
/// <summary>
/// A handler to invoke commands.
/// </summary>
public sealed class CommandHandler
{
    internal readonly Dictionary<CommandInfo, CommandAttribute> _commands = new();
    internal readonly Dictionary<CommandInfo, object> _instances = new ();
    internal readonly Dictionary<CommandInfo, MethodInfo> _methods = new ();
    internal readonly Dictionary<Type, BaseCommandModule> _modules = new ();


    /// <summary>
    /// Commands being invoked.
    /// </summary>
    public IReadOnlyDictionary<CommandInfo, CommandAttribute> Commands => _commands;


    /// <summary>
    /// Types of registered modules.
    /// </summary>
    public IEnumerable<Type> Modules => _modules.Keys;


    /// <summary>
    /// Options for your Handler
    /// </summary>
    public HandlerConfig Options { get; init; }

    /// <summary>
    /// Used converter
    /// </summary>
    public StringConverter Converter { get; } = new StringConverter();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="config"></param>
    public CommandHandler(HandlerConfig config)
    {
        Options = config;

        Converter.RegisterConverter(delegate (string parse) { return ValueTask.FromResult(parse); }); //add in basic converters here

#pragma warning disable CS1998
        Converter.RegisterConverter(async delegate (string parse)
        {
            if (int.TryParse(parse, out int result))
                return  result;

            return 0;
        });

        Converter.RegisterConverter(async delegate (string parse)
        {
            if (double.TryParse(parse, out double result))
                return result;

            return 0;
        });

        Converter.RegisterConverter(async delegate (string parse)
        {
            if (float.TryParse(parse, out float result))
                return result;

            return 0;
        });
#pragma warning restore CS1998
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
        await Invoke(Array.Empty<object>(), invoker, Array.Empty<object>());
    }

    /// <summary>
    /// Invoke a command, must start with the prefix - name - arguments
    /// <para>Example: !name arg1 arg2</para>
    /// </summary>
    /// <param name="invoker"></param>
    /// <param name="aft">After arguments you want to add that are not string</param>
    /// <exception cref="Exceptions.InvalidConversionException"></exception>
    public async Task Invoke(string invoker, params object[] aft)
    {
        await Invoke(Array.Empty<object>(), invoker, aft);
    }

    /// <summary>
    /// Invoke a command, must start with the prefix - name - arguments
    /// <para>Example: !name arg1 arg2</para>
    /// </summary>
    /// <param name="invoker"></param>
    /// <param name="aft">After arguments you want to add that are not string</param>
    /// <param name="pre">Before arguments you want to add that are not string</param>
    /// <exception cref="Exceptions.InvalidConversionException"></exception>
    public async Task Invoke(object[] pre, string invoker, object[] aft)
    {
        if (Options.AlwaysTrim)
            invoker = invoker.Trim();

        string[] words = invoker.Split(Options.Separator);

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

        int argLen = pre.Length + stringArguments.Length + aft.Length;

        CommandInfo mockInfo = new (commandName, argLen);

        CommandInfo comparingResult = Commands.Keys.FirstOrDefault(x => x.Name.Equals(commandName, Options.Comp));

        if (comparingResult == default)
        {
            Options.ToLog($"'{commandName}' is not registered", LogLevel.Warning);
            return;
        }
        else
            mockInfo.Name = comparingResult.Name;

        List<object> methodInvoke = new (); //this list is responsible for the method invoking
        MethodInfo invokeableMethod = null;

        foreach (KeyValuePair<CommandInfo, MethodInfo> validMethods in _methods) //never use return in here!
        {
            MethodInfo method = validMethods.Value;

            ParameterInfo[] methodParameters = method.GetParameters();

            ParameterInfo[] skip = new ParameterInfo[(methodParameters.Length - pre.Length - aft.Length)];

            Array.Copy(methodParameters, pre.Length, skip, 0, skip.Length);


            for (int i = 0; i < skip.Length; i++)
            {
                ParameterInfo loopX = skip[i];

                foreach (CommandParameterAttribute pinvokes in loopX.GetCustomAttributes<CommandParameterAttribute>().OrderByDescending(x => x.importance))
                {
                    pinvokes.Handler = this;

                    //we should call this method in here because it can effect the total outcome of the command invokemennt
                    stringArguments = await pinvokes.OnCollect(loopX, stringArguments, skip);
                }
            }

            argLen = pre.Length + stringArguments.Length + aft.Length;

            if (argLen < methodParameters.Length)
            {
                Options.ToLog("Arguments do not match length", LogLevel.Warning);
                continue;
            }
            else if (argLen != methodParameters.Length) //catches a possible exception
            {
                Options.ToLog("Argument length does not match the Parameter Info Length!", LogLevel.Error);
                return;
            }

            for (int i = 0; i < stringArguments.Length; i++)
            {
                int strStart = i + pre.Length;

                bool converted = Converter.CastString(pre, stringArguments[i], aft, methodParameters[strStart].ParameterType, out ValueTask<object> convertedObject, out string possibleError);
                if (!converted)
                {
                    Options.ToLog(possibleError, LogLevel.Error);
                    methodInvoke.Clear(); //clear invoke list
                    continue;
                }
                else
                {
                    methodInvoke.Add(await convertedObject);
                }
            }

            invokeableMethod = method;
            mockInfo = new CommandInfo(mockInfo.Name, argLen);
        }

        if (invokeableMethod == null)
        {
            Options.ToLog("Could not find any commands to invoke", LogLevel.Warning);
            return;
        }

        IEnumerable<BaseCommandAttribute> baseCommandAttributes = invokeableMethod.GetCustomAttributes<BaseCommandAttribute>();
        object moduleInstance = _instances.GetValue(mockInfo);
        object[] methodInvokeArray = methodInvoke.ToArray();

        if (invokeableMethod.GetParameters().Length == argLen)
        {
            int yea = 0, nei = 0; //voting system

            foreach (BaseCommandAttribute attr in baseCommandAttributes)
            {
                attr.Handler = this;
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
                if (pre.Length > 0)
                    methodInvokeArray = pre.Concat(methodInvokeArray).ToArray();

                if (aft.Length > 0)
                    methodInvokeArray = methodInvokeArray.Concat(aft).ToArray();

                object returnInstance = invokeableMethod.Invoke(moduleInstance, methodInvokeArray);

                await _modules[moduleInstance.GetType()].OnCommandExecute(invokeableMethod, moduleInstance, methodInvokeArray, returnInstance);

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

        _modules.Add(reg, (BaseCommandModule)i);
    }




    /// <summary>
    /// Fully unregisters a module
    /// </summary>
    /// <param name="unreg"></param>
    public void UnRegisterModule(Type unreg)
    {
        if (!unreg.Inherits(typeof(BaseCommandModule)))
            return;
        else if (!_modules.ContainsKey(unreg))
            return;

        foreach (MethodInfo method in unreg.GetMethods((BindingFlags)(-1)))
        {
            CommandAttribute cmdAttr = method.GetCustomAttribute<CommandAttribute>();

            if (cmdAttr == null)
                continue;
            else if (method.GetCustomAttribute<IgnoreAttribute>() != null)
                continue;

            CommandInfo inf = new (cmdAttr.CommandName, method.GetParameters().Length);

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

            _modules.Remove(unreg);
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
