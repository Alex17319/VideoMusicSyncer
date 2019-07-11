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
	public struct MediaPos
	{
		/// <summary>
		/// Non-negative. Values beyond than the last position in the media
		/// (eg. <see cref="Double.PositiveInfinity"></see>) just represent the last position.
		/// </summary>
		public double Position { get; }

		public const double Start = 0;
		public const double End   = double.PositiveInfinity;

		/// <param name="position">
		/// Non-negative. Values beyond than the last position in the media
		/// (eg. <see cref="Double.PositiveInfinity"></see>) just represent the last position.
		/// </param>
		public MediaPos(double position)
		{
			if (position < 0) throw new ArgumentOutOfRangeException(nameof(position), position, "Cannot be negative.");

			this.Position = position;
		}

		public static explicit operator MediaPos(double x) => new MediaPos(x);
		public static implicit operator double(MediaPos x) => x.Position;

		public static bool Equals(MediaPos a, MediaPos b) => a.Position == b.Position;
		public override bool Equals(object obj) => obj is MediaPos p ? Equals(this, p) : false;
		public static bool operator ==(MediaPos a, MediaPos b) => Equals(a, b);
		public static bool operator !=(MediaPos a, MediaPos b) => !(a == b);
		public override int GetHashCode() => this.Position.GetHashCode();

		public override string ToString() {
			return $"{{[{nameof(MediaPos)}] {nameof(Position)}: {Position}}}";
		}
	}
}

//*/