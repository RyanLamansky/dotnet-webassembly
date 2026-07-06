using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Definite-assignment validation for non-defaultable (non-nullable reference) locals.
/// Each test builds a function with a non-nullable <c>(ref func)</c> parameter (always initialized) and one
/// non-nullable <c>(ref func)</c> local (initially uninitialized), then compiles it to exercise one of the
/// seven <c>local_init</c> fixture shapes. Block/if results are kept empty so nothing here depends on
/// inline-reference block-result support.
/// </summary>
[TestClass]
public class LocalInitTests
{
    private static readonly ReferenceType NonNullFuncRef = new(WebAssemblyHeapType.Func, isNullable: false);

    // Compiles a single non-exported function of type ((ref func)) -> () with one extra non-nullable (ref func)
    // local at index 1. Compilation runs the validator, so an uninitialized-local violation surfaces here.
    private static void Compile(params Instruction[] code)
    {
        var module = new Module();
        var type = new WebAssemblyType();
        type.AddParameter(NonNullFuncRef); // Parameter 0: always initialized.
        module.Types.Add(type);
        module.Functions.Add(new Function(0));
        var body = new FunctionBody { Code = code };
        body.Locals.Add(new Local { Count = 1, RefType = NonNullFuncRef }); // Local 1: initially uninitialized.
        module.Codes.Add(body);
        module.ToInstance<object>();
    }

    private static void AssertUninitialized(params Instruction[] code)
    {
        var exception = Assert.ThrowsExactly<ModuleLoadException>(() => Compile(code));
        StringAssert.Contains(exception.Message, "uninitialized local");
    }

    /// <summary>Shape 1 — <c>local.get</c> after <c>local.set</c> is valid.</summary>
    [TestMethod]
    public void LocalInit_GetAfterSet_Valid() => Compile(
        new LocalGet(0), new LocalSet(1),
        new LocalGet(1), new Drop(),
        new End());

    /// <summary>Shape 2 — <c>local.tee</c> initializes the local, so a later <c>local.get</c> is valid.</summary>
    [TestMethod]
    public void LocalInit_GetAfterTee_Valid() => Compile(
        new LocalGet(0), new LocalTee(1), new Drop(),
        new LocalGet(1), new Drop(),
        new End());

    /// <summary>Shape 3 — a set before a block persists into the (inner) block, so a get there is valid.</summary>
    [TestMethod]
    public void LocalInit_SetBeforeBlock_GetInsideBlock_Valid() => Compile(
        new LocalGet(0), new LocalSet(1),
        new Block(BlockType.Empty),
            new LocalGet(1), new Drop(),
        new End(),
        new End());

    /// <summary>Shape 4 — <c>local.get</c> before any assignment is invalid ("uninitialized local").</summary>
    [TestMethod]
    public void LocalInit_GetBeforeSet_Invalid() => AssertUninitialized(
        new LocalGet(1), new Drop(),
        new End());

    /// <summary>Shape 5 — a set inside a block does not survive its <c>end</c>; a later get is invalid.</summary>
    [TestMethod]
    public void LocalInit_SetInsideBlock_GetAfterEnd_Invalid() => AssertUninitialized(
        new Block(BlockType.Empty),
            new LocalGet(0), new LocalSet(1),
        new End(),
        new LocalGet(1), new Drop(),
        new End());

    /// <summary>Shape 6 — <c>else</c> resets to the <c>if</c>-entry state, so a get in the else-arm of a
    /// then-arm-only assignment is invalid.</summary>
    [TestMethod]
    public void LocalInit_SetInThenArm_GetInElseArm_Invalid() => AssertUninitialized(
        new Int32Constant(1),
        new If(BlockType.Empty),
            new LocalGet(0), new LocalSet(1),
        new Else(),
            new LocalGet(1), new Drop(),
        new End(),
        new End());

    /// <summary>Shape 7 — even when both if-arms assign, the initialization does not survive the construct's
    /// <c>end</c>; a get afterward is invalid.</summary>
    [TestMethod]
    public void LocalInit_BothArmsSet_GetAfterEnd_Invalid() => AssertUninitialized(
        new Int32Constant(1),
        new If(BlockType.Empty),
            new LocalGet(0), new LocalSet(1),
        new Else(),
            new LocalGet(0), new LocalSet(1),
        new End(),
        new LocalGet(1), new Drop(),
        new End());

    /// <summary>A non-nullable reference parameter is initialized on entry, so reading it is always valid.</summary>
    [TestMethod]
    public void LocalInit_ParameterIsInitialized_Valid() => Compile(
        new LocalGet(0), new Drop(),
        new End());
}
