namespace ProjectMakoto.Entities.Plugins.Commands;
public class BaseOverload
{
    /// <summary>
    /// Creates a new required overload for a command.
    /// </summary>
    /// <param name="Type">The type to use for the overload.</param>
    /// <param name="Name">The name for the overload to use.</param>
    /// <param name="Description">The description of the overload to use.</param>
    /// <param name="Required">If the overload should be required.</param>
    /// <param name="UseRemainingString">If the remaining string of the triggering message should be used as the last argument.</param>
    public BaseOverload(Type Type, string Name, string Description, bool Required = true, bool UseRemainingString = false)
    {
        this.Type = Type;
        this.Name = Name;
        this.Description = Description;
        this.Required = Required;
        this.UseRemainingString = UseRemainingString;
    }

    /// <summary>
    /// The type of overload.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// The name of the overload.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the overload.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// If the overload is required.
    /// </summary>
    public bool Required { get; set; }
    
    /// <summary>
    /// If the overload is required.
    /// </summary>
    public bool UseRemainingString { get; set; }
}
