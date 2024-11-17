using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Rethrow"/> instruction.
/// </summary>
[TestClass]
public class RethrowTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Rethrow"/> instruction.
    /// </summary>
    [TestMethod]
    public void Rethrow_Exception()
    {
        Assert.ThrowsException<WebAssemblyException>(() =>
        {
            AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
                new Instruction[]
                {
                    new Try(),
                    new Throw(0),
                    new Catch(0),
                    new Rethrow(0),
                    new End(),

                    new Int32Constant(0),
                    new End()
                },
                module =>
                {
                    module.Types.Add(new WebAssemblyType
                    {
                        Parameters = Array.Empty<WebAssemblyValueType>()
                    });

                    module.Tags.Add(new WebAssemblyTag
                    {
                        TypeIndex = 1
                    });
                }
            ).Test();
        });
    }
}
