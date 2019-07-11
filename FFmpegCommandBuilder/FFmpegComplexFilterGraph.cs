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
	public class FFmpegComplexFilterGraph : FFmpegFilterGraph
	{
		public ImmutableList<FFmpegFilterChain> FilterChains { get; }

		public override string Name => "filter_complex";
		public override string Value => "\"" + string.Join("; ", this.FilterChains) + "\"";

		public FFmpegComplexFilterGraph(ImmutableList<FFmpegFilterChain> filterChains)
		{
			ErrorUtils.ThrowIfArgNull(filterChains, nameof(filterChains));

			this.FilterChains = filterChains;
		}

		public FFmpegComplexFilterGraph(params FFmpegFilterChain[] filterChains)
			: this(ImmutableList.Create(filterChains))
		{ }
	}
}

//*/