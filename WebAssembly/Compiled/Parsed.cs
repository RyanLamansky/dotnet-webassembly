using System;
using System.Collections.Generic;

namespace WebAssembly.Compiled
{
	internal sealed class Parsed
	{
		public Parsed(Reader reader)
		{
			if (reader.ReadUInt32() != Module.Magic)
				throw new ModuleLoadException("File preamble magic value is incorrect.", 0);

			switch (reader.ReadUInt32())
			{
				case 0x1: //First release
				case 0xd: //Final pre-release, binary format is identical with first release.
					break;
				default:
					throw new ModuleLoadException("Unsupported version, only version 0x1 and 0xd are accepted.", 4);
			}

			Signature[] signatures = null;
			Signature[] functionSignatures = null;
			KeyValuePair<string, uint>[] exportedFunctions = null;
			Function[] functions = null;
			var previousSection = Section.None;

			while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
			{
				var payloadLength = reader.ReadVarUInt32();
				if (id != 0 && (Section)id < previousSection)
					throw new ModuleLoadException($"Sections out of order; section {(Section)id} encounterd after {previousSection}.", reader.Offset);

				switch ((Section)id)
				{
					case Section.Type:
						{
							signatures = new Signature[reader.ReadVarUInt32()];

							for (var i = 0; i < signatures.Length; i++)
								signatures[i] = new Signature(reader);
						}
						break;

					case Section.Function:
						{
							functionSignatures = new Signature[reader.ReadVarUInt32()];

							for (var i = 0; i < functionSignatures.Length; i++)
								functionSignatures[i] = signatures[reader.ReadVarUInt32()];
						}
						break;

					case Section.Memory:
						{
							var count = reader.ReadVarUInt32();
							if (count > 1)
								throw new ModuleLoadException("Multiple memory values are not supported.", reader.Offset);

							var setFlags = (ResizableLimits.Flags)reader.ReadVarUInt32();
							this.MemoryPagesMinimum = reader.ReadVarUInt32();
							if ((setFlags & ResizableLimits.Flags.Maximum) != 0)
								this.MemoryPagesMaximum = reader.ReadVarUInt32();
							else
								this.MemoryPagesMaximum = this.MemoryPagesMinimum;
						}
						break;

					case Section.Export:
						{
							var totalExports = reader.ReadVarUInt32();
							var xFunctions = new List<KeyValuePair<string, uint>>((int)Math.Min(int.MaxValue, totalExports));

							for (var i = 0; i < totalExports; i++)
							{
								var name = reader.ReadString(reader.ReadVarUInt32());

								var kind = (ExternalKind)reader.ReadByte();
								switch (kind)
								{
									case ExternalKind.Function:
										xFunctions.Add(new KeyValuePair<string, uint>(name, reader.ReadVarUInt32()));
										break;
									default:
										throw new NotSupportedException($"Unsupported or unrecognized export kind {kind}.");
								}
							}

							exportedFunctions = xFunctions.ToArray();
						}
						break;

					case Section.Code:
						{
							functions = new Function[reader.ReadVarUInt32()];

							for (var i = 0; i < functions.Length; i++)
								functions[i] = new Function(reader, functionSignatures[i], reader.ReadVarUInt32());
						}
						break;

					default:
						throw new ModuleLoadException($"Unrecognized section type {id}.", reader.Offset);
				}

				previousSection = (Section)id;
			}

			this.Functions = functions;
			this.ExportedFunctions = exportedFunctions;
		}

		internal readonly Function[] Functions;
		internal readonly KeyValuePair<string, uint>[] ExportedFunctions;
		internal readonly uint MemoryPagesMinimum;
		internal readonly uint MemoryPagesMaximum;
	}
}