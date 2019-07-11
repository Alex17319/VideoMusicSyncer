using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace VideoMusicSyncer
{
	public struct SyncedRange
	{
		public SyncPoint Start { get; }
		public SyncPoint End { get; }

		public double AudioDuration => End.AudioPos - Start.AudioPos;
		public double VideoDuration => End.VideoPos - Start.VideoPos;

		/// <summary>
		/// The factor by which the video should be sped up or slowed down by to match the duration of the audio.
		/// </summary>
		public double SyncedVidSpeed => VideoDuration / AudioDuration;

		public SyncedRange(SyncPoint start, SyncPoint end)
		{
			this.Start = start;
			this.End = end;
		}
	}
}

//*/