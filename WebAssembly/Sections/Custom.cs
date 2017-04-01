using System.Collections.Generic;

namespace WebAssembly.Sections
{
	class Custom : Section
	{
		/// <summary>
		/// Creates a new custom section.
		/// </summary>
		public Custom()
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

		/// <summary>
		/// Summarizes the custom section as a string.
		/// </summary>
		/// <returns>A string summary of this custom section.</returns>
		public override string ToString() => $"Custom: {this.Name}, {this.content.Count:n}";
	}
}