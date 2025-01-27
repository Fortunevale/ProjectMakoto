// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;
/// <summary>
/// Creates a new required overload for a command.
/// </summary>
/// <param name="Type">The type to use for the overload.</param>
/// <param name="Name">The name for the overload to use.</param>
/// <param name="Description">The description of the overload to use.</param>
/// <param name="Required">If the overload should be required.</param>
/// <param name="UseRemainingString">If the remaining string of the triggering message should be used as the last argument.</param>
public sealed class MakotoCommandOverload(Type Type, string Name, string Description, bool Required = true, bool UseRemainingString = false)
{

    /// <summary>
    /// The type of overload.
    /// </summary>
    public Type Type { get; init; } = Type;

    /// <summary>
    /// The name of the overload.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// The description of the overload.
    /// </summary>
    public string Description { get; init; } = Description;

    /// <summary>
    /// If the overload is required.
    /// </summary>
    public bool Required { get; init; } = Required;

    /// <summary>
    /// If the overload is required.
    /// </summary>
    public bool UseRemainingString { get; init; } = UseRemainingString;

    /// <summary>
    /// The type used for auto complete, null if no auto complete defined.
    /// </summary>
    public Type? AutoCompleteType { get; internal set; } = null;

    /// <summary>
    /// The minimum value of an int.
    /// </summary>
    public long? MinimumValue { get; internal set; } = null;

    /// <summary>
    /// The maximum value of an int.
    /// </summary>
    public long? MaximumValue { get; internal set; } = null;

    /// <summary>
    /// The type of channels to limit this overload to.
    /// </summary>
    public ChannelType? ChannelType { get; internal set; } = null;

    /// <summary>
    /// Sets the auto complete provider for this overload.
    /// </summary>
    /// <param name="autocompleteType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public MakotoCommandOverload WithAutoComplete(Type autocompleteType)
    {
        if (autocompleteType.IsAssignableFrom(typeof(IAutocompleteProvider)))
            throw new ArgumentException($"The provided type does not inherit {nameof(IAutocompleteProvider)}!", nameof(autocompleteType));

        this.AutoCompleteType = autocompleteType;
        return this;
    }

    /// <summary>
    /// Sets the channel type and returns this overload.
    /// </summary>
    /// <param name="channelType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public MakotoCommandOverload WithChannelType(ChannelType channelType)
    {
        if (this.Type != typeof(DiscordChannel))
            throw new ArgumentException($"Type has to be a {nameof(DiscordChannel)}!", nameof(channelType));

        this.ChannelType = channelType;
        return this;
    }

    /// <summary>
    /// Sets the minimum value and returns this overload.
    /// </summary>
    /// <param name="newValue"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public MakotoCommandOverload WithMinimumValue(long newValue)
    {
        if (this.Type != typeof(int) && this.Type != typeof(long) && this.Type != typeof(double))
            throw new ArgumentException("Type has to be an int, long or double!", nameof(newValue));

        this.MinimumValue = newValue;
        return this;
    }

    /// <summary>
    /// Sets the minimum value and returns this overload.
    /// </summary>
    /// <param name="newValue"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public MakotoCommandOverload WithMaximumValue(long newValue)
    {
        if (this.Type != typeof(int) && this.Type != typeof(long) && this.Type != typeof(double))
            throw new ArgumentException("Type has to be an int, long or double!", nameof(newValue));

        this.MaximumValue = newValue;
        return this;
    }
}
