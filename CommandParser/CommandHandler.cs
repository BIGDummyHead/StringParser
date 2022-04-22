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
    internal readonly Dictionary<CommandInfo, Info> _commands = new();
    internal readonly Dictionary<MethodInfo, CommandInfo> _mc = new();
    internal readonly Dictionary<Type, BaseCommandModule> _modules = new();

    internal struct Info
    {
        public readonly CommandAttribute cmdAttr;
        public readonly object instance;
        public readonly MethodInfo method;

        public readonly bool isIgnored;
        public readonly ParameterInfo[] parameters;

        public readonly IReadOnlyDictionary<ParameterInfo, CommandParameterAttribute> parameterAttributes;

        //ctor
        public Info(CommandAttribute cmdAttr, object instance, MethodInfo method)
        {
            this.cmdAttr = cmdAttr;
            this.instance = instance;
            this.method = method;
            isIgnored = method.GetCustomAttribute<IgnoreAttribute>() != null;
            parameters = method.GetParameters();

            List<KeyValuePair<ParameterInfo, CommandParameterAttribute>> ls = new();
            foreach (ParameterInfo pi in parameters)
            {
                CommandParameterAttribute cpa = pi.GetCustomAttribute<CommandParameterAttribute>();

                if (cpa == null)
                    continue;

                ls.Add(new(pi, cpa));
            }

            parameterAttributes = new Dictionary<ParameterInfo, CommandParameterAttribute>(ls);
            ls.GetEnumerator().Dispose();
        }
    }


    /// <summary>
    /// Commands being invoked.
    /// </summary>
    public IReadOnlyDictionary<CommandInfo, CommandAttribute> Commands => _commands.ToDictionary(x => x.Key, x => x.Value.cmdAttr);


    /// <summary>
    /// Types of registered modules.
    /// </summary>
    public IEnumerable<Type> Modules => _modules.Keys;


    /// <summary>
    /// Options for your Handler
    /// </summary>
    public HandlerConfig Config { get; init; }

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
        Config = config;

        Converter.RegisterConverter(delegate (string parse) { return ValueTask.FromResult(parse); }); //add in basic converters here

