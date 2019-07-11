using System;
using System.Collections;
using System.Collections.Generic;
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
	public abstract class FFmpegFilterGraph : FFmpegOption
	{
		public abstract override string Name { get; }
		public abstract override string Value { get; }
	}
}

//*/