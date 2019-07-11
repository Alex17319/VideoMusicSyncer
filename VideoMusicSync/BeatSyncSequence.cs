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
using System.Linq;
using VideoMusicSyncer.Beats;

namespace VideoMusicSyncer
{
	/// <summary>
	/// Immutable
	/// <para/>
	/// See <see cref="BeatSequence"/> for more documentation on methods.
	/// </summary>
	public class BeatSyncSequence
	{
		//Note: These collections must always be the same length
		private BeatSequence _beatSequence { get; }
		private ImmutableList<double> _videoPositions { get; }

		public BeatPositioner BeatPositioner => _beatSequence.BeatPositioner;
		
		public IReadOnlyList<SyncPoint> SyncPoints { get; }

		public SyncPoint LastSync => SyncPoints[SyncPoints.Count - 1];

		public double LastSyncAudioTime => BeatPositioner.BeatToTime(_beatSequence.LastBeatTime);
		/// <summary>
		/// Time until the next beat after the current sequence.
		/// If zero, there is no next beat for this sequence.
		/// This uses the time in the audio clip (not the video time or beat-based time).
		/// </summary>
		public double NextSyncAudioOffset => BeatPositioner.BeatDeltaToTimeDelta(_beatSequence.NextBeatOffset);
		public double NextSyncAudioTime => LastSyncAudioTime + NextSyncAudioOffset;

		private BeatSyncSequence(BeatSequence beatSequence, ImmutableList<double> videoPositions)
		{
			if (beatSequence == null) throw new ArgumentNullException(nameof(beatSequence));
			if (videoPositions == null) throw new ArgumentNullException(nameof(videoPositions));

			this._beatSequence = beatSequence;
			this._videoPositions = videoPositions;

			this.SyncPoints = new BeatSyncSequence.SyncPointList(parent: this);
		}

		//Can't be stuffed validating that the syncpoint list is correct, and I only actually need to build it here anyway,
		//so just don't worry about allowing construction from existing lists
		//	public BeatSyncSequence(ImmutableList<SyncPoint> syncPoints, double beatInterval, int barLength = 4, double nextBeatOffset = 0)
		//	{
		//		if (syncPoints == null) throw new ArgumentNullException(nameof(syncPoints));
		//		if (syncPoints.Count < 1) throw new ArgumentException("Cannot be empty", nameof(syncPoints));
		//		if (nextBeatOffset < 0) throw new ArgumentOutOfRangeException(nameof(nextBeatOffset), nextBeatOffset, "Cannot be negative.");
		//	
		//		this.SyncPoints = syncPoints;
		//		this.BeatPositioner = new BeatPositioner(
		//			firstBeat: syncPoints[0].AudioPos,
		//			interval: beatInterval,
		//			barLength: barLength
		//		);
		//		this.NextSyncAudioOffset = nextBeatOffset;
		//	}

		public BeatSyncSequence(SyncPoint firstSyncPoint, double beatInterval, int barLength = 4, double nextBeatOffset = 0)
			: this(
				beatSequence: new BeatSequence(
					beatPositioner: new BeatPositioner(
						firstBeat: firstSyncPoint.AudioPos.Position,
						interval: beatInterval,
						barLength: barLength
					),
					nextBeatOffset: nextBeatOffset
				),
				videoPositions: ImmutableList.Create(firstSyncPoint.VideoPos.Position)
			)
		{ }


		private SyncPoint MakeSyncPoint(double beat, double vidPos) => new SyncPoint(
			videoPos: vidPos,
			audioPos: this._beatSequence.BeatPositioner.BeatToTime(beat)
		);


		public BeatSyncSequence SkipToBeat(Beat beat) => new BeatSyncSequence(
			_beatSequence.SkipToBeat(beat),
			_videoPositions
		);

		public BeatSyncSequence SkipByBeats(double beats) => new BeatSyncSequence(
			_beatSequence.SkipByBeats(beats),
			_videoPositions
		);
		public BeatSyncSequence SkipBySeconds(double seconds) => new BeatSyncSequence(
			_beatSequence.SkipBySeconds(seconds),
			_videoPositions
		);

		public BeatSyncSequence DeleteLastBeat() => new BeatSyncSequence(
			_beatSequence.DeleteLastBeat(),
			_videoPositions.RemoveAt(_videoPositions.Count - 1)
		);

		public BeatSyncSequence DeleteAllInLastBeatsRange(double beats)
		{
			var newBeatSeq = _beatSequence.DeleteAllInLastBeatsRange(beats);
			int numDeleted = _beatSequence.Beats.Count - newBeatSeq.Beats.Count;

			return new BeatSyncSequence(
				newBeatSeq,
				_videoPositions.RemoveRange(_videoPositions.Count - numDeleted, numDeleted)
			);
		}
		public BeatSyncSequence DeleteAllInLastSecondsRange(double seconds) => DeleteAllInLastBeatsRange(
			this.BeatPositioner.TimeDeltaToBeatDelta(seconds)
		);

		public BeatSyncSequence SoftRewindByBeats(double beats) => new BeatSyncSequence(
			_beatSequence.SoftRewindByBeats(beats),
			_videoPositions
		);
		public BeatSyncSequence SoftRewindBySeconds(double seconds) => new BeatSyncSequence(
			_beatSequence.SoftRewindBySeconds(seconds),
			_videoPositions
		);

