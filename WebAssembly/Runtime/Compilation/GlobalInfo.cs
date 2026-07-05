using System.Reflection;

namespace WebAssembly.Runtime.Compilation;

internal sealed class GlobalInfo(ValType type, bool requiresInstance, MethodInfo getter, MethodInfo? setter)
{
    public readonly ValType Type = type;
    public readonly bool RequiresInstance = requiresInstance;
    public readonly MethodInfo Getter = getter;
    public readonly MethodInfo? Setter = setter;

#if DEBUG
    public sealed override string ToString() => $"{this.Type} {this.RequiresInstance}";
#endif
}
