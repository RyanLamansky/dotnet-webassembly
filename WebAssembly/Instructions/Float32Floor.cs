using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// Floor operator.
/// </summary>
public class Float32Floor : Float64CallWrapperInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Float32Floor"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float32Floor;

    /// <summary>
    /// Creates a new  <see cref="Float32Floor"/> instance.
    /// </summary>
    public Float32Floor()
    {
    }

    private protected sealed override MethodInfo MethodInfo => Float64Floor.Method;
}
