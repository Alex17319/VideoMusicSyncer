using System;
using System.Collections;
using System.Collections.Generic;
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
	public class FFmpegPad
	{
		public const string NameValidatorRegex = @"^[0-9a-zA-Z_\:]+$";
		public static FFmpegPad Default { get; } = new FFmpegPad();

		public string StreamName { get; }

		private FFmpegPad() {
			this.StreamName = "";
		}

		public FFmpegPad(string streamName)
		{
			ErrorUtils.ThrowIfArgFailsMatch(streamName, NameValidatorRegex, nameof(streamName));

			this.StreamName = streamName;
		}

		public override string ToString()
		{
			return StreamName != "" ? "[" + StreamName + "]" : "";
		}
	}
}

//*/