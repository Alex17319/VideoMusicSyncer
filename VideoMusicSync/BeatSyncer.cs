/*

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Linq;

namespace VideoMusicSyncer
{
	//Not used in this project, but useful to others accessing this project
	public class BeatSyncer
	{
		public BeatPositioner BeatPositioner { get; }

		public BeatSyncer(BeatPositioner beatPositioner)
		{
			this.BeatPositioner = beatPositioner ?? throw new ArgumentNullException(nameof(beatPositioner));
		}

		public IEnumerable<SyncPoint> AlignToFractionalBeats(double firstBeat, double beatFraction, IEnumerable<double> vidPositions)
		{
			return vidPositions.Select(
				(vpos, i) => new SyncPoint(
					videoPos: vpos,
					audioPos: BeatPositioner.BeatToTime(firstBeat + (beatFraction * i))
				)
			);
		}
	}
}

//*/