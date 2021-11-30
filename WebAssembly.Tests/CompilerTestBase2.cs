namespace WebAssembly;

/// <summary>
/// Many compiler tests can use this template to host the test.
/// </summary>
public abstract class CompilerTestBase2<T>
    where T : struct
{
    /// <summary>
    /// Creates a new <see cref="CompilerTestBase2{T}"/> instance.
    /// </summary>
    protected CompilerTestBase2()
    {
    }

    /// <summary>
    /// Returns a value
    /// </summary>
    /// <param name="parameter0">Input to the test function.</param>
    /// <param name="parameter1">Input to the test function.</param>
    /// <returns>A value to ensure proper control flow and execution.</returns>
    public abstract T Test(T parameter0, T parameter1);

    /// <summary>
    /// Provides a <see cref="CompilerTestBase2{T}"/> for the provided instructions.
    /// </summary>
    /// <param name="instructions">The instructions that form the body of the <see cref="Test(T, T)"/> function.</param>
    /// <returns>The <see cref="CompilerTestBase2{T}"/> instance.</returns>
    public static CompilerTestBase2<T> CreateInstance(params Instruction[] instructions)
    {
        var type = AssemblyBuilder.Map(typeof(T));

        return AssemblyBuilder.CreateInstance<CompilerTestBase2<T>>(nameof(CompilerTestBase2<T>.Test),
            type,
            new[]
            {
                    type,
                    type,
            },
            instructions);
    }
}
