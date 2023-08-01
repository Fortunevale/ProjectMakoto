// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Runtime.Serialization;

namespace ProjectMakoto.Exceptions;
internal class InvalidCallException : Exception
{
    public InvalidCallException()
    {
    }

    public InvalidCallException(string? message) : base(message)
    {
    }

    public InvalidCallException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidCallException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
