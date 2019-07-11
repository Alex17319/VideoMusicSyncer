using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using VideoMusicSyncer.Beats;

namespace VideoMusicSyncer
{
	public struct SyncPoint
	{
		public MediaPos VideoPos { get; }
		public MediaPos AudioPos { get; }

		public static SyncPoint Start => new SyncPoint(MediaPos.Start, MediaPos.Start);
		public static SyncPoint End   => new SyncPoint(MediaPos.End, MediaPos.End);

		public SyncPoint(MediaPos videoPos, MediaPos audioPos)
		{
			this.VideoPos = videoPos;
			this.AudioPos = audioPos;
		}

		public SyncPoint(double videoPos, double audioPos)
			: this((MediaPos)videoPos, (MediaPos)audioPos)
		{ }

		public static bool Equals(SyncPoint a, SyncPoint b) => a.VideoPos == b.VideoPos && a.AudioPos == b.AudioPos;
		public override bool Equals(object obj) => obj is SyncPoint s ? Equals(this, s) : false;
		public static bool operator ==(SyncPoint a, SyncPoint b) => Equals(a, b);
		public static bool operator !=(SyncPoint a, SyncPoint b) => !(a == b);
		public override int GetHashCode() {
			unchecked {
				int hash = 17;
				hash = hash * 23 + this.VideoPos.GetHashCode();
				hash = hash * 23 + this.AudioPos.GetHashCode();
				return hash;
			}
		}

		public override string ToString() {
			return $"{{[{nameof(SyncPoint)}] {nameof(VideoPos)}: {VideoPos}, {nameof(AudioPos)}: {AudioPos}}}";
		}
	}
}

//*/