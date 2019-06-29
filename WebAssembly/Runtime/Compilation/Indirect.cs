using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation
{
    internal struct Indirect
    {
        public Indirect(uint type, MethodBuilder function)
        {
            this.Type = type;
            this.Function = function;
        }

        public readonly uint Type;
        public readonly MethodBuilder Function;
    }
}
