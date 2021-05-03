// This sample isn't complete but may be useful as a starting point for handling exports.
// The WebAssembly.Tests project has cases for all export types, so you can track down how to use them there until this example is finished.
using System;
using System.Linq;
using WebAssembly; // Acquire from https://www.nuget.org/packages/WebAssembly

// Create a WASM with a variety of imports/exports for illustrative purposes.
// When working with an existing WASM, you'd use `Module.ReadFromBinary`.
var module = Sample.GenerateExample();

const string indent = "    ";
Console.WriteLine("abstract class Exports");
Console.WriteLine("{");
Console.WriteLine($"{indent}protected Exports()");
Console.WriteLine($"{indent}{{");
Console.WriteLine($"{indent}}}");
Console.WriteLine();

foreach (var export in module.Exports)
{
    static string WebAssemblyValueTypeToNative(WebAssemblyValueType type) => type switch
    {
        WebAssemblyValueType.Int32 => "int",
        WebAssemblyValueType.Int64 => "long",
        WebAssemblyValueType.Float32 => "float",
        WebAssemblyValueType.Float64 => "double",
        _ => throw new NotSupportedException(),
    };

    switch (export.Kind)
    {
        case ExternalKind.Function:
            // TODO: Account for the indexes of imported functions--first internal function's index is 1 after the last imported.
            var signature = module.Types[(int)module.Functions[(int)export.Index].Type];
            Console.Write(indent);
            Console.Write("public abstract ");
            Console.Write(signature.Returns.Count == 0 ? "void" : WebAssemblyValueTypeToNative(signature.Returns[0]));
            Console.Write(' ');
            Console.Write(export.Name);
            Console.Write('(');
            Console.Write(string.Join(", ", signature.Parameters.Select((type, index) => $"{WebAssemblyValueTypeToNative(type)} p{index}")));
            Console.WriteLine(");");
            continue;

        case ExternalKind.Table:
            // TODO
            continue;

        case ExternalKind.Memory:
            // TODO
            continue;

        case ExternalKind.Global:
            // TODO: Account for the indexes of imported globals--first internal global's index is 1 after the last imported.
            var global = module.Globals[(int)export.Index];
            Console.Write(indent);
            Console.Write("public abstract ");
            Console.Write(WebAssemblyValueTypeToNative(global.ContentType));
            Console.Write(' ');
            Console.Write(export.Name);
            Console.Write(" { get;");
            if (global.IsMutable)
                Console.Write(" set;");
            Console.WriteLine(" }");
            continue;
    }
}

Console.WriteLine("}");

// After generating the code, it can be used as the type parameter for `Compile.FromBinary`.
// The compiler will inherit and implement the class.
