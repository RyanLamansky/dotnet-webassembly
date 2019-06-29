using System.Reflection;

namespace WebAssembly.Runtime.Compilation
{
    internal struct Indirect
    {
        public Indirect(uint type, MethodInfo function)
        {
            this.Type = type;
            this.Function = function;
        }

        public readonly uint Type;
        public readonly MethodInfo Function;
    }
}
