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
using System.Threading.Tasks;
using VideoMusicSyncer.Beats;

namespace VideoMusicSyncer.VideoGlowOverlay
{
	/// <summary>
	/// A series of beats where the glows attached to each beat overlap only with each other (if at all),
	/// not other glows before or after
	/// </summary>
	public struct IndependantGlowSeries
	{
		private ImmutableList<TaggedBeat> _beats;
		public ImmutableList<TaggedBeat> Beats => _beats ?? (_beats = ImmutableList.Create<TaggedBeat>());
		public double GlowStartTime { get; }
		public double GlowEndTime { get; }

		public bool IsEmpty => Beats.IsEmpty;

		public static IndependantGlowSeries Empty { get; } = new IndependantGlowSeries();

		public IndependantGlowSeries(ImmutableList<TaggedBeat> beats, BeatPositioner beatPositioner)
			: this(
				beats: ErrorUtils.ThrowIfArgNull(beats, nameof(beats)),
				glowStartTime: beats.Min(
					b => b.Tags.OfType<Glow>().Min(
						g => beatPositioner.BeatToTime(b.Beat) - g.FadeInTime
					)
				),
				glowEndTime: beats.Max(
					b => b.Tags.OfType<Glow>().Max(
						g => beatPositioner.BeatToTime(b.Beat) + g.FadeOutTime
					)
				)
			)
		{ }

		private IndependantGlowSeries(ImmutableList<TaggedBeat> beats, double glowStartTime, double glowEndTime)
		{
			ErrorUtils.ThrowIfArgNull(beats, nameof(beats));

			this._beats = beats;
			this.GlowEndTime = glowEndTime;
			this.GlowStartTime = glowStartTime;
		}

