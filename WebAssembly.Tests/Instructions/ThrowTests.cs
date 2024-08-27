using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Throw"/> instruction.
/// </summary>
[TestClass]
public class ThrowTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Throw"/> instruction.
    /// </summary>
    [TestMethod]
    public void Throw_Exception()
    {
        Assert.ThrowsException<WebAssemblyException>(() =>
        {
            AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
                new Instruction[]
                {
                    new Throw(0),

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
