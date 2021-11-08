using System;
using System.IO;
using WebAssembly.Runtime;

namespace WebAssembly
{
    /// <summary>
    /// Tested functions for interacting with UnmanagedMemory
    /// </summary>
    public static class UnmanagedMemoryExtensions
    {
        /// <summary>
        /// Allows copying from a BinaryReader into a new UnmanagedMemory
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static unsafe UnmanagedMemory ReadIntoNewUnmanagedMemory(this BinaryReader reader)
        {
            var pagesToFit = 1 + (uint)(reader.BaseStream.Length / Memory.PageSize);
            var unmanagedMem = new UnmanagedMemory(pagesToFit, pagesToFit);
            unmanagedMem.ImportStream(0, (uint)reader.BaseStream.Length, reader);
            return unmanagedMem;
        }
        
        /// <summary>
        /// Allows importing length data from the current position of a BinaryReader to the offset in the UnmanagedMemory.
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="atOffset">location in UnmanagedMemory to write to</param>
        /// <param name="length">number of bytes to copy</param>
        /// <param name="reader">source of the data</param>
        /// <exception cref="MemoryAccessOutOfRangeException">Thrown if atOffset + length exceeds the maximum capacity of the UnmanagedMemory</exception>
        public static unsafe void ImportStream(this UnmanagedMemory mem, uint atOffset, uint length, BinaryReader reader)
        {
            if (atOffset + length > mem.Maximum * Memory.PageSize)
            {
                throw new MemoryAccessOutOfRangeException(atOffset, length);
            }
            // grow if we need to
            if ((mem.Current * Memory.PageSize) - atOffset < length)
            {
                var pagesToFit = 1+ (length / Memory.PageSize);
                mem.Grow(pagesToFit);
            }
            var unmanagedMem = mem;
            unsafe
            {
                var dest = (long*)(unmanagedMem.Start + (int)atOffset);
                // copy from BinaryReader to UnmanagedMemory
                for (var i = 0; (i+1) * sizeof(long) < length; i++)
                {
                    dest[i] = reader.ReadInt64();
                }

                // did we copy it all in 8 byte chunks, or do we need to copy a few extra bytes?
                if (length % sizeof(long) != 0)
                {
                    // start from where we left off with the 8 byte copies
                    for (var i = (length / sizeof(long)) * 8; i < length; i++)
                    {
                        ((byte*)dest)[i] = reader.ReadByte();
                    }
                }
            }
        }
        
        /// <summary>
        /// Allows computing a standard Crc32 over a region of UnmanagedMemory
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static unsafe uint Compute_CRC32_Simple(this UnmanagedMemory mem, int offset, int length)
        {
            const uint polynomial  = 0x04C11DB7;
            byte* start = (byte*)(mem.Start + offset);
            uint crc = 0;
            for(int j = 0; j < length; j ++){
                crc ^= (uint)(start[j] << 24);
                
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x80000000) != 0)
                    {
                        crc = (uint)((crc << 1) ^ polynomial);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return crc;
        }
    }
}