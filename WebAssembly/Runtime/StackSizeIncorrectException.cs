namespace WebAssembly.Runtime;

/// <summary>
/// Used by the compiler to indicate the stack was the wrong size to execute an operation.
/// </summary>
/// <param name="opCode">The operation attempted.</param>
/// <param name="expected">The expected stack height.</param>
/// <param name="actual">The actual stack height at the time the operation was attempted.</param>
public class StackSizeIncorrectException(OpCode opCode, int expected, int actual) : OpCodeCompilationException(opCode, $"requires at {expected} value{(expected != 1 ? "s" : "")} on the stack, found {actual}.")
{

    /// <summary>
    /// The expected stack height.
    /// </summary>
    public int Expected { get; } = expected;

    /// <summary>
    /// The actual stack height at the time the operation was attempted.
    /// </summary>
    public int Actual { get; } = actual;
}
