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
	public class FFmpegFilter
	{
		public const string NameValidatorRegex = @"^[0-9a-zA-Z_]+$";
		public const string IdValidatorRegex = @"^[0-9a-zA-Z_]*$";

		public string Name { get; }
		/// <summary>
		/// Note: All characters except '=' should be (if I've escaped things correctly) valid in the option names and values
		/// </summary>
		public ImmutableList<FFmpegFilterOption> Options { get; }
		public string ID { get; }

		public FFmpegFilter(string name, ImmutableList<FFmpegFilterOption> options = null, string id = null)
		{
			options = options ?? ImmutableList.Create<FFmpegFilterOption>();
			id = id ?? "";

			ErrorUtils.ThrowIfArgFailsMatch(name, NameValidatorRegex, nameof(name));
			ErrorUtils.ThrowIfArgFailsMatch(id, IdValidatorRegex, nameof(id));
			for (int i = 0; i < options.Count; i++) {
				ErrorUtils.ThrowIfArgNull(options[i], $"{nameof(options)}[{i}]");
			} //TODO: Validate nulls in other collections in this project like this

			this.Name = name;
			this.Options = options;
			this.ID = id;
		}

		public FFmpegFilter(string name, params FFmpegFilterOption[] options)
			: this(name, ImmutableList.Create(options), id: null)
		{ }

		public override string ToString()
		{
			return (
				this.Name
				+ (string.IsNullOrEmpty(this.ID) ? null : "@" + ID)
				+ (
					this.Options.Count == 0
					? ""
					: (
						"="
						+ "'"
						+ string.Join(":", Options).Replace(@"\", @"\\").Replace(@"'", @"\'")
						+ "'"
					)
				)
			);
		}

		public FFmpegFilter WithChangedOptions(
			IEnumerable<FFmpegFilterOption> options
		) {
			if (options == null) return new FFmpegFilter(this.Name, this.Options, this.ID);

			return new FFmpegFilter(
				this.Name,
				this.Options
				.RemoveAll(x => !options.Any(y => y.Name == x.Name))
				.AddRange(options),
				this.ID
			);
		}

		public FFmpegFilter WithChangedOptions(params FFmpegFilterOption[] options)
			=> WithChangedOptions(options.AsEnumerable());
	}
}

//*/