#pragma warning disable CS1998
        Converter.RegisterConverter(async delegate (string parse)
        {
            if (int.TryParse(parse, out int result))
                return result;

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
    /// <returns>The result of the method if any.</returns>
    /// <exception cref="Exceptions.InvalidConversionException"></exception>
    public async Task<object?> Invoke(string invoker)
    {
        return await Invoke(Array.Empty<object>(), invoker, Array.Empty<object>());
    }

    /// <summary>
    /// Invoke a command, must start with the prefix - name - arguments
    /// <para>Example: !name arg1 arg2</para>
    /// </summary>
    /// <param name="invoker"></param>
    /// <param name="aft">Non-string object arguments that are supplied after the string args.</param>
    /// <returns>The result of the method if any.</returns>
    /// <exception cref="Exceptions.InvalidConversionException"></exception>
    public async Task<object?> Invoke(string invoker, params object[] aft)
    {
        return await Invoke(Array.Empty<object>(), invoker, aft);
    }

    /// <summary>
    /// Invoke a command, must start with the prefix - name - arguments
    /// <para>Example: !name arg1 arg2</para>
    /// </summary>
    /// <param name="pre">Non-string object arguments that are supplied before the string args.</param>
    /// <param name="invoker">Invoking string arguments</param>
    /// <param name="aft">Non-string object arguments that are supplied after the string args.</param>
    /// <returns>The result of the method if any.</returns>
    /// <exception cref="Exceptions.InvalidConversionException"></exception>
    public async Task<object?> Invoke(object[] pre, string invoker, object[] aft)
    {
        string prefix = Config.HasPrefix ? invoker[0..(Config.Prefix.Length)] : string.Empty;

        if (Config.HasPrefix)
        {
            if (!prefix.Equals(Config.Prefix, Config.Comp))
            {
                Config.ToLog($"Prefix invalid! Expected '{Config.Prefix}'", LogLevel.Information);
                return null;
            }
        }

        string[] stringArgs = invoker.Split(Config.Separator);

        string commandName = stringArgs[0][prefix.Length..];

        stringArgs = stringArgs[1..];

        MethodInfo method = null;

        var filteredCommands = _commands.Where(x => x.Key.Name.Equals(commandName, Config.Comp));

        if (!filteredCommands.Any())
        {
            string b = $"There is no command with the name of '{commandName}'";

            foreach (var item in _commands)
            {
                b += $"\r\n* {item.Key.Name}";
            }

            Config.ToLog(b, LogLevel.Warning);
            return null;
        }

        foreach (KeyValuePair<CommandInfo, Info> selCommand in filteredCommands)
        {
            Info command = selCommand.Value;

            foreach (ParameterInfo pi in command.parameters)
            {
                if (!command.parameterAttributes.TryGetValue(pi, out CommandParameterAttribute cpa))
                    continue;

                cpa.Handler = this;
                stringArgs = await cpa.OnCollect(pi, pre, stringArgs, aft, command.parameters);
            }

            if (command.parameters.Length == stringArgs.Length)
            {
                method = command.method;
                break;
            }
        }

        int stringArgCount = stringArgs.Length;

        int totalCount = pre.Length + stringArgCount + aft.Length;

        if (method == null)
        {
            Config.ToLog($"No command with the name of '{commandName}' has '{totalCount}' arguments.", LogLevel.Warning);
            return null;
        }

        List<object> lso = new();

        lso.AddRange(pre);

        for (int i = 0; i < stringArgCount; i++)
        {
            int at = pre.Length + i;
            string arg = stringArgs[i];

            Type type = method.GetParameters()[at].ParameterType;

            if (!Converter.CastString(pre, arg, aft, type, out ValueTask<object> converted, out string error))
            {
                Config.ToLog(error, LogLevel.Error);
                return null;
            }

            lso.Add(await converted);
        }

        lso.AddRange(aft);

        Info finalInfoCommand = _commands[_mc[method]];

        IEnumerable<BaseCommandAttribute> bcas = method.GetCustomAttributes<BaseCommandAttribute>();

        object[] invokeArr = lso.ToArray();

        int yay = 0;
        int nay = 0;

        foreach (BaseCommandAttribute bca in bcas)
        {
            bca.Handler = this;
            if (await bca.BeforeCommandExecute(finalInfoCommand.instance, invokeArr))
                yay++;
            else
                nay++;
        }

        if (yay >= nay && Config.ByPopularVote || !Config.ByPopularVote)
        {
            object? result = method.Invoke(finalInfoCommand.instance, invokeArr);

            foreach (BaseCommandAttribute bca in bcas)
                await bca.AfterCommandExecute(finalInfoCommand.instance, invokeArr, result);

            return result;
        }

        Config.ToLog($"Popular vote decided not to invoke '{commandName}' for method : '{method.Name}'", LogLevel.Information);
        return null;
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
        object? i = Activator.CreateInstance(reg);

        if (i is null)
            throw new Exceptions.InvalidModuleException(reg, "could not be created because an empty CTOR does not exist.");

        foreach (MethodInfo method in typeMethods)
        {
            CommandAttribute cmd = method.GetCustomAttribute<CommandAttribute>();
            IgnoreAttribute ig = method.GetCustomAttribute<IgnoreAttribute>(); //check if we should ignore adding in this command

            if (cmd is not null && ig is null)
            {
                cmd.OnRegister(reg, method);
                AddCommand(cmd, i, method);
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

            cmdAttr.OnUnRegister(unreg, method);

            CommandInfo inf = new(cmdAttr.CommandName, method.GetParameters().Length);

            foreach (KeyValuePair<CommandInfo, Info> item in _commands)
            {
                if (item.Key == inf)
                {
                    inf = item.Key;
                    break;
                }
            }

            _commands.Remove(inf);
            _mc.Remove(method);
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

        Info addingInfo = new(cmd, instance, info);

        _commands.Add(commandInfo, addingInfo);
        _mc.Add(info, commandInfo);
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
