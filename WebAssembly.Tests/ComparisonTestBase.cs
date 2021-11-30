namespace WebAssembly;

/// <summary>
/// Many comparison tests can use this template to host the test.
/// </summary>
public abstract class ComparisonTestBase<T>
    where T : struct
{
    /// <summary>
    /// Creates a new <see cref="CompilerTestBase{T}"/> instance.
    /// </summary>
    protected ComparisonTestBase()
    {
    }

    /// <summary>
    /// Returns a value.
    /// </summary>
    /// <param name="parameter0">Input to the test function.</param>
    /// <param name="parameter1">Input to the test function.</param>
    /// <returns>A value to ensure proper control flow and execution.</returns>
    public abstract int Test(T parameter0, T parameter1);

    /// <summary>
    /// Provides a <see cref="ComparisonTestBase{T}"/> for the provided instructions.
    /// </summary>
    /// <param name="instructions">The instructions that form the body of the <see cref="Test(T, T)"/> function.</param>
    /// <returns>The <see cref="ComparisonTestBase{T}"/> instance.</returns>
    public static ComparisonTestBase<T> CreateInstance(params Instruction[] instructions)
    {
        var type = AssemblyBuilder.Map(typeof(T));

        return AssemblyBuilder.CreateInstance<ComparisonTestBase<T>>(nameof(ComparisonTestBase<T>.Test),
            WebAssemblyValueType.Int32,
            new[]
            {
                    type,
                    type,
            },
            instructions);
    }
}
