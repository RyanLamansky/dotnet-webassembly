using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// Indicates that something provided as an import doesn't match the type expected by the WASM.
    /// </summary>
    public class ImportException : RuntimeException
    {
        /// <summary>
        /// Creates a new <see cref="ImportException"/> with a default message.
        /// </summary>
        public ImportException()
            : base("Import type did not match the expected type.")
        {
        }

        /// <summary>
        /// Creates a new <see cref="ImportException"/> with a default message.
        /// </summary>
        public ImportException(string message)
            : base(message)
        {
        }

        internal static void EmitTryCast(ILGenerator il, Type target)
        {
            il.Emit(OpCodes.Isinst, target);
            il.Emit(OpCodes.Dup);

            var typeCheckPassed = il.DefineLabel();
            il.Emit(OpCodes.Brtrue, typeCheckPassed);
            
            il.Emit(OpCodes.Newobj, typeof(ImportException).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 0));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(typeCheckPassed);
        }
    }
}
