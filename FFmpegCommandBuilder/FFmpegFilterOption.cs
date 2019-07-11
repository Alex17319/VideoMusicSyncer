using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VideoMusicSyncer.FFmpegCommandBuilder
{
	public class FFmpegFilterOption
	{
		public const string NameValidatorRegex = @"^[^\=\:]*$";
		public const string ValueValidatorRegex = @"^[^\=\:]+$";

		public string Name { get; }
		/// <summary>
		/// When a list of values is used, the formatting may vary depending on the option.
		/// List items in an option's value are usually separated by '|', but this could have a different
		/// meaning in single-value or differently-formatted options.
		/// </summary>
		public string Value { get; }

		/// <param name="name">May be null or empty</param>
		public FFmpegFilterOption(string name, string value)
		{
			name = name ?? "";
			ErrorUtils.ThrowIfArgFailsMatch(name, NameValidatorRegex, nameof(name));
			ErrorUtils.ThrowIfArgFailsMatch(value, ValueValidatorRegex, nameof(value));
			
			this.Name = name;
			this.Value = value;
		}
		/// <param name="name">May be null or empty</param>
		public FFmpegFilterOption(string name, object value) : this(name, value?.ToString() ?? "") { }
		public FFmpegFilterOption(string value) : this("", value) { }
		public FFmpegFilterOption(object value) : this("", value?.ToString() ?? "") { }

		public override string ToString()
		{
			return (Name == "" ? Value : Name + "=" + Value);
		}
	}
}

//*/