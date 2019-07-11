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

namespace VideoMusicSyncer.Beats
{
	/// <summary>
	/// Immutable
	/// </summary>
	public class BeatSequence
	{
		/// <summary>
		/// Note: This uses beat time (i.e beats 0, 1, 2 are at time = 0, 1, 2) not audio time (i.e. beat 0 is at 12.34s).
		/// </summary>
		public ImmutableList<double> Beats { get; }
		public double LastBeatTime => Beats[Beats.Count - 1];

		public BeatPositioner BeatPositioner { get; }
		/// <summary>
		/// Time until the next beat after the current sequence.
		/// If zero, there is no beat after this sequence.
		/// This uses the beat time, not the time in the audio clip.
		/// </summary>
		public double NextBeatOffset { get; }
		public double NextBeatTime => LastBeatTime + NextBeatOffset;

		private BeatSequence(ImmutableList<double> beats, BeatPositioner beatPositioner, double nextBeatOffset)
		{
			if (beats == null) throw new ArgumentNullException(nameof(beats));
			if (beats.Count == 0) throw new ArgumentException("Cannot be empty", nameof(beats));
			if (beatPositioner == null) throw new ArgumentNullException(nameof(beatPositioner));

			this.BeatPositioner = beatPositioner;
			this.NextBeatOffset = nextBeatOffset;

			if (beats == null) throw new ArgumentNullException(nameof(beats));
			if (beats.Count < 1) throw new ArgumentException("Cannot be empty", nameof(beats));

			this.Beats = beats;
		}

		public BeatSequence(BeatPositioner beatPositioner, double nextBeatOffset = 0)
			: this(
				beats: ImmutableList.Create<double>(0), // first beat is always at beat-time = 0
				beatPositioner: beatPositioner,
				nextBeatOffset: nextBeatOffset
			)
		{ }

		/// <summary>
		/// Adds a beat at the current offset, and sets the offset to zero.
		/// Use the skip and rewind methods to modify the offset first,
		/// as beats cannot be added with an offset of zero.
		/// </summary>
		public BeatSequence AddBeat()
		{
			if (this.NextBeatOffset == 0) throw new InvalidOperationException(
				"Cannot add a beat without first skipping forward "
				+ "(" + nameof(NextBeatOffset) + " is zero). "
				+ "Please call one of the skip methods first."
			);

			return new BeatSequence(
				beats: Beats.Add(this.LastBeatTime + this.NextBeatOffset),
				beatPositioner: this.BeatPositioner,
				nextBeatOffset: 0
			);
		}

		public BeatSequence SkipToBeat(Beat beat)
		{
			var nextBeatTime = beat.GetFixedBeat(prevBeat: NextBeatTime, barLength: BeatPositioner.BarLength);

			if (nextBeatTime <= NextBeatTime) throw new ArgumentException(
				"Next beat '" + beat + "' is before or at the same time as the currently planned next beat "
				+ "(at audio time '" + NextBeatTime + "', "
				+ "with the last beat at audio time '" + LastBeatTime + "')"
			);

			return new BeatSequence(
				this.Beats,
				this.BeatPositioner,
				nextBeatOffset: nextBeatTime - LastBeatTime //Note: Make sure not to negate the current offset (I did that, oops)
			);
		}

		public BeatSequence SkipByBeats(double beats)
		{
			if (beats <= 0) throw new ArgumentOutOfRangeException(nameof(beats), beats, "Must be greater than zero.");

			return new BeatSequence(
				this.Beats,
				this.BeatPositioner,
				nextBeatOffset: NextBeatOffset + beats
			);
		}
		public BeatSequence SkipBySeconds(double seconds) {
			if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Must be greater than zero.");
			return SkipByBeats(BeatPositioner.TimeDeltaToBeatDelta(seconds));
		}

		/// <summary>
		/// Deletes the last beat in <see cref="Beats"/>, and adjusts <see cref="NextBeatOffset"/>
		/// so that <see cref="NextBeatTime"/> remains constant.
		/// </summary>
		public BeatSequence DeleteLastBeat()
		{
			int lastIndex = this.Beats.Count - 1;
			double secondLastToLastDelta = this.Beats[lastIndex] - this.Beats[lastIndex - 1];

			return new BeatSequence(
				this.Beats.RemoveAt(lastIndex),
				this.BeatPositioner,
				nextBeatOffset: secondLastToLastDelta + NextBeatOffset
			);
		}