		public override string ToString()
		{
			return "{[" + nameof(IndependantGlowSeries) + "] " + (
				nameof(GlowStartTime) + ": " + GlowStartTime + ", " +
				nameof(GlowEndTime) + ": " + GlowEndTime + ", " +
				nameof(Beats) + ": " + (
					"[" + string.Join(", ", Beats) + "]"
				)
			) + "}";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="beats">Does not need to be sorted</param>
		/// <param name="beatPositioner"></param>
		/// <param name="targetSize"></param>
		/// <returns>
		/// Note: Will be sorted by <see cref="GlowStartTime"/> (and therefore
		/// also <see cref="GlowEndTime"/>, as they can't overlap)
		/// </returns>
		public static List<IndependantGlowSeries> Split(IEnumerable<TaggedBeat> beats, BeatPositioner beatPositioner, int targetSize)
		{
			if (beats == null) throw new ArgumentNullException(nameof(beats));
			if (beatPositioner == null) throw new ArgumentNullException(nameof(beatPositioner));
			if (beats == null) throw new ArgumentNullException(nameof(beats));

			var glowSeriesList = new List<IndependantGlowSeries>();

			foreach (var beat in beats)
			{
				double beatTime = beatPositioner.BeatToTime(beat.Beat);

				//	var glowEndTime = beats[i].Tags.OfType<Glow>().Max(g => beatTime + g.FadeOutTime);

				var glows = beat.Tags.OfType<Glow>();

				var glowStartTime = beatTime - glows.Max(g => g.FadeInTime);
				var glowEndTime = beatTime + glows.Max(g => g.FadeOutTime);

				// - Loop through all existing glow series.
				// - For the first one that overlaps with the current beat,
				//   add the current beat to it (and expand the glow start/end times as appropriate)
				// - For each subsequent one that overlaps, add it to the first
				//   one that overlapped (and expand the glow start/end times as appropriate)
				//     - Then set the index the subsquent overlapping series was at to Empty (equiv. of null)
				// - After the looping, remove all Empty entries from the glow series list
				// - Finally, if the beat did not overlap with any existing glow series,
				//   add a new glow series with just that beat.
				// - This design allows any IEnumerable to be used. In contrast, a design that looked ahead
				//   through the subsequent elements to check if they overlapped would only work with an
				//   IList (as it is not advisable to enumerate through an IEnumerable multiple times, as
				//   it could be performing some complex query or taking data from a database for all we know)
				// - This should (hopefully) be less than O(n^2) when there's a decent amount of overlapping
				//   (the null-entry-removal step might complicate it, idk)
				int firstOverlapIndex = -1;
				for (int gsIndex = 0; gsIndex < glowSeriesList.Count; gsIndex++)
				{
					bool overlaps = (
						glowSeriesList[gsIndex].GlowEndTime >= glowStartTime
						&& glowSeriesList[gsIndex].GlowStartTime <= glowEndTime
					);

					if (overlaps)
					{
						if (firstOverlapIndex < 0)
						{
							firstOverlapIndex = gsIndex;
							glowSeriesList[gsIndex] = glowSeriesList[gsIndex].AddAndExpand(
								beats: new[] { beat },
								glowStartTime: glowStartTime,
								glowEndTime: glowEndTime
							);
						}
						else
						{
							glowSeriesList[firstOverlapIndex] = glowSeriesList[firstOverlapIndex].AddAndExpand(
								beats: glowSeriesList[gsIndex].Beats,
								glowStartTime: glowSeriesList[gsIndex].GlowStartTime,
								glowEndTime: glowSeriesList[gsIndex].GlowStartTime
							);
							glowSeriesList[gsIndex] = Empty;
						}
					}
				}

				RemoveAllUnordered(glowSeriesList, x => x.IsEmpty);
				// ^ Do this each outer loop to avoid the list getting overly long

				if (firstOverlapIndex < 0)
				{
					glowSeriesList.Add(
						new IndependantGlowSeries(
							beats: ImmutableList.Create(beat),
							glowStartTime: glowStartTime,
							glowEndTime: glowEndTime
						)
					);
				}

				//	if (
				//		beats
				//		.Select((taggedBeat, index) => (taggedBeat, index))
				//		.Skip(beatIndex + 1)
				//		.TryGetLast(
				//			x => x.taggedBeat.Tags.OfType<Glow>().Any(
				//				g => beatPositioner.BeatToTime(x.taggedBeat.Beat) - g.FadeInTime <= lastBeatGlowEndTime
				//			),
				//			out var lastOverlap
				//		)
				//	) {
				//		
				//	}
				//	
				//	beatIndex++;
			}

			glowSeriesList.Sort((x, y) => x.GlowStartTime.CompareTo(y.GlowStartTime));

			GroupToMeetTargetSize(glowSeriesList, targetSize);

			return glowSeriesList;
		}

		private static void GroupToMeetTargetSize(List<IndependantGlowSeries> glowSeriesList, int targetSize)
		{
			if (targetSize <= 1) return;

			for (int i = 0; i < glowSeriesList.Count - 1; )
			{
				// ^ "Count - 1" to avoid trying to combine the last element with elements that follow,
				// as there are no elements that follow. (see comment below for why this is helpful)

				int size = glowSeriesList[i].Beats.Count;
				int prevSize = size;
				if (size < targetSize)
				{
					int j = i;

					//Scan the next elements to find how many to use to minimise the gap
					//Prefer going below the target than above the target if the gap is the same either way

					while (j < glowSeriesList.Count) //will run at least once, as we already know i is not at the last element
					{
						prevSize = size;
						size += glowSeriesList[j].Beats.Count;

						int gapAhead = Math.Abs(targetSize - size);
						int gapBehind = Math.Abs(targetSize - prevSize);

						if (gapAhead >= gapBehind)
						{
							j--;
							break;
						}

						//If at the last element, break without incrementing j, so that j doesn't end up
						//out of range for the next part
						if (j == glowSeriesList.Count - 1) {
							break;
						}

						j++;
					}

					if (j > i) //If any of the next elements are to be included into the current one
					{
						var elementsToInclude = glowSeriesList.Skip(i + 1).Take(j - i);

						//Include them, concatenating all the relevant beats and working out the glow start/end times
						glowSeriesList[i] = glowSeriesList[i].AddAndExpand(
							beats: elementsToInclude.SelectMany(gs => gs.Beats),
							glowStartTime: elementsToInclude.Min(gs => gs.GlowStartTime),
							glowEndTime: elementsToInclude.Max(gs => gs.GlowEndTime)
						);

						//Flag all elements that have just been included as to-be-deleted
						for (int k = i + 1; k <= j; k++) {
							glowSeriesList[k] = IndependantGlowSeries.Empty;
						}

						//Move past all the elements that have just been included, then continue;
						i = j + 1;
						continue;
					}

					i++;
					continue;
				}

				i++;
				continue;
			}

			//Remove all elements that were combined into others and so flagged as to-be-deleted
			glowSeriesList.RemoveAll(x => x.IsEmpty); //Need to preserve the ordering, so can't use RemoveAllUnordered()
		}

		private IndependantGlowSeries AddAndExpand(IEnumerable<TaggedBeat> beats, double glowStartTime, double glowEndTime)
		{
			return new IndependantGlowSeries(
				beats: this.Beats.AddRange(beats),
				glowStartTime: Math.Min(this.GlowStartTime, glowStartTime),
				glowEndTime: Math.Max(this.GlowEndTime, glowEndTime)
			);
		}

		/// <summary>
		/// Removes all matching items from a list, without the requirement
		/// that the order of the items in the list be preserved. This is an
		/// O(n + m) operation, where n is the number of items in the list and
		/// m is the number removed.
		/// </summary>
		private static void RemoveAllUnordered<T>(List<T> list, Func<T, bool> predicate)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			int removed = 0;
			for (int i = 0; i < list.Count - removed; i++)
			{
				if (predicate(list[i]))
				{
					list[i] = list[list.Count - removed - 1];
					removed++;
				}
			}

			list.RemoveRange(list.Count - removed, removed);
		}
	}
}

//*/