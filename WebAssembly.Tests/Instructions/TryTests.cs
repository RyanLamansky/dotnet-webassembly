using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Try"/> instruction.
/// </summary>
[TestClass]
public class TryTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Catch"/> instruction when there are no exceptions.
    /// </summary>
    [TestMethod]
    public void Try_ReturnValue()
    {
        Assert.AreEqual<int>(0, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
            new Instruction[]
            {
                new Try(BlockType.Int32),
                new Int32Constant(0),
                new Catch(0),
                new End(),
                new End()
            },
            module =>
            {
                module.Types.Add(new WebAssemblyType
                {
                    Parameters = new[]
                    {
                        WebAssemblyValueType.Int32
                    },
                });

                module.Tags.Add(new WebAssemblyTag
                {
                    TypeIndex = 1
                });
            }
        ).Test());
    }
}
