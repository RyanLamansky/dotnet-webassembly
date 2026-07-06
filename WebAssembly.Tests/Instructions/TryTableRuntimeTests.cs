using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Runtime tests for <see cref="TryTable"/> compilation.
/// </summary>
[TestClass]
public class TryTableRuntimeTests
{
    /// <summary>Exposes an i32 -> i32 function named "f".</summary>
    public abstract class Int32ToInt32
    {
        /// <summary>Invokes the compiled function.</summary>
        public abstract int f(int value);
    }

    /// <summary>Exposes a niladic i32-returning function named "f".</summary>
    public abstract class ToInt32
    {
        /// <summary>Invokes the compiled function.</summary>
        public abstract int f();
    }

    // block $h { try_table (result i32) (catch $e $h) { local.get 0; i32.eqz; if { throw $e } end; i32.const 42 } return } i32.const 23
    private static Module SimpleThrowCatchModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // 0: () -> ()
        var sig = new WebAssemblyType();
        sig.Parameters.Add(WebAssemblyValueType.Int32);
        sig.Returns.Add(WebAssemblyValueType.Int32);
        module.Types.Add(sig); // 1: (i32) -> (i32)
        module.Tags.Add(new Tag(0));
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("f", 0, ExternalKind.Function));

        var tryTable = new TryTable(BlockType.Int32);
        tryTable.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 0));

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Empty),
                    tryTable,
                        new LocalGet(0),
                        new Int32EqualZero(),
                        new If(BlockType.Empty),
                            new Throw(0),
                        new End(),
                        new Int32Constant(42),
                    new End(), // end try_table
                    new Return(),
                new End(), // end block $h
                new Int32Constant(23),
                new End(), // end function
            ],
        });
        return module;
    }

    /// <summary>A throw inside a try_table is caught by a matching clause and control branches to the label.</summary>
    [TestMethod]
    public void TryTable_SimpleThrowCatch()
    {
        var f = SimpleThrowCatchModule().ToInstance<Int32ToInt32>().Exports;
        Assert.AreEqual(42, f.f(1), "No throw: the try body's result flows out.");
        Assert.AreEqual(23, f.f(0), "Throw caught: control reaches the catch label.");
    }

    /// <summary>A trap (unreachable) inside a try_table escapes even a catch_all — it is not a WebAssembly exception.</summary>
    [TestMethod]
    public void TryTable_TrapEscapesCatchAll()
    {
        // block $h { try_table (result i32) (catch_all $h) { unreachable } } i32.const 7
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // 0: () -> ()
        var sig = new WebAssemblyType();
        sig.Returns.Add(WebAssemblyValueType.Int32);
        module.Types.Add(sig); // 1: () -> (i32)
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("f", 0, ExternalKind.Function));

        var tryTable = new TryTable(BlockType.Int32);
        tryTable.Catches.Add(new CatchClause(CatchKind.CatchAll, 0));

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Empty),
                    tryTable,
                        new Unreachable(),
                    new End(),
                    new Return(),
                new End(),
                new Int32Constant(7),
                new End(),
            ],
        });

        var f = module.ToInstance<ToInt32>().Exports;
        Assert.ThrowsExactly<UnreachableException>(() => f.f());
    }

    /// <summary>A divide-by-zero trap inside a try_table escapes a catch_all as a .NET exception, not a caught WASM exception.</summary>
    [TestMethod]
    public void TryTable_DivideByZeroEscapesCatchAll()
    {
        // block $h { try_table (result i32) (catch_all $h) { local.get 0; i32.const 0; i32.div_s } return } i32.const 7
        var module = new Module();
        module.Types.Add(new WebAssemblyType());
        var sig = new WebAssemblyType();
        sig.Parameters.Add(WebAssemblyValueType.Int32);
        sig.Returns.Add(WebAssemblyValueType.Int32);
        module.Types.Add(sig); // 1: (i32) -> (i32)
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("f", 0, ExternalKind.Function));

        var tryTable = new TryTable(BlockType.Int32);
        tryTable.Catches.Add(new CatchClause(CatchKind.CatchAll, 0));

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Empty),
                    tryTable,
                        new LocalGet(0),
                        new Int32Constant(0),
                        new Int32DivideSigned(),
                    new End(),
                    new Return(),
                new End(),
                new Int32Constant(7),
                new End(),
            ],
        });

        var f = module.ToInstance<Int32ToInt32>().Exports;
        Assert.ThrowsExactly<DivideByZeroException>(() => f.f(10));
    }

    /// <summary>Exposes a niladic void function named "f".</summary>
    public abstract class VoidToVoid
    {
        /// <summary>Invokes the compiled function.</summary>
        public abstract void f();
    }

    /// <summary>A tail call inside a try_table replaces the frame: 5,000,000-deep self-recursion does not grow the stack.</summary>
    [TestMethod]
    public void TryTable_TailCall_DeepRecursion_ConstantStack()
    {
        // f(n) = n == 0 ? 0 : (block $h { try_table (catch_all $h) { return_call f(n-1) } } 0)
        var module = new Module();
        var sig = new WebAssemblyType();
        sig.Parameters.Add(WebAssemblyValueType.Int32);
        sig.Returns.Add(WebAssemblyValueType.Int32);
        module.Types.Add(sig); // 0: (i32) -> (i32)
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export("f", 0, ExternalKind.Function));

        var tryTable = new TryTable(BlockType.Empty);
        tryTable.Catches.Add(new CatchClause(CatchKind.CatchAll, 0));

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),
                new Int32EqualZero(),
                new If(BlockType.Int32),
                    new Int32Constant(0),
                new Else(),
                    new Block(BlockType.Empty),
                        tryTable,
                            new LocalGet(0),
                            new Int32Constant(1),
                            new Int32Subtract(),
                            new ReturnCall(0),
                        new End(),
                    new End(),
                    new Int32Constant(0),
                new End(),
                new End(),
            ],
        });

        var f = module.ToInstance<Int32ToInt32>().Exports;
        Assert.AreEqual(0, f.f(5_000_000));
    }

    /// <summary>A throw in a tail-called callee escapes the caller's enclosing try_table (the frame was replaced).</summary>
    [TestMethod]
    public void TryTable_TailCall_CalleeThrowEscapes()
    {
        // $thrower() { throw $e }   f() { block $h { try_table (catch_all $h) { return_call $thrower } } }
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // 0: () -> ()
        module.Tags.Add(new Tag(0));
        module.Functions.Add(new Function(0)); // 0: thrower
        module.Functions.Add(new Function(0)); // 1: f
        module.Exports.Add(new Export("f", 1, ExternalKind.Function));

        module.Codes.Add(new FunctionBody { Code = [new Throw(0), new End()] });

        var tryTable = new TryTable(BlockType.Empty);
        tryTable.Catches.Add(new CatchClause(CatchKind.CatchAll, 0));
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Empty),
                    tryTable,
                        new ReturnCall(0),
                    new End(),
                new End(),
                new End(),
            ],
        });

        var f = module.ToInstance<VoidToVoid>().Exports;
        try
        {
            f.f();
        }
        catch (System.Reflection.TargetInvocationException x) when (x.InnerException is WebAssemblyException)
        {
            return;
        }
        catch (WebAssemblyException)
        {
            return;
        }
        throw new AssertFailedException("The tail-called callee's throw should have escaped the try_table.");
    }

    /// <summary>A value on the operand stack below a try_table survives the entry spill and its end reload.</summary>
    [TestMethod]
    public void TryTable_ValueBelowTry_SurvivesSpill()
    {
        // 100 ; block $h (result i32) { try_table (result i32) (catch $e-i32 $h) { local.get 0; if { i32.const 5; throw $e-i32 } end; i32.const 42 } } ; i32.add
        var module = new Module();
        var tagSig = new WebAssemblyType();
        tagSig.Parameters.Add(WebAssemblyValueType.Int32);
        module.Types.Add(tagSig); // 0: (i32) -> ()
        var funcSig = new WebAssemblyType();
        funcSig.Parameters.Add(WebAssemblyValueType.Int32);
        funcSig.Returns.Add(WebAssemblyValueType.Int32);
        module.Types.Add(funcSig); // 1: (i32) -> (i32)
        module.Tags.Add(new Tag(0));
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("f", 0, ExternalKind.Function));

        var tryTable = new TryTable(BlockType.Int32);
        tryTable.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 0));

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(100),
                new Block(BlockType.Int32),
                    tryTable,
                        new LocalGet(0),
                        new If(BlockType.Empty),
                            new Int32Constant(5),
                            new Throw(0),
                        new End(),
                        new Int32Constant(42),
                    new End(),
                new End(),
                new Int32Add(),
                new End(),
            ],
        });

        var f = module.ToInstance<Int32ToInt32>().Exports;
        Assert.AreEqual(142, f.f(0), "No throw: 100 + 42.");
        Assert.AreEqual(105, f.f(1), "Caught: 100 + the ferried operand 5.");
    }

    /// <summary>A `return` inside a protected region leaves to the function boundary, carrying the result.</summary>
    [TestMethod]
    public void TryTable_ReturnInsideRegion()
    {
        // block $h { try_table (catch $e $h) { local.get 0; if { i32.const 7; return } end; throw $e } } i32.const 9
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // 0: () -> ()
        var funcSig = new WebAssemblyType();
        funcSig.Parameters.Add(WebAssemblyValueType.Int32);
        funcSig.Returns.Add(WebAssemblyValueType.Int32);
        module.Types.Add(funcSig); // 1: (i32) -> (i32)
        module.Tags.Add(new Tag(0));
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("f", 0, ExternalKind.Function));

        var tryTable = new TryTable(BlockType.Empty);
        tryTable.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 0));

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Empty),
                    tryTable,
                        new LocalGet(0),
                        new If(BlockType.Empty),
                            new Int32Constant(7),
                            new Return(),
                        new End(),
                        new Throw(0),
                    new End(),
                new End(),
                new Int32Constant(9),
                new End(),
            ],
        });

        var f = module.ToInstance<Int32ToInt32>().Exports;
        Assert.AreEqual(7, f.f(1), "In-region return.");
        Assert.AreEqual(9, f.f(0), "Throw caught, control reaches the catch label.");
    }

    /// <summary>A branch that crosses two nested protected regions reaches a block outside both, carrying its value.</summary>
    [TestMethod]
    public void TryTable_BranchCrossingTwoRegions()
    {
        // block $t (result i32) { block $v { try_table (catch_all $v) { try_table (catch_all 0) { i32.const 77; br $t } } } i32.const -1 }
        var module = new Module();
        var sig = new WebAssemblyType();
        sig.Returns.Add(WebAssemblyValueType.Int32);
        module.Types.Add(sig); // 0: () -> (i32)
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export("f", 0, ExternalKind.Function));

        var outer = new TryTable(BlockType.Empty);
        outer.Catches.Add(new CatchClause(CatchKind.CatchAll, 0)); // -> $v
        var inner = new TryTable(BlockType.Empty);
        inner.Catches.Add(new CatchClause(CatchKind.CatchAll, 0)); // -> outer try end

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Int32),   // $t (depthKey 2)
                    new Block(BlockType.Empty), // $v (depthKey 3)
                        outer,                  // depthKey 4
                            inner,              // depthKey 5
                                new Int32Constant(77),
                                new Branch(3),  // -> $t (depthKey 2)
                            new End(),
                        new End(),
                    new End(),
                    new Int32Constant(-1),
                new End(),
                new End(),
            ],
        });

        var f = module.ToInstance<ToInt32>().Exports;
        Assert.AreEqual(77, f.f());
    }

    /// <summary>A loop back-edge that crosses a protected region iterates correctly.</summary>
    [TestMethod]
    public void TryTable_LoopBackEdgeCrossingRegion()
    {
        // block $exit (result i32) { loop $loop { try_table (catch_all $loop) {
        //   local.get 0; i32.eqz; if { i32.const 123; br $exit } end;
        //   local.get 0; i32.const 1; i32.sub; local.set 0; br $loop } } i32.const -1 }
        var module = new Module();
        var sig = new WebAssemblyType();
        sig.Parameters.Add(WebAssemblyValueType.Int32);
        sig.Returns.Add(WebAssemblyValueType.Int32);
        module.Types.Add(sig); // 0: (i32) -> (i32)
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export("f", 0, ExternalKind.Function));

        var tryTable = new TryTable(BlockType.Empty);
        tryTable.Catches.Add(new CatchClause(CatchKind.CatchAll, 0)); // -> $loop

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Int32),   // $exit (depthKey 2)
                    new Loop(BlockType.Empty), // $loop (depthKey 3)
                        tryTable,              // depthKey 4
                            new LocalGet(0),
                            new Int32EqualZero(),
                            new If(BlockType.Empty),
                                new Int32Constant(123),
                                new Branch(3), // -> $exit (depthKey 2)
                            new End(),
                            new LocalGet(0),
                            new Int32Constant(1),
                            new Int32Subtract(),
                            new LocalSet(0),
                            new Branch(1),     // -> $loop back-edge (depthKey 3)
                        new End(),
                    new End(),
                    new Int32Constant(-1),
                new End(),
                new End(),
            ],
        });

        var f = module.ToInstance<Int32ToInt32>().Exports;
        Assert.AreEqual(123, f.f(0));
        Assert.AreEqual(123, f.f(4), "The back-edge must iterate four times before exiting.");
    }
}
