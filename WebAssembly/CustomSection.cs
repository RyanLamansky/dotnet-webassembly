using System.Collections.Generic;

namespace WebAssembly
{
	/// <summary>
	/// Contains arbitrary bytes that may have meaning in certain environments.
	/// </summary>
	public class CustomSection
	{
		/// <summary>
		/// Creates a new custom section.
		/// </summary>
		public CustomSection()
		{
		}

		private string name;

		/// <summary>
		/// The name of the custom section; nulls are converted to <see cref="string.Empty"/>.
		/// </summary>
		public string Name
		{
			get => this.name ?? (this.name = string.Empty);
			set => this.name = value ?? string.Empty;
		}

		private IList<byte> content;

		/// <summary>
		/// The content of the custom section; nulls are converted to an empty modifiable collection.
		/// </summary>
		public IList<byte> Content
		{
			get => this.content ?? (this.content = new List<byte>());
			set => this.content = value ?? new List<byte>();
		}
	}
}