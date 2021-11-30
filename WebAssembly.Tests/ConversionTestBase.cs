namespace WebAssembly;

/// <summary>
/// Tests of conversion instructions can use this template to host the test.
/// </summary>
public abstract class ConversionTestBase<TInput, TReturn>
    where TInput : struct
    where TReturn : struct
{
    /// <summary>
    /// Creates a new <see cref="ConversionTestBase{TInput, TReturn}"/> instance.
    /// </summary>
    protected ConversionTestBase()
    {
    }

    /// <summary>
    /// Returns a value.
    /// </summary>
    /// <param name="parameter">Input to the test function.</param>
    /// <returns>A value to ensure proper control flow and execution.</returns>
    public abstract TReturn Test(TInput parameter);

    /// <summary>
    /// Provides a <see cref="ConversionTestBase{TInput, TReturn}"/> for the provided instructions.
    /// </summary>
    /// <param name="instructions">The instructions that form the body of the <see cref="Test(TInput)"/> function.</param>
    /// <returns>The <see cref="ConversionTestBase{TInput, TReturn}"/> instance.</returns>
    public static ConversionTestBase<TInput, TReturn> CreateInstance(params Instruction[] instructions)
    {
        var input = AssemblyBuilder.Map(typeof(TInput));
        var @return = AssemblyBuilder.Map(typeof(TReturn));

        return AssemblyBuilder.CreateInstance<ConversionTestBase<TInput, TReturn>>(nameof(ConversionTestBase<TInput, TReturn>.Test),
            @return,
            new[]
            {
                    input,
            },
            instructions);
    }
}