		/// <summary>
		/// Deletes all beats after (<see cref="NextBeatTime"/> - <paramref name="beats"/>).
		/// Adjusts <see cref="NextBeatOffset"/> so that <see cref="NextBeatTime"/> remains constant.
		/// </summary>
		public BeatSequence DeleteAllInLastBeatsRange(double beats)
		{
			if (beats <= 0) throw new ArgumentOutOfRangeException(nameof(beats), beats, "Must be greater than zero.");

			int newLastBeatIndex = this.Beats.FindLastIndex(
				x => x <= NextBeatTime - beats
			);
			double nextBeatTime = this.NextBeatTime;

			return new BeatSequence(
				beats: this.Beats.RemoveRange(newLastBeatIndex + 1, this.Beats.Count - (newLastBeatIndex + 1)),
				beatPositioner: this.BeatPositioner,
				nextBeatOffset: nextBeatTime - this.Beats[newLastBeatIndex]
			);
		}
		public BeatSequence DeleteAllInLastSecondsRange(double seconds) {
			if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Must be greater than zero.");
			return DeleteAllInLastBeatsRange(BeatPositioner.TimeDeltaToBeatDelta(seconds));
		}

		/// <summary>
		/// Rewinds by the specified time (in beat time, not audio time),
		/// throwing an error if this requires previous beats to be deleted - 
		/// only <see cref="NextBeatOffset"/> can be modified by this method.
		/// </summary>
		public BeatSequence SoftRewindByBeats(double beats)
		{
			if (beats <= 0) throw new ArgumentOutOfRangeException(nameof(beats), beats, "Must be greater than zero.");

			if (beats > this.NextBeatOffset) throw new ArgumentOutOfRangeException(
				nameof(beats),
				beats,
				"Cannot be greater than the current " + nameof(NextBeatOffset) + " "
				+ "(equal to '" + this.NextBeatOffset + "') "
				+ " as soft rewinding does not allow previous beats to be deleted."
			);

			return new BeatSequence(
				beats: this.Beats,
				beatPositioner: this.BeatPositioner,
				nextBeatOffset: this.NextBeatOffset - beats
			);
		}
		public BeatSequence SoftRewindBySeconds(double seconds) {
			if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Must be greater than zero.");
			return SoftRewindByBeats(BeatPositioner.TimeDeltaToBeatDelta(seconds));
		}

		/// <summary>
		/// Rewinds by the specified time (in beat time, not audio time),
		/// deleting any previous beats as necessary (and throwing an error
		/// if the rewind would go beyond the start of the audio).
		/// </summary>
		public BeatSequence HardRewindByBeats(double beats)
		{
			if (beats <= 0) throw new ArgumentOutOfRangeException(nameof(beats), beats, "Must be greater than zero.");

			if (beats > this.NextBeatTime) throw new ArgumentOutOfRangeException(
				nameof(beats),
				beats,
				"Cannot be greater than the current " + nameof(NextBeatTime) + " "
				+ "(equal to '" + this.NextBeatTime + "') "
				+ " as that would mean rewinding before the start of the audio."
			);

			BeatSequence beatsDeleted;
			if (beats > this.NextBeatOffset) beatsDeleted = this.DeleteAllInLastBeatsRange(beats);
			else beatsDeleted = this;

			return beatsDeleted.SoftRewindByBeats(beats);
		}
		public BeatSequence HardRewindBySeconds(double seconds) {
			if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Must be greater than zero.");
			return HardRewindByBeats(BeatPositioner.TimeDeltaToBeatDelta(seconds));
		}
	}

	//	public abstract class BeatSequenceBase
	//	{
	//		public BeatPositioner BeatPositioner { get; }
	//	
	//		public abstract double CurrentAudioTime { get; }
	//		/// <summary>
	//		/// Time until the next beat after the current sequence.
	//		/// If zero, there is no next beat for this sequence.
	//		/// This uses the time in the audio clip.
	//		/// </summary>
	//		public double NextBeatAudioOffset { get; }
	//		public double NextBeatAudioTime => CurrentAudioTime + NextBeatAudioOffset;
	//	
	//		protected BeatSequenceBase(BeatPositioner beatPositioner, double nextBeatAudioOffset)
	//		{
	//			if (beatPositioner == null) throw new ArgumentNullException(nameof(beatPositioner));
	//	
	//			this.BeatPositioner = beatPositioner;
	//			this.NextBeatAudioOffset = nextBeatAudioOffset;
	//		}
	//	}
	//	
	//	/// <summary>
	//	/// Immutable
	//	/// </summary>
	//	public abstract class BeatSequence<TBeat> : BeatSequenceBase
	//	{
	//		public ImmutableList<TBeat> Beats { get; }
	//		public TBeat LastSetBeat => Beats[Beats.Count - 1];
	//	
	//		public 
	//	
	//		public sealed override double CurrentAudioTime => LastSetBeat.AudioPos;
	//	
	//		protected BeatSequence(ImmutableList<TBeat> beats, BeatPositioner beatPositioner, double nextBeatAudioOffset)
	//			: base(beatPositioner, nextBeatAudioOffset)
	//		{
	//			if (beats == null) throw new ArgumentNullException(nameof(beats));
	//			if (beats.Count < 1) throw new ArgumentException("Cannot be empty", nameof(beats));
	//	
	//			this.Beats = beats;
	//		}
	//	
	//	
	//	}
}

//*/