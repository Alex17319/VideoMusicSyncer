using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace VideoMusicSyncer
{
	/// <summary>
	/// A collection of at least 2 <see cref="SyncPoint"/>s, to be used by <see cref="VideoMusicSync"/>.
	/// The <see cref="SyncPoint"/>s must be in ascending order, sorted by both
	/// <see cref="SyncPoint.VideoPos"/> and <see cref="SyncPoint.AudioPos"/>.
	/// <para/>
	/// Only the content of the video and audio between the first and last <see cref="SyncPoint"/>s will be
	/// included - to include the whole video or audio, use the start and end positions as <see cref="SyncPoint"/>s (see <see cref="MediaPos.Start"/> and <see cref="MediaPos.End"/>)
	/// </summary>
	public class SyncPointCollection : IReadOnlyList<SyncPoint>
	{
		private readonly IImmutableList<SyncPoint> _list;

		public SyncPoint First => _list[0];
		public SyncPoint Last => _list[_list.Count - 1];

		public double CroppedVideoStartTime => First.VideoPos;
		public double CroppedAudioStartTime => First.AudioPos;
		public double CroppedVideoEndTime   => Last .VideoPos;
		public double CroppedAudioEndTime   => Last .AudioPos;
		public double CroppedVideoDuration  => CroppedVideoEndTime - CroppedVideoStartTime;
		public double CroppedAudioDuration  => CroppedAudioEndTime - CroppedAudioStartTime;

		public SyncPointCollection(IImmutableList<SyncPoint> syncPoints)
		{
			ValidateSyncPointList(syncPoints);

			this._list = syncPoints;
		}

		public int Count => this._list.Count;
		public SyncPoint this[int index] => this._list[index];
		public IEnumerator<SyncPoint> GetEnumerator() => this._list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this._list.GetEnumerator();

		private static void ValidateSyncPointList(IImmutableList<SyncPoint> syncPoints)
		{
			if (syncPoints == null) throw new ArgumentNullException(nameof(syncPoints));
			if (syncPoints.Count < 2) throw new ArgumentException("Must have at least 2 elements", nameof(syncPoints));

			SyncPoint prev = SyncPoint.Start;
			for (int i = 0; i < syncPoints.Count; i++)
			{
				var current = syncPoints[i];

				var videoBackwards = current.VideoPos < prev.VideoPos;
				var audioBackwards = current.AudioPos < prev.AudioPos;
				if (videoBackwards || audioBackwards)
				{
					throw new ArgumentException(
						$"Element at index '{i}' has a lower "
						+ (
							videoBackwards
								? audioBackwards
									? (nameof(SyncPoint.VideoPos) + " and " + nameof(SyncPoint.AudioPos))
									: nameof(SyncPoint.VideoPos)
								: nameof(SyncPoint.AudioPos)
						)
						+ "than the previous element. "
						+ $"The SyncPoints list must be ordered so that both the {nameof(SyncPoint.VideoPos)} "
						+ $"and {nameof(SyncPoint.AudioPos)} are sorted in ascending order. "
						+ $"Current element: {current}, prev element: {prev}.",
						nameof(syncPoints)
					);
				}

				prev = current;
			}
		}

		public IEnumerable<SyncedRange> GetSyncedRanges()
		{
			var prev = this.First;
			for (int i = 1 /*skip first*/; i < this.Count; i++)
			{
				var current = this[i];
				if (current != prev) //Ignore exact duplicates
				{
					yield return new SyncedRange(start: prev, end: current);
				}
				prev = current;
			}
		}
	}
}

//*/