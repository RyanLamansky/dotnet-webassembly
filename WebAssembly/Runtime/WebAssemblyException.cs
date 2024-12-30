using System;

namespace WebAssembly.Runtime;

/// <summary>
/// Describes an error that was thrown from the WASM runtime.
/// </summary>
public class WebAssemblyException : Exception
{
    /// <summary>
    /// Creates a new <see cref="WebAssemblyException"/> instance with the provided message.
    /// </summary>
    /// <param name="tagIndex">Tag index that was thrown.</param>
    public WebAssemblyException(uint tagIndex)
        : base($"WebAssembly runtime threw an exception with tag index {tagIndex}.")
    {
        TagIndex = tagIndex;
    }

    /// <summary>
    /// Creates a new <see cref="WebAssemblyException"/> instance with the provided message.
    /// </summary>
    /// <param name="tagIndex">Tag index that was thrown.</param>
    /// <param name="message">The message that describes the error.</param>
    protected WebAssemblyException(uint tagIndex, string message)
        : base(message)
    {
        TagIndex = tagIndex;
    }

    /// <summary>
    /// Index of the tag that was thrown.
    /// </summary>
    public uint TagIndex { get; }

    /// <summary>
    /// Amount of arguments that were passed to the exception.
    /// </summary>
    protected virtual int ArgCount => 0;

    /// <summary>
    /// Gets the argument at the specified index.
    /// </summary>
    /// <param name="index">The index of the argument to get.</param>
    /// <returns>The argument at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="ArgCount"/>.</exception>
    protected virtual object? GetArg(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>
    /// Gets the arguments as an array.
    /// </summary>
    public object?[] GetArgs()
    {
        var result = new object?[ArgCount];
        for (var i = 0; i < result.Length; i++)
            result[i] = GetArg(i);
        return result;
    }
}


/// <summary>
/// Describes an error that was thrown from the WASM runtime.
/// </summary>
public class WebAssemblyException<T0> : WebAssemblyException
{
    /// <summary>
    /// Creates a new <see cref="WebAssemblyException"/> instance with the provided message.
    /// </summary>
    /// <param name="param0">The first parameter.</param>
    /// <param name="tagIndex">Tag index that was thrown.</param>
    public WebAssemblyException(T0 param0, uint tagIndex)
        : base(tagIndex, $"WebAssembly runtime threw an exception with tag index {tagIndex} and parameter {param0}.")
    {
        Param0 = param0;
    }

    /// <summary>
    /// First parameter.
    /// </summary>
    public T0 Param0 { get; set; }

    /// <inheritdoc />
    protected override int ArgCount => 1;

    /// <inheritdoc />
    protected override object? GetArg(int index)
    {
        return index switch
        {
            0 => Param0,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }
}

/// <summary>
/// Describes an error that was thrown from the WASM runtime.
/// </summary>
public class WebAssemblyException<T0, T1> : WebAssemblyException
{
    /// <summary>
    /// Creates a new <see cref="WebAssemblyException"/> instance with the provided message.
    /// </summary>
    /// <param name="param1">The second parameter.</param>
    /// <param name="param0">The first parameter.</param>
    /// <param name="tagIndex">Tag index that was thrown.</param>
    public WebAssemblyException(T1 param1, T0 param0, uint tagIndex)
        : base(tagIndex, $"WebAssembly runtime threw an exception with tag index {tagIndex} and parameters {param0} and {param1}.")
    {
        Param0 = param0;
        Param1 = param1;
    }

    /// <summary>
    /// First parameter of the exception.
    /// </summary>
    public T0 Param0 { get; set; }

    /// <summary>
    /// Second parameter of the exception.
    /// </summary>
    public T1 Param1 { get; set; }

    /// <inheritdoc />
    protected override int ArgCount => 2;

    /// <inheritdoc />
    protected override object? GetArg(int index)
    {
        return index switch
        {
            0 => Param0,
            1 => Param1,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }
}
