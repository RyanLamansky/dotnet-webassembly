using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests <see cref="Instruction"/> inheritors for proper behavior
    /// </summary>
    [TestClass]
    public class InstructionTests
    {
        private static readonly Type[] InstructionTypes =
            typeof(Instruction)
            .GetTypeInfo()
            .Assembly
            .GetTypes()
            .Where(type => type.IsDescendantOf(typeof(Instruction)) && type.IsAbstract == false && type.IsNested == false)
            .ToArray();

        /// <summary>
        /// Ensures that all instructions are public.
        /// </summary>
        [TestMethod]
        public void Instruction_AllPublic()
        {
            var nonPublic = string.Join(", ", InstructionTypes.Where(type => type.IsPublic == false));

            Assert.AreEqual("", nonPublic, "Non-public instructions found.");
        }

        /// <summary>
        /// Ensures that instruction names match their opcode name.
        /// </summary>
        [TestMethod]
        public void Instruction_NameMatchesOpcode()
        {
            var mismatch = string.Join(", ",
                InstructionTypes
                .Where(x => !x.IsSubclassOf(typeof(MiscellaneousInstruction)))
                .Select(type => (
                OpCode: ((Instruction)type.GetConstructor(System.Type.EmptyTypes)!.Invoke(null)).OpCode.ToString(),
                TypeName: type.Name
                ))
                .Where(result => result.OpCode != result.TypeName)
                .Select(result => result.TypeName)
                );

            Assert.AreEqual("", mismatch, "Instructions whose name do not match their opcode found.");
        }

        /// <summary>
        /// Ensures that miscellaneous instruction names match their miscellaneous opcode name.
        /// </summary>
        [TestMethod]
        public void Instruction_NameMatchesMiscellaneousOpcode()
        {
            var mismatch = string.Join(", ",
                InstructionTypes
                    .Where(x => x.IsSubclassOf(typeof(MiscellaneousInstruction)))
                    .Select(type => (
                        MiscellaneousOpCode: ((MiscellaneousInstruction)type.GetConstructor(System.Type.EmptyTypes)!.Invoke(null)).MiscellaneousOpCode.ToString(),
                        TypeName: type.Name
                    ))
                    .Where(result => result.MiscellaneousOpCode != result.TypeName)
                    .Select(result => result.TypeName)
            );

            Assert.AreEqual("", mismatch, "Instructions whose name do not match their miscellaneous opcode found.");
        }

        /// <summary>
        /// Ensures that every instruction has a corresponding test class.
        /// </summary>
        [TestMethod]
        public void Instruction_HasTestClass()
        {
            var testClasses = typeof(InstructionTests)
                .Assembly
                .GetTypes()
                .Where(type => type.GetCustomAttribute<TestClassAttribute>() != null)
                .Select(type => type.Name.Substring(0, type.Name.Length - "Tests".Length));

            var missing = string.Join(", ", InstructionTypes.Select(type => type.Name).Except(testClasses));

            Assert.AreEqual("", missing, "Instructions with no matching test class found.");
        }

        /// <summary>
        /// Ensures that every instruction has a working <see cref="Instruction.ToString"/> implementation.
        /// </summary>
        [TestMethod]
        public void Instruction_ToStringWorks()
        {
            Assert.IsTrue(InstructionTypes.All(type => !string.IsNullOrWhiteSpace(((Instruction)type.GetConstructor(Type.EmptyTypes)!.Invoke(null)).ToString())));
        }

        /// <summary>
        /// Ensures that all instructions have a public parameterless constructor.
        /// </summary>
        [TestMethod]
        public void Instruction_HasPublicParameterlessConstructor()
        {
            static IEnumerable<string> GatherViolations()
            {
                foreach (var type in InstructionTypes)
                {
                    if (type.IsAbstract)
                        continue;
                    if (!type.IsDescendantOf<Instruction>())
                        continue;

                    if (type.GetConstructor(System.Type.EmptyTypes) == null)
                        yield return type.Name;
                }
            }

            Assert.AreEqual("", string.Join(", ", GatherViolations()), "Instructions must have a parameterless constructor.");
        }
    }
}