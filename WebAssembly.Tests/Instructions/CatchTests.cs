using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Catch"/> instruction.
/// </summary>
[TestClass]
public class CatchTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Catch"/> instruction when there are no exceptions.
    /// </summary>
    [TestMethod]
    public void Catch_NoException()
    {
        Assert.AreEqual<int>(0, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
            new Instruction[]
            {
                new Try(),
                new Int32Constant(0),
                new Return(),
                new Catch(0),
                new Int32Constant(1),
                new Return(),
                new End(),

                new Int32Constant(2),
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

    /// <summary>
    /// Tests compilation and execution of the <see cref="Catch"/> instruction when there is an exception.
    /// </summary>
    [TestMethod]
    public void Catch_Exception()
    {
        Assert.AreEqual<int>(1, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
            new Instruction[]
            {
                new Try(),
                new Int32Constant(0),
                new Throw(0),
                new Catch(0),
                new Int32Constant(1),
                new Return(),
                new End(),

                new Int32Constant(2),
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

    /// <summary>
    /// Tests compilation and execution of the <see cref="Catch"/> instruction when there is an exception of the wrong type.
    /// </summary>
    [TestMethod]
    public void Catch_MultipleExceptionTags()
    {
        Assert.AreEqual<int>(2, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
            new Instruction[]
            {
                new Try(),
                new Int32Constant(0),
                new Throw(1),
                new Catch(0),
                new Int32Constant(1),
                new Return(),
                new Catch(1),
                new Int32Constant(2),
                new Return(),
                new End(),

                new Int32Constant(3),
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

                module.Tags.Add(new WebAssemblyTag
                {
                    TypeIndex = 1
                });
            }
        ).Test());
    }


    /// <summary>
    /// Tests compilation and execution of the <see cref="Rethrow"/> instruction.
    /// </summary>
    [TestMethod]
    public void Catch_TryInCatch()
    {
        Assert.AreEqual(1, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
            new Instruction[]
            {
                new Try(),

                    new Throw(0),

                new Catch(0),

                    new Try(),
                        new Throw(1),
                    new Catch(1),
                        new Int32Constant(1),
                        new Return(),
                    new End(),

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

                module.Tags.Add(new WebAssemblyTag
                {
                    TypeIndex = 1
                });
            }
        ).Test());
    }
}
