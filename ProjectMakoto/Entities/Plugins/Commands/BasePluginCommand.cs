namespace ProjectMakoto.Entities.Plugins.Commands;

public class BasePluginCommand
{
    /// <summary>
    /// Create a new Plugin Command.
    /// </summary>
    /// <param name="Name">The name of the command to be registered.</param>
    /// <param name="Description">The description of the command to be registered.</param>
    /// <param name="Command">The command to be executed.</param>
    /// <param name="Module">The module of the command to be registered.</param>
    /// <param name="Overloads">The required overloads of the command to be registered.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required argument is <see langword="null"/> or consists only of whitespaces.</exception>
    public BasePluginCommand(string Name, string Description, string Module, BaseCommand Command, IEnumerable<BaseOverload> Overloads = null)
    {
        if (Name.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Name));

        if (Description.IsNullOrWhiteSpace()) 
            throw new ArgumentNullException(nameof(Description));

        if (Command is null)
            throw new ArgumentNullException(nameof(Command));

        this.Name = Name.Trim();
        this.Description = Description.Trim();
        this.Module = Module.Trim();
        this.Command = Command;
        this.Overloads = Overloads?.ToArray() ?? Array.Empty<BaseOverload>();
    }

    /// <summary>
    /// The command's name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The command's description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The command's module.
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// The command.
    /// </summary>
    public BaseCommand Command { get; set; }

    /// <summary>
    /// The required overloads.
    /// </summary>
    public BaseOverload[] Overloads { get; set; }
}