		public BeatSyncSequence HardRewindByBeats(double beats) => new BeatSyncSequence(
			_beatSequence.HardRewindByBeats(beats),
			_videoPositions
		);
		public BeatSyncSequence HardRewindBySeconds(double seconds) => new BeatSyncSequence(
			_beatSequence.HardRewindBySeconds(seconds),
			_videoPositions
		);

		public BeatSyncSequence SyncWithVideo(double videoPos)
		{
			if (videoPos < LastSync.VideoPos) throw new ArgumentOutOfRangeException(
				nameof(videoPos),
				"The specified video position '" + videoPos + "' "
				+ "is lower than the last synced video position '" + LastSync.VideoPos + "'."
			);

			return new BeatSyncSequence(
				beatSequence: _beatSequence.AddBeat(),
				videoPositions: _videoPositions.Add(videoPos)
			);
		}


		private class SyncPointList : IReadOnlyList<SyncPoint>
		{
			private readonly BeatSyncSequence _parent;

			public SyncPointList(BeatSyncSequence parent) {
				this._parent = parent;
			}

			public int Count => _parent._beatSequence.Beats.Count;

			public IEnumerator<SyncPoint> GetEnumerator() => Enumerable.Zip(
				first: _parent._beatSequence.Beats,
				second: _parent._videoPositions,
				resultSelector: (beat, vidPos) => _parent.MakeSyncPoint(beat: beat, vidPos: vidPos)
			).GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

			public SyncPoint this[int index] => _parent.MakeSyncPoint(
				beat: _parent._beatSequence.Beats[index],
				vidPos: _parent._videoPositions[index]
			);
		}

		

		//	public BeatSyncSequence SkipToBeat(Beat beat)
		//	{
		//		var nextBeatTime = this.BeatPositioner.NextBeatToTime(
		//			currentTime: NextSyncAudioTime,
		//			nextBeat: beat
		//		);
		//	
		//		if (nextBeatTime <= NextSyncAudioTime) throw new ArgumentException(
		//			"Next beat '" + beat + "' is before or at the same time as the currently planned next sync "
		//			+ "(at audio time '" + NextSyncAudioTime + "', "
		//			+ "with the last sync at audio time '" + CurrentAudioTime + "')"
		//		);
		//	
		//		return new BeatSyncSequence(
		//			this.SyncPoints,
		//			this.BeatPositioner,
		//			nextBeatOffset: nextBeatTime - CurrentAudioTime //Make sure not to negate the current offset (did that oops)
		//		);
		//	}
		//	
		//	public BeatSyncSequence SkipBy(double seconds)
		//	{
		//		if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Must be greater than zero.");
		//	
		//		return new BeatSyncSequence(
		//			this.SyncPoints,
		//			this.BeatPositioner,
		//			nextBeatOffset: NextSyncAudioOffset + seconds
		//		);
		//	}
		//	
		//	public BeatSyncSequence SkipByBeats(double beats)
		//	{
		//		if (beats <= 0) throw new ArgumentOutOfRangeException(nameof(beats), beats, "Must be greater than zero.");
		//	
		//		return SkipBy(seconds: this.BeatPositioner.BeatDeltaToTimeDelta(beats));
		//	}
		//	
		//	public BeatSyncSequence RewindBy(double seconds)
		//	{
		//		if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Must be greater than zero.");
		//	
		//		int lastIndex = this.SyncPoints.Count - 1;
		//		SyncPoint last = this.SyncPoints[lastIndex];
		//	
		//		return new BeatSyncSequence(
		//			this.SyncPoints.RemoveAt(lastIndex),
		//			this.BeatPositioner,
		//			nextBeatOffset: (last.AudioPos - seconds) + NextSyncAudioOffset
		//		);
		//	}
		//	
		//	public BeatSyncSequence RewindByBeats(double beats)
		//	{
		//		if (beats <= 0) throw new ArgumentOutOfRangeException(nameof(beats), beats, "Must be greater than zero.");
		//	
		//		return RewindBy(seconds: this.BeatPositioner.BeatDeltaToTimeDelta(beats));
		//	}
		//	
		//	public BeatSyncSequence SyncWithVideo(double videoPos)
		//	{
		//		if (this.NextSyncAudioOffset == 0) throw new InvalidOperationException(
		//			"Cannot sync with the video without first skipping forward through the audio "
		//			+ "(" + nameof(NextSyncAudioOffset) + " is zero). "
		//			+ "Please call " + nameof(SkipToBeat) + " or " + nameof(SkipBy) + " first."
		//		);
		//		if (videoPos < LastSync.VideoPos) throw new ArgumentOutOfRangeException(
		//			nameof(videoPos),
		//			"The specified video position '" + videoPos + "' "
		//			+ "is lower than the last synced video position '" + LastSync.VideoPos + "'."
		//		);
		//	
		//		return new BeatSyncSequence(
		//			syncPoints: this.SyncPoints.Add(
		//				new SyncPoint(
		//					videoPos: videoPos,
		//					audioPos: CurrentAudioTime + NextSyncAudioOffset
		//				)
		//			),
		//			beatPositioner: this.BeatPositioner,
		//			nextBeatOffset: 0
		//		);
		//	}
	}
}

//*/