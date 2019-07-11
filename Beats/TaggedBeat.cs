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

namespace VideoMusicSyncer.Beats
{
	public struct TaggedBeat
	{
		/// <summary>
		/// The fixed time at which the beat occurs, in beat-time
		/// (i.e. beats 0, 1, 2 are at time = 0, 1, 2) not audio time
		/// (i.e. where eg. beat 0 is at time = 12.34s)
		/// </summary>
		public double Beat { get; }

		private readonly IReadOnlyList<object> _tags;
		public IReadOnlyList<object> Tags => _tags ?? ImmutableList.Create<object>();

		public TaggedBeat(double beat, IReadOnlyList<object> tags)
		{
			tags = tags ?? ImmutableList.Create<object>();

			this.Beat = beat;
			this._tags = tags;
		}

		public override string ToString()
		{
			return (
				$"{{[{nameof(TaggedBeat)}] "
				+ $"{nameof(Beat)}: {Beat}, "
				+ $"{nameof(Tags)}: [{String.Join(", ", Tags)}]"
				+ "}}"
			);
		}
	}
}

//*/