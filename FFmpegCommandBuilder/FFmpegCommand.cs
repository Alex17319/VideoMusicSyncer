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
	public class FFmpegCommand
	{
		public ImmutableList<FFmpegInput> Inputs { get; }
		public FFmpegFilterGraph FilterGraph { get; }
		public ImmutableList<FFmpegOption> OtherMidOptions { get; }
		public ImmutableList<FFmpegOutput> Outputs { get; }
		public ImmutableList<FFmpegOption> OtherFinalOptions { get; }


		public FFmpegCommand(
			ImmutableList<FFmpegInput> inputs,
			FFmpegFilterGraph filterGraph,
			ImmutableList<FFmpegOption> otherMidOptions,
			ImmutableList<FFmpegOutput> outputs,
			ImmutableList<FFmpegOption> otherFinalOptions
		) {
			this.Inputs = inputs ?? ImmutableList.Create<FFmpegInput>();
			this.FilterGraph = filterGraph;
			this.OtherMidOptions = otherMidOptions ?? ImmutableList.Create<FFmpegOption>();
			this.Outputs = outputs ?? ImmutableList.Create<FFmpegOutput>();
			this.OtherFinalOptions = otherFinalOptions ?? ImmutableList.Create<FFmpegOption>();
		}

		public override string ToString()
		{
			return StringUtils.JoinNonNull(
				" ",
				"ffmpeg",
				Inputs.Count == 0 ? null : string.Join(" ", Inputs),
				FilterGraph,
				OtherMidOptions.Count == 0 ? null : string.Join(" ", OtherMidOptions),
				Outputs.Count == 0 ? null : string.Join(" ", Outputs),
				OtherFinalOptions.Count == 0 ? null : string.Join(" ", OtherFinalOptions)
			);
		}
	}
}

//*/