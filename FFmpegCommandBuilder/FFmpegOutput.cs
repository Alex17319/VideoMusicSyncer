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
	public class FFmpegOutput
	{
		/// <summary>Compressed, but losslessly. Does not include the '.' before the extension.</summary>
		public static string LosslessOutputExtension => "AVI";
		/// <summary>Compressed, but losslessly.</summary>
		public static ImmutableList<FFmpegOption> LosslessOutputOptions { get; } = ImmutableList.Create<FFmpegOption>(
			new FFmpegOption("vcodec", "ffv1 -level 3")
		);

		public FileInfo File { get; }
		public ImmutableList<FFmpegOption> Modifiers { get; }

		public FFmpegOutput(FileInfo file, ImmutableList<FFmpegOption> modifiers = null)
		{
			this.File = file;
			this.Modifiers = modifiers ?? ImmutableList.Create<FFmpegOption>();
		}

		public override string ToString()
		{
			return (Modifiers.Count == 0 ? "" : string.Join(" ", Modifiers) + " ") + $"\"{File}\"";
		}
	}
}

//*/