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
	public class FFmpegEnableBetween : FFmpegFilterOption
	{
		public FFmpegEnableBetween(double startTime, double endTime)
			: base("enable", $"between(t, {startTime.ToString("F10")}, {endTime.ToString("F10")})")
			//Format values to avoid scientific notation, as FFmpeg can't parse that
		{ }

		public FFmpegEnableBetween(int startFrame, int endFrame)
			: base("enable", $"between(n, {startFrame}, {endFrame})")
		{ }
	}
}

//*/