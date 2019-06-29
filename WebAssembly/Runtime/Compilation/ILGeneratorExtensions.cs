using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation
{
    static class ILGeneratorExtensions
    {
        public static void EmitLoadArg(this ILGenerator il, int arg) => il.EmitLoadArg(checked((ushort)arg));

        public static void EmitLoadArg(this ILGenerator il, ushort arg)
        {
            System.Reflection.Emit.OpCode opCode;
            switch (arg)
            {
                case 0: opCode = OpCodes.Ldarg_0; break;
                case 1: opCode = OpCodes.Ldarg_1; break;
                case 2: opCode = OpCodes.Ldarg_2; break;
                case 3: opCode = OpCodes.Ldarg_3; break;
                default:
                    if (arg <= byte.MaxValue)
                        il.Emit(OpCodes.Ldarg_S, (byte)arg);
                    else
                        il.Emit(OpCodes.Ldarg, arg);
                    return;
            }
            il.Emit(opCode);
        }
    }
}
