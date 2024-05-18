using System.Reflection;

namespace WebAssembly.Runtime.Compilation;

internal sealed class GlobalInfo(WebAssemblyValueType type, bool requiresInstance, MethodInfo getter, MethodInfo? setter)
{
    public readonly WebAssemblyValueType Type = type;
    public readonly bool RequiresInstance = requiresInstance;
    public readonly MethodInfo Getter = getter;
    public readonly MethodInfo? Setter = setter;

#if DEBUG
    public sealed override string ToString() => $"{this.Type} {this.RequiresInstance}";
#endif
}
