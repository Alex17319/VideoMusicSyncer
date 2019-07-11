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
	/// <summary>
	/// Immutable
	/// <para/>
	/// See <see cref="BeatSequence"/> for more documentation on methods.
	/// </summary>
	public class BeatTagSequence
	{
		//Note: These collections must always be the same length
		private BeatSequence _beatSequence { get; }
		private ImmutableList<ImmutableList<object>> _tagLists { get; }

		public BeatPositioner BeatPositioner => _beatSequence.BeatPositioner;

		public IReadOnlyList<TaggedBeat> TaggedBeats { get; }

		public TaggedBeat LastBeat => TaggedBeats[TaggedBeats.Count - 1];

		public double LastBeatTime => _beatSequence.LastBeatTime;
		/// <summary>
		/// Time until the next beat after the current sequence.
		/// If zero, there is no next beat for this sequence.
		/// This uses the time in the audio clip (not the video time or beat-based time).
		/// </summary>
		public double NextBeatOffset => _beatSequence.NextBeatOffset;
		public double NextBeatTime => LastBeatTime + NextBeatOffset;

		private BeatTagSequence(BeatSequence beatSequence, ImmutableList<ImmutableList<object>> tagLists)
		{
			if (beatSequence == null) throw new ArgumentNullException(nameof(beatSequence));
			if (tagLists == null) throw new ArgumentNullException(nameof(tagLists));

			this._beatSequence = beatSequence;
			this._tagLists = tagLists;

			this.TaggedBeats = new BeatTagSequence.TaggedBeatList(parent: this);
		}

		public BeatTagSequence(BeatPositioner beatPositioner, ImmutableList<object> firstBeatTags, double nextBeatOffset = 0)
			: this(
				beatSequence: new BeatSequence(
					beatPositioner: beatPositioner,
					nextBeatOffset: nextBeatOffset
				),
				tagLists: ImmutableList.Create(
					item: firstBeatTags ?? ImmutableList.Create<object>()
				)
			)
		{ }


		public BeatTagSequence SkipToBeat(Beat beat) => new BeatTagSequence(
			_beatSequence.SkipToBeat(beat),
			_tagLists
		);

		public BeatTagSequence SkipByBeats(double beats) => new BeatTagSequence(
			_beatSequence.SkipByBeats(beats),
			_tagLists
		);
		public BeatTagSequence SkipBySeconds(double seconds) => new BeatTagSequence(
			_beatSequence.SkipBySeconds(seconds),
			_tagLists
		);

		public BeatTagSequence DeleteLastBeat() => new BeatTagSequence(
			_beatSequence.DeleteLastBeat(),
			_tagLists.RemoveAt(_tagLists.Count - 1)
		);

		public BeatTagSequence DeleteAllInLastBeatsRange(double beats)
		{
			var newBeatSeq = _beatSequence.DeleteAllInLastBeatsRange(beats);
			int numDeleted = _beatSequence.Beats.Count - newBeatSeq.Beats.Count;

			return new BeatTagSequence(
				newBeatSeq,
				_tagLists.RemoveRange(_tagLists.Count - numDeleted, numDeleted)
			);
		}
		public BeatTagSequence DeleteAllInLastSecondsRange(double seconds) => DeleteAllInLastBeatsRange(
			this.BeatPositioner.TimeDeltaToBeatDelta(seconds)
		);

		public BeatTagSequence SoftRewindByBeats(double beats) => new BeatTagSequence(
			_beatSequence.SoftRewindByBeats(beats),
			_tagLists
		);
		public BeatTagSequence SoftRewindBySeconds(double seconds) => new BeatTagSequence(
			_beatSequence.SoftRewindBySeconds(seconds),
			_tagLists
		);

		public BeatTagSequence HardRewindByBeats(double beats) => new BeatTagSequence(
			_beatSequence.HardRewindByBeats(beats),
			_tagLists
		);
		public BeatTagSequence HardRewindBySeconds(double seconds) => new BeatTagSequence(
			_beatSequence.HardRewindBySeconds(seconds),
			_tagLists
		);

		public BeatTagSequence TagAndAdd(ImmutableList<object> tags = null)
		{
			tags = tags ?? ImmutableList.Create<object>();

			return new BeatTagSequence(
				beatSequence: _beatSequence.AddBeat(),
				tagLists: _tagLists.Add(tags)
			);
		}
		public BeatTagSequence TagAndAdd(IEnumerable<object> tags) => TagAndAdd(tags?.ToImmutableList());
		public BeatTagSequence TagAndAdd(params object[] tags) => TagAndAdd(tags != null ? ImmutableList.Create(tags) : null);

		private class TaggedBeatList : IReadOnlyList<TaggedBeat>
		{
			private readonly BeatTagSequence _parent;

			public TaggedBeatList(BeatTagSequence parent) {
				this._parent = parent;
			}

			public int Count => _parent._beatSequence.Beats.Count;

			public IEnumerator<TaggedBeat> GetEnumerator() => Enumerable.Zip(
				first: _parent._beatSequence.Beats,
				second: _parent._tagLists,
				resultSelector: (beat, tags) => new TaggedBeat(beat, tags)
			).GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

			public TaggedBeat this[int index] => new TaggedBeat(
				beat: _parent._beatSequence.Beats[index],
				tags: _parent._tagLists[index]
			);
		}
	}
}

//*/