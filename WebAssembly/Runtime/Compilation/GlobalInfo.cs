using System.Reflection;

namespace WebAssembly.Runtime.Compilation
{
    internal sealed class GlobalInfo
    {
        public readonly ValueType Type;
        public readonly bool RequiresInstance;
        public readonly MethodInfo Getter;
        public readonly MethodInfo Setter;

        public GlobalInfo(ValueType type, bool requiresInstance, MethodInfo getter, MethodInfo setter)
        {
            this.Type = type;
            this.RequiresInstance = requiresInstance;
            this.Getter = getter;
            this.Setter = setter;
        }

#if DEBUG
        public sealed override string ToString() => $"{this.Type} {this.RequiresInstance}";
#endif
    }
}
