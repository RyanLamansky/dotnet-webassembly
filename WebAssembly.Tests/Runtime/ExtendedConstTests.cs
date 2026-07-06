using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the WebAssembly 3.0 extended-constant-expression support: arithmetic (i32/i64 add/sub/mul) and
/// <c>global.get</c> of a previously-declared immutable global inside global/data/element initializer expressions.
/// </summary>
[TestClass]
public class ExtendedConstTests
{
    /// <summary>Exposes the three arithmetic-initialized globals.</summary>
    public abstract class ArithmeticExports
    {
        /// <summary>Gets the i32.add-initialized global.</summary>
        public abstract int Added();

        /// <summary>Gets the i32.sub / i32.mul-initialized global.</summary>
        public abstract int Mixed();

        /// <summary>Gets the i64.mul-initialized global.</summary>
        public abstract long Product();
    }

    /// <summary>
    /// A constant expression may compute a global's value with i32/i64 add, sub, and mul.
    /// </summary>
    [TestMethod]
    public void ExtendedConst_GlobalArithmetic_Compiled()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int64] });

        // 10 + 20 = 30
        module.Globals.Add(new Global(WebAssemblyValueType.Int32,
            new Int32Constant(10), new Int32Constant(20), new Int32Add(), new End()));
        // (7 - 3) * 5 = 20
        module.Globals.Add(new Global(WebAssemblyValueType.Int32,
            new Int32Constant(7), new Int32Constant(3), new Int32Subtract(),
            new Int32Constant(5), new Int32Multiply(), new End()));
        // 6 * 7 = 42
        module.Globals.Add(new Global(WebAssemblyValueType.Int64,
            new Int64Constant(6), new Int64Constant(7), new Int64Multiply(), new End()));

        module.Functions.Add(new Function(0));
        module.Functions.Add(new Function(0));
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(ArithmeticExports.Added), 0));
        module.Exports.Add(new Export(nameof(ArithmeticExports.Mixed), 1));
        module.Exports.Add(new Export(nameof(ArithmeticExports.Product), 2));
        module.Codes.Add(new FunctionBody(new GlobalGet(0), new End()));
        module.Codes.Add(new FunctionBody(new GlobalGet(1), new End()));
        module.Codes.Add(new FunctionBody(new GlobalGet(2), new End()));

        var exports = module.ToInstance<ArithmeticExports>().Exports;
        Assert.AreEqual(30, exports.Added());
        Assert.AreEqual(20, exports.Mixed());
        Assert.AreEqual(42L, exports.Product());
    }

    /// <summary>i32/i64 arithmetic in a constant expression wraps on overflow (unchecked), like the runtime ops.</summary>
    [TestMethod]
    public void ExtendedConst_GlobalArithmetic_Wraps()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });

        // int.MaxValue + 1 wraps to int.MinValue.
        module.Globals.Add(new Global(WebAssemblyValueType.Int32,
            new Int32Constant(int.MaxValue), new Int32Constant(1), new Int32Add(), new End()));
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(SingleInt32Export.Value), 0));
        module.Codes.Add(new FunctionBody(new GlobalGet(0), new End()));

        var exports = module.ToInstance<SingleInt32Export>().Exports;
        Assert.AreEqual(int.MinValue, exports.Value());
    }

    /// <summary>Exposes a single i32 global.</summary>
    public abstract class SingleInt32Export
    {
        /// <summary>Gets the global.</summary>
        public abstract int Value();
    }

    /// <summary>
    /// A constant expression may <c>global.get</c> an earlier module-defined immutable global (3.0 lifted the old
    /// imports-only restriction) and combine it with arithmetic.
    /// </summary>
    [TestMethod]
    public void ExtendedConst_GlobalGetPreviousGlobal_Compiled()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });

        module.Globals.Add(new Global(WebAssemblyValueType.Int32, new Int32Constant(5), new End())); // g0 = 5
        // g1 = g0 * 8 + 2 = 42
        module.Globals.Add(new Global(WebAssemblyValueType.Int32,
            new GlobalGet(0), new Int32Constant(8), new Int32Multiply(),
            new Int32Constant(2), new Int32Add(), new End()));

        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(SingleInt32Export.Value), 0));
        module.Codes.Add(new FunctionBody(new GlobalGet(1), new End()));

        var exports = module.ToInstance<SingleInt32Export>().Exports;
        Assert.AreEqual(42, exports.Value());
    }

    /// <summary>Reads the byte written by an arithmetic-offset active data segment.</summary>
    public abstract class DataByteExport
    {
        /// <summary>Reads memory[8] as an unsigned byte.</summary>
        public abstract int ByteAt8();
    }

    /// <summary>
    /// An active data segment's offset may be an extended constant expression (here <c>global.get</c> plus i32.add).
    /// </summary>
    [TestMethod]
    public void ExtendedConst_DataOffsetArithmetic_Compiled()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Memories.Add(new Memory(1, 1));

        module.Globals.Add(new Global(WebAssemblyValueType.Int32, new Int32Constant(4), new End())); // g0 = 4

        var data = new Data
        {
            // offset = g0 + 4 = 8
            InitializerExpression = [new GlobalGet(0), new Int32Constant(4), new Int32Add(), new End()],
        };
        data.RawData.Add(0xAB);
        module.Data.Add(data);

        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(DataByteExport.ByteAt8), 0));
        module.Codes.Add(new FunctionBody(new Int32Constant(8), new Int32Load8Unsigned(), new End()));

        var exports = module.ToInstance<DataByteExport>().Exports;
        Assert.AreEqual(0xAB, exports.ByteAt8());
    }

    /// <summary>
    /// A constant expression's <c>global.get</c> may not reference a not-yet-declared global (forward/self reference).
    /// </summary>
    [TestMethod]
    public void ExtendedConst_ForwardGlobalReference_Rejected()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });

        // g0 references g1, which is declared later — invalid.
        module.Globals.Add(new Global(WebAssemblyValueType.Int32, new GlobalGet(1), new End()));
        module.Globals.Add(new Global(WebAssemblyValueType.Int32, new Int32Constant(1), new End()));
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(SingleInt32Export.Value), 0));
        module.Codes.Add(new FunctionBody(new GlobalGet(0), new End()));

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<SingleInt32Export>());
    }

    /// <summary>
    /// A constant expression's <c>global.get</c> may reference only immutable globals, even previously-declared ones.
    /// </summary>
    [TestMethod]
    public void ExtendedConst_MutableGlobalReference_Rejected()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });

        module.Globals.Add(new Global
        {
            IsMutable = true,
            ContentType = WebAssemblyValueType.Int32,
            InitializerExpression = [new Int32Constant(5), new End()],
        }); // g0 mutable
        module.Globals.Add(new Global(WebAssemblyValueType.Int32, new GlobalGet(0), new End())); // references mutable g0
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(SingleInt32Export.Value), 1));
        module.Codes.Add(new FunctionBody(new GlobalGet(1), new End()));

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<SingleInt32Export>());
    }
}
