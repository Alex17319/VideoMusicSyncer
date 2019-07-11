using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace VideoMusicSyncer.Beats
{
	public class BeatPositioner
	{
		/// <summary>
		/// Note that this is the beat numbered 0, unlike normal music conventions
		/// (this way makes more sense for bars, quavers, etc, and makes the maths slightly simpler).
		/// <para/>
		/// Can be neagtive, positive, or zero
		/// </summary>
		public double FirstBeat { get; }
		/// <summary>Cannot be negative or zero</summary>
		public double Interval { get; }
		/// <summary>Cannot be negative or zero</summary>
		public int BarLength { get; }

		public double Bpm => 60 / Interval;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="firstBeat">May be negative, positive, or zero</param>
		/// <param name="interval">Cannot be negative or zero</param>
		/// <param name="barLength">Cannot be negative or zero</param>
		public BeatPositioner(double firstBeat, double interval, int barLength = 4)
		{
			if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval), interval, "Must be greater than zero.");
			if (barLength <= 0) throw new ArgumentOutOfRangeException(nameof(barLength), barLength, "Must be greater than zero.");

			this.FirstBeat = firstBeat;
			this.Interval = interval;
			this.BarLength = barLength;
		}

		public double BeatToTime(double beat) => FirstBeat + (beat * Interval);
		public double TimeToBeat(double time) => (time - FirstBeat)/Interval;
		public double BeatDeltaToTimeDelta(double beatDelta) => beatDelta * Interval;
		public double TimeDeltaToBeatDelta(double timeDelta) => timeDelta / Interval;

		public double BarToBeat(double bar)  => bar * BarLength;
		public double BeatToBar(double beat) => beat / BarLength;
		public double BarDeltaToBeatDelta(double bar) => BarToBeat(bar);
		public double BeatDeltaToBarDelta(double beat) => BeatToBar(beat);

		public double BarToTime(double bar) => BeatToTime(BarToBeat(bar));
		public double TimeToBar(double time) => BeatToBar(TimeToBeat(time));
		public double BarDeltaToTimeDelta(double barDelta) => BeatDeltaToTimeDelta(BarToBeat(barDelta));
		public double TimeDeltaToBarDelta(double timeDelta) => TimeDeltaToBeatDelta(TimeToBeat(timeDelta));

		public double NextBeatToTime(double currentTime, Beat nextBeat)
		{
			return BeatToTime(beat: nextBeat.GetFixedBeat(TimeToBeat(currentTime), this.BarLength));
		}
		public double this[double currentTime, Beat nextBeat] => NextBeatToTime(currentTime, nextBeat);

		public double ToTime(int bar, int beat                                                                                                                                    ) => BeatToTime((bar * BarLength) + beat                                                                                                                                             );
		public double ToTime(int bar, int beat, int quaver                                                                                                                        ) => BeatToTime((bar * BarLength) + beat + (quaver/2)                                                                                                                                );
		public double ToTime(int bar, int beat, int quaver, int semiquaver                                                                                                        ) => BeatToTime((bar * BarLength) + beat + (quaver/2) + (semiquaver/4)                                                                                                               );
		public double ToTime(int bar, int beat, int quaver, int semiquaver, int demisemiquaver                                                                                    ) => BeatToTime((bar * BarLength) + beat + (quaver/2) + (semiquaver/4) + (demisemiquaver/8)                                                                                          );
		public double ToTime(int bar, int beat, int quaver, int semiquaver, int demisemiquaver, int hemidemisemiquaver                                                            ) => BeatToTime((bar * BarLength) + beat + (quaver/2) + (semiquaver/4) + (demisemiquaver/8) + (hemidemisemiquaver/16)                                                                );
		public double ToTime(int bar, int beat, int quaver, int semiquaver, int demisemiquaver, int hemidemisemiquaver, int semihemidemisemiquaver                                ) => BeatToTime((bar * BarLength) + beat + (quaver/2) + (semiquaver/4) + (demisemiquaver/8) + (hemidemisemiquaver/16) + (semihemidemisemiquaver/32)                                  );
		public double ToTime(int bar, int beat, int quaver, int semiquaver, int demisemiquaver, int hemidemisemiquaver, int semihemidemisemiquaver, int demisemihemidemisemiquaver) => BeatToTime((bar * BarLength) + beat + (quaver/2) + (semiquaver/4) + (demisemiquaver/8) + (hemidemisemiquaver/16) + (semihemidemisemiquaver/32) + (demisemihemidemisemiquaver/64));

		public double this[int beat] => BeatToTime(beat);

		public double this[int bar, int beat                                                                     ] => ToTime(bar, beat                                             );
		public double this[int bar, int beat, int frac2                                                          ] => ToTime(bar, beat, frac2                                      );
		public double this[int bar, int beat, int frac2, int frac4                                               ] => ToTime(bar, beat, frac2, frac4                               );
		public double this[int bar, int beat, int frac2, int frac4, int frac8                                    ] => ToTime(bar, beat, frac2, frac4, frac8                        );
		public double this[int bar, int beat, int frac2, int frac4, int frac8, int frac16                        ] => ToTime(bar, beat, frac2, frac4, frac8, frac16                );
		public double this[int bar, int beat, int frac2, int frac4, int frac8, int frac16, int frac32            ] => ToTime(bar, beat, frac2, frac4, frac8, frac16, frac32        );
		public double this[int bar, int beat, int frac2, int frac4, int frac8, int frac16, int frac32, int frac64] => ToTime(bar, beat, frac2, frac4, frac8, frac16, frac32, frac64);
	}
}

//*/