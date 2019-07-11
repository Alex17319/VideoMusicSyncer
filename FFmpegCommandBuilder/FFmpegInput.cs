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
using System.Threading;
using System.Threading.Tasks;

namespace VideoMusicSyncer.FFmpegCommandBuilder
{
	public class FFmpegInput
	{
		public FileInfo File { get; }
		public ImmutableList<FFmpegOption> Modifiers { get; }

		public FFmpegInput(FileInfo file, ImmutableList<FFmpegOption> modifiers = null)
		{
			ErrorUtils.ThrowIfArgNull(file, nameof(file));

			this.File = file;
			this.Modifiers = modifiers ?? ImmutableList.Create<FFmpegOption>();
		}

		public override string ToString()
		{
			return (Modifiers.Count == 0 ? "" : string.Join(" ", Modifiers) + " ") + $"-i \"{File.FullName}\"";
		}
	}
}

//*/