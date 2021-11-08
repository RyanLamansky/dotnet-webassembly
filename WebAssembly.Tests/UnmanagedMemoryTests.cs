using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly
{
    /// <summary>
    /// Tests basic functionality of <see cref="UnmanagedMemory"/> when used with <see cref="MemoryImport"/>.
    /// </summary>
    [TestClass]
    public class UnmanagedMemoryTests
    {
        /// <summary>
        /// Used in the Crc32ImportedMemory test
        /// </summary>
        public abstract class MemoryCheck
        {
            /// <summary>
            /// calculate crc32 of the range of specified memory 
            /// </summary>
            /// <param name="offset">start</param>
            /// <param name="length">count</param>
            /// <returns>checksum</returns>
            public abstract int compute_CRC32_Simple(int offset, int length);
        }

        /// <summary>
        /// Demonstrates streaming data into an UnmanagedMemory produces same UnmanagedMemory as doing a Marshal.Copy
        /// </summary>
        [TestMethod]
        public void BinaryReaderIntoUnmanagedMemory()
        {
            using var verifyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebAssembly.Samples.memoryCheck.wat"); 
            using var streamData = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebAssembly.Samples.memoryCheck.wat");
            if (streamData == null || verifyStream == null)
            {
                throw new InvalidOperationException("StreamData or VerifyStream was null");
            }
            int length = (int)streamData.Length;
            
            var verifyMem = new BinaryReader(verifyStream).ReadBytes((int)verifyStream.Length);
            verifyStream.Position = 0;
            var verifyUnmanagedMem = new UnmanagedMemory(1, 1);
            Marshal.Copy(verifyMem,0,verifyUnmanagedMem.Start,verifyMem.Length);
            
            var loadedMemory = new BinaryReader(streamData ).ReadIntoNewUnmanagedMemory();
            
            var hostSideCrc = loadedMemory.Compute_CRC32_Simple(0, length);
            var verifyCrc = verifyUnmanagedMem.Compute_CRC32_Simple(0, length);
            Assert.AreEqual(verifyCrc, hostSideCrc);
        }
        
        /// <summary>
        /// Demonstrates an example of moving data from a binaryReader to UnmanagedMemory
        /// </summary>
        [TestMethod]
        public void Crc32ImportedMemory()
        {
            // just using as an placeholder. Actual stream comes from a database
            using var verifyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebAssembly.Samples.memoryCheck.wat"); 
            using var streamData = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebAssembly.Samples.memoryCheck.wat");
            if (streamData == null || verifyStream == null)
            {
                throw new InvalidOperationException("StreamData or VerifyStream was null");
            }
            int length = (int)streamData.Length;
            var loadedMemory = new BinaryReader(streamData ).ReadIntoNewUnmanagedMemory();

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebAssembly.Samples.memoryCheck.wasm");
            var mod = Compile.FromBinary<MemoryCheck>(stream ?? throw new InvalidOperationException("Stream was null"));
            var inst = mod.Invoke(new ImportDictionary
            {
                { "env", "memory", new MemoryImport(() => loadedMemory) }
            });
            var hostSideCrc = loadedMemory.Compute_CRC32_Simple(0, length);
            var wasmSideCrc = (uint)inst.Exports.compute_CRC32_Simple(0, length);
            
            Assert.AreEqual(hostSideCrc, wasmSideCrc);
        }
    }
}