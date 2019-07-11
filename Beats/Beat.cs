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
	//TODO: Allow an automatic position to be skipped to without needing to sync,
	//ie. allow multiple skips before a sync

	public struct Beat
	{
		public readonly double Position;
		public bool IsAuto => Position < 0;

		private Beat(double position)
		{
			this.Position = position;
		}

		public static Beat CreateFixed(double position)
		{
			if (Double.IsNaN(position)) throw new ArgumentException("Cannot be NaN", nameof(position));
			if (position < 0) throw new ArgumentOutOfRangeException(
				nameof(position),
				position,
				"Beats with fixed (non-automatic) positions cannot have negative positions."
			);

			return new Beat(position);
		}

		public static readonly Beat NextBeat        = NextFracBeat(powerOf2: 0);
		public static readonly Beat NextHalfBeat    = NextFracBeat(powerOf2: 1);
		public static readonly Beat NextQuarterBeat = NextFracBeat(powerOf2: 2);
		/// <summary>Bars have an unknown length, so <see cref="double.NegativeInfinity"/> is used to avoid collision with the values for other measurements.</summary>
		public static readonly Beat NextBar         = new Beat(double.NegativeInfinity);
		public static readonly Beat Next4BarGroup = NextBarGroup(4);
		public static readonly Beat Next8BarGroup = NextBarGroup(8);
		public static readonly Beat Next16BarGroup = NextBarGroup(8);
		//Note: Shouldn't get floating point errors as we're either dividing by powers of two, or multiplying,
		//but even if there are any, they shouldn't matter as all values to reprensent the same thing will have
		//identical errors (there's only one way to generate each thing to be repesented).
		//However when printing values nicely, round them appropriately to be safe.

		/// <summary>
		/// Creates an auto-positioned beat that represents the next position
		/// that is a multiple of 1/<paramref name="powerOf2"/> of a beat.
		/// </summary>
		public static Beat NextFracBeat(int powerOf2)
		{
			if (Double.IsNaN(powerOf2)) throw new ArgumentException("Cannot be NaN", nameof(powerOf2));
			if (powerOf2 < 0) throw new ArgumentOutOfRangeException(nameof(powerOf2), powerOf2, "Cannot be negative.");

			return new Beat(-1 / (double)Math.Pow(2, powerOf2));
		}

		/// <summary>
		/// Creates an auto-positioned beat that represents the next position
		/// that is a multiple of <paramref name="numBars"/> bars.
		/// </summary>
		public static Beat NextBarGroup(int numBars)
		{
			if (Double.IsNaN(numBars)) throw new ArgumentException("Cannot be NaN", nameof(numBars));
			if (numBars < 1) throw new ArgumentOutOfRangeException(nameof(numBars), numBars, "Cannot be less than 1.");

			return new Beat(-1 * numBars);
		}

		/// <summary>
		/// Returns the beat's position if it has a fixed position,
		/// or resolves it to a fixed position if has an automatic position.
		/// </summary>
		public double GetFixedBeat(double prevBeat, int barLength)
		{
			if (prevBeat < 0) throw new ArgumentOutOfRangeException(nameof(prevBeat), prevBeat, "Cannot be negative.");
			if (barLength < 0) throw new ArgumentOutOfRangeException(nameof(barLength), barLength, "Cannot be negative.");

			if (this.IsAuto)
			{
				double intervalLength;
				if (Double.IsNegativeInfinity(this.Position)) { //whole bar
					intervalLength = barLength;
				} else if (this.Position >= -1) { //fraction of a beat
					intervalLength = Math.Abs(this.Position);
				} else { //group of bars
					intervalLength = barLength * Math.Abs(this.Position);
				}

				return (Math.Floor(prevBeat/intervalLength) + 1) * intervalLength;
			}
			else
			{
				return this.Position;
			}
		}

		public override int GetHashCode() => this.Position.GetHashCode();
		public static bool Equals(Beat a, Beat b) => a.Position == b.Position;
		public override bool Equals(object obj) => obj is Beat beat && Equals(this, beat);
		public static bool operator ==(Beat a, Beat b) => Equals(a, b);
		public static bool operator !=(Beat a, Beat b) => !(a == b);

		public override string ToString()
		{
			if (this.IsAuto) {
				if (this.Position == -1) {
					return "{Auto-positioned beat - next beat}";
				} else if (Double.IsNegativeInfinity(this.Position)) {
					return "{Auto-positioned beat - next bar}";
				} else if (this.Position > -1) {
					return "{Auto-positioned beat - next 1/" + Math.Round(1 / Math.Abs(this.Position)) + "th beat}";
				} else {
					return "{Auto-positioned beat - next group of " + Math.Round(Math.Abs(this.Position)) + " bars}";
				}
			} else {
				return "{Fixed-position beat @ " + this.Position + "}";
			}
		}
	}
}

//*/