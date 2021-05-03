using WebAssembly; // Acquire from https://www.nuget.org/packages/WebAssembly
using WebAssembly.Instructions;

static class Sample
{
    public static Module GenerateExample() => new()
    {
        Types = new WebAssemblyType[]
        {
            new WebAssemblyType
            {
                Returns = new WebAssemblyValueType[] { WebAssemblyValueType.Int32 },
                Parameters = new WebAssemblyValueType[]
                {
                    WebAssemblyValueType.Int32,
                    WebAssemblyValueType.Int64,
                    WebAssemblyValueType.Float32,
                    WebAssemblyValueType.Float64,
                },
            },
            new WebAssemblyType
            {
            },
        },
        Functions = new Function[]
        {
            new Function(0),
            new Function(1),
        },
        Codes = new FunctionBody[]
        {
            new FunctionBody(new LocalGet(), new End()),
            new FunctionBody(new End()),
        },
        Globals = new Global[]
        {
            new Global
            {
                ContentType = WebAssemblyValueType.Int32,
            },
            new Global
            {
                ContentType = WebAssemblyValueType.Int32,
                IsMutable = true,
            }
        },
        Exports = new Export[]
        {
            new Export("Variety", 0, ExternalKind.Function),
            new Export("DoNothing", 1, ExternalKind.Function),
            new Export("ReadOnlyInt32Global", 0, ExternalKind.Global),
            new Export("ReadWriteInt32Global", 1, ExternalKind.Global),
        },
    };
}
