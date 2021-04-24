using System;
using WebAssembly; // Acquire from https://www.nuget.org/packages/WebAssembly
using WebAssembly.Instructions;
using WebAssembly.Runtime;

// We need this later to call the code we're generating.
public abstract class Sample
{
    // Sometimes you can use C# dynamic instead of building an abstract class like this.
    public abstract int Demo(int value);
}

static class Program
{
    static void Main()
    {
        // Module can be used to create, read, modify, and write WebAssembly files.
        var module = new Module(); // In this case, we're creating a new one.

        // Types are function signatures: the list of parameters and returns.
        module.Types.Add(new WebAssemblyType // The first added type gets index 0.
        {
            Parameters = new[]
            {
                WebAssemblyValueType.Int32, // This sample takes a single Int32 as input.
                // Complex types can be passed by sending them in pieces.
            },
            Returns = new[]
            {
                // Multiple returns are supported by the binary format.
                // Standard currently allows a count of 0 or 1, though.
                WebAssemblyValueType.Int32,
            },
        });
        // Types can be re-used for multiple functions to reduce WASM size.

        // The function list associates a function index to a type index.
        module.Functions.Add(new Function // The first added function gets index 0.
        {
            Type = 0, // The index for the "type" value added above.
        });

        // Code must be passed in the exact same order as the Functions above.
        module.Codes.Add(new FunctionBody
        {
            Code = new Instruction[]
            {
                new LocalGet(0), // The parameters are the first locals, in order.
                // We defined the first parameter as Int32, so now an Int32 is at the top of the stack.
                new Int32CountOneBits(), // Returns the count of binary bits set to 1.
                // It takes the Int32 from the top of the stack, and pushes the return value.
                // So, in the end, there is still a single Int32 on the stack.
                new End(), // All functions must end with "End".
                // The final "End" also delivers the returned value.
            },
        });

        // Exports enable features to be accessed by external code.
        // Typically this means JavaScript, but this library adds .NET execution capability, too.
        module.Exports.Add(new Export
        {
            Kind = ExternalKind.Function,
            Index = 0, // This should match the function index from above.
            Name = "Demo", // Anything legal in Unicode is legal in an export name.
        });

        // We now have enough for a usable WASM file, which we could save with module.WriteToBinary().
        // Below, we show how the Compile feature can be used for .NET-based execution.
        // For stream-based compilation, WebAssembly.Compile should be used.
        var instanceCreator = module.Compile<Sample>();

        // Instances should be wrapped in a "using" block for automatic disposal.
        // This sample doesn't import anything, so we pass an empty import dictionary.
        using (var instance = instanceCreator(new ImportDictionary()))
        {
            // FYI, instanceCreator can be used multiple times to create independent instances.
            Console.WriteLine(instance.Exports.Demo(0)); // Binary 0, result 0
            Console.WriteLine(instance.Exports.Demo(1)); // Binary 1, result 1,
            Console.WriteLine(instance.Exports.Demo(42));  // Binary 101010, result 3
        } // Automatically release the WebAssembly instance here.
    }
}