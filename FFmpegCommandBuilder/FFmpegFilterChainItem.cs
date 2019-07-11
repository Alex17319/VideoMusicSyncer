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
	public class FFmpegFilterChainItem
	{
		public ImmutableList<FFmpegPad> InputStreams { get; }
		public FFmpegFilter Filter { get; }
		public ImmutableList<FFmpegPad> OutputStreams { get; }

		public FFmpegFilterChainItem(ImmutableList<FFmpegPad> inputStreams, FFmpegFilter filter, ImmutableList<FFmpegPad> outputStreams)
		{
			ErrorUtils.ThrowIfArgNull(inputStreams, nameof(inputStreams));
			ErrorUtils.ThrowIfArgNull(filter, nameof(filter));
			ErrorUtils.ThrowIfArgNull(outputStreams, nameof(outputStreams));

			this.InputStreams = inputStreams;
			this.Filter = filter;
			this.OutputStreams = outputStreams;
		}
		public FFmpegFilterChainItem(ImmutableList<FFmpegPad> inputStreams, FFmpegFilter filter)
			: this(inputStreams, filter, ImmutableList.Create<FFmpegPad>())
		{ }
		public FFmpegFilterChainItem(FFmpegFilter filter, ImmutableList<FFmpegPad> outputStreams)
			: this(ImmutableList.Create<FFmpegPad>(), filter, outputStreams)
		{ }
		public FFmpegFilterChainItem(FFmpegFilter filter)
			: this(ImmutableList.Create<FFmpegPad>(), filter, ImmutableList.Create<FFmpegPad>())
		{ }

		public override string ToString()
		{
			return (
				string.Concat(InputStreams)
				+ Filter.ToString()
				+ string.Concat(OutputStreams)
			);
		}
	}
}

//*/