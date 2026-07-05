using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Throw"/> instruction.
/// </summary>
[TestClass]
public class ThrowTests
{
    /// <summary>Tests <see cref="Throw"/> property round-tripping and equality.</summary>
    [TestMethod]
    public void Throw_Equality()
    {
        var instruction = new Throw { Index = 3 };
        Assert.AreEqual(3u, instruction.Index);
        Assert.AreEqual(new Throw(3), instruction);
        Assert.AreNotEqual(new Throw { Index = 4 }, instruction);
        Assert.AreEqual(new Throw { Index = 3 }.GetHashCode(), instruction.GetHashCode());
        Assert.AreEqual(OpCode.Throw, instruction.OpCode);
    }

    /// <summary>Exposes a throwing function and the exported tag.</summary>
    public abstract class ThrowExports
    {
        /// <summary>Runs the throwing function.</summary>
        public abstract void Run();

        /// <summary>The exported exception tag.</summary>
        public abstract ExceptionTag e { get; }
    }

    private static WebAssemblyException RunAndCaptureThrow(ThrowExports exports)
    {
        try
        {
            exports.Run();
        }
        catch (TargetInvocationException x) when (x.InnerException is WebAssemblyException wae)
        {
            return wae;
        }
        catch (WebAssemblyException wae)
        {
            return wae;
        }

        throw new AssertFailedException("Run did not throw a WebAssemblyException.");
    }

    /// <summary>A thrown tag with an i32 operand surfaces to the .NET caller as a <see cref="WebAssemblyException"/>.</summary>
    [TestMethod]
    public void Throw_SingleOperand_SurfacesAsWebAssemblyException()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType());                                            // 0: () -> ()
        var withI32 = new WebAssemblyType();
        withI32.Parameters.Add(WebAssemblyValueType.Int32);
        module.Types.Add(withI32);                                                          // 1: (i32) -> ()
        module.Tags.Add(new Tag(1));
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export("Run", 0, ExternalKind.Function));
        module.Exports.Add(new Export("e", 0, ExternalKind.Tag));
        module.Codes.Add(new FunctionBody
        {
            Code = [new Int32Constant(42), new Throw(0), new End()],
        });

        var exports = module.ToInstance<ThrowExports>().Exports;
        var thrown = RunAndCaptureThrow(exports);

        Assert.AreSame(exports.e, thrown.Tag);
        Assert.AreEqual(1, thrown.Arguments.Count);
        Assert.AreEqual(42, thrown.Arguments[0]);
        Assert.AreEqual(42, thrown.GetArgument(0));
    }

    /// <summary>A niladic thrown tag surfaces with the exported tag and no arguments.</summary>
    [TestMethod]
    public void Throw_NoOperand_SurfacesAsWebAssemblyException()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType());     // 0: () -> ()
        module.Tags.Add(new Tag(0));
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export("Run", 0, ExternalKind.Function));
        module.Exports.Add(new Export("e", 0, ExternalKind.Tag));
        module.Codes.Add(new FunctionBody
        {
            Code = [new Throw(0), new End()],
        });

        var exports = module.ToInstance<ThrowExports>().Exports;
        var thrown = RunAndCaptureThrow(exports);

        Assert.AreSame(exports.e, thrown.Tag);
        Assert.AreEqual(0, thrown.Arguments.Count);
    }

    /// <summary>A thrown tag with several operands carries all of them, in order, boxed.</summary>
    [TestMethod]
    public void Throw_MultipleOperands_CarriesAllInOrder()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType());     // 0: () -> ()
        var sig = new WebAssemblyType();
        sig.Parameters.Add(WebAssemblyValueType.Int32);
        sig.Parameters.Add(WebAssemblyValueType.Int64);
        sig.Parameters.Add(WebAssemblyValueType.Float64);
        module.Types.Add(sig);                       // 1: (i32 i64 f64) -> ()
        module.Tags.Add(new Tag(1));
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export("Run", 0, ExternalKind.Function));
        module.Exports.Add(new Export("e", 0, ExternalKind.Tag));
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(7),
                new Int64Constant(8),
                new Float64Constant(9.5),
                new Throw(0),
                new End(),
            ],
        });

        var thrown = RunAndCaptureThrow(module.ToInstance<ThrowExports>().Exports);

        Assert.AreEqual(3, thrown.Arguments.Count);
        Assert.AreEqual(7, thrown.Arguments[0]);
        Assert.AreEqual(8L, thrown.Arguments[1]);
        Assert.AreEqual(9.5, thrown.Arguments[2]);
    }

    /// <summary>A <see cref="Throw"/> of a tag index the module doesn't define is rejected at compile.</summary>
    [TestMethod]
    public void Throw_UnknownTag_Rejected()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType());
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export("Run", 0, ExternalKind.Function));
        module.Codes.Add(new FunctionBody
        {
            Code = [new Throw(0), new End()],
        });

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<ThrowExports>());
    }
}
