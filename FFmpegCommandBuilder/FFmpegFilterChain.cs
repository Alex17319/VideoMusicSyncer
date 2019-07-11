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
	public class FFmpegFilterChain
	{
		public ImmutableList<FFmpegFilterChainItem> ChainItems { get; }

		public FFmpegFilterChain(ImmutableList<FFmpegFilterChainItem> chainItems)
		{
			ErrorUtils.ThrowIfArgNull(chainItems, nameof(chainItems));

			this.ChainItems = chainItems;
		}

		public FFmpegFilterChain(FFmpegFilterChainItem chainItem)
			: this(ImmutableList.Create(chainItem))
		{ }
		public FFmpegFilterChain(IEnumerable<FFmpegFilterChainItem> chainItems)
			: this(chainItems?.ToImmutableList() ?? ImmutableList.Create<FFmpegFilterChainItem>())
		{ }
		public FFmpegFilterChain(params FFmpegFilterChainItem[] chainItems)
			: this(chainItems.AsEnumerable())
		{ }

		public override string ToString()
		{
			return string.Join(", ", ChainItems);
		}
	}
}

//*/