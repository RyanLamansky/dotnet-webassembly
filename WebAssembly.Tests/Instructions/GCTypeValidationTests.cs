using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Validation tests for the WebAssembly 3.0 GC struct/array type guards in
/// <see cref="WebAssembly.Runtime.Compilation.GCTypeEmitter"/>: applying a struct instruction to an array type (or the
/// reverse), or naming an out-of-range field, must surface a clean <see cref="ModuleLoadException"/> rather than an
/// internal fault.
/// </summary>
[TestClass]
public class GCTypeValidationTests
{
    // A module whose type 0 is a struct { i32 }, type 1 is an array of i32, and type 2 is the () -> () signature of
    // the single (exported) function, whose body is the supplied instructions.
    private static Module StructAndArrayModule(params Instruction[] body)
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }] }); // 0: struct
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Array, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }] }); // 1: array
        module.Types.Add(new WebAssemblyType()); // 2: () -> ()
        module.Functions.Add(new Function(2));
        module.Exports.Add(new Export("Test", 0));
        module.Codes.Add(new FunctionBody(body));
        return module;
    }

    /// <summary><c>struct.get</c> on an array-typed index is rejected (GCTypeEmitter.RequireStruct).</summary>
    [TestMethod]
    public void StructGet_OnArrayType_ThrowsModuleLoad()
    {
        var module = StructAndArrayModule(new StructGet { Type = 1, Field = 0 }, new End());
        var x = Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<object>());
        StringAssert.Contains(x.Message, "struct type");
    }

    /// <summary><c>array.get</c> on a struct-typed index is rejected (GCTypeEmitter.RequireArray).</summary>
    [TestMethod]
    public void ArrayGet_OnStructType_ThrowsModuleLoad()
    {
        var module = StructAndArrayModule(new ArrayGet { Type = 0 }, new End());
        var x = Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<object>());
        StringAssert.Contains(x.Message, "array type");
    }

    /// <summary><c>struct.get</c> of an out-of-range field is rejected (GCTypeEmitter.ResolveStructField).</summary>
    [TestMethod]
    public void StructGet_FieldOutOfRange_ThrowsModuleLoad()
    {
        var module = StructAndArrayModule(new StructGet { Type = 0, Field = 5 }, new End());
        var x = Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<object>());
        StringAssert.Contains(x.Message, "unknown field");
    }
}
