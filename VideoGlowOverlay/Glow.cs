using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoMusicSyncer.FFmpegCommandBuilder;

namespace VideoMusicSyncer.VideoGlowOverlay
{
	public class Glow
	{
		public GlowType GlowType { get; }
		public double FadeInTime { get; }
		public double FadeOutTime { get; }
		/// <summary>
		/// Any value is valid, 0.0 = glow centre placed on the left hand edge,
		/// 1.0 = glow centre placed on the right hand edge
		/// </summary>
		public double X { get; }
		/// <summary>
		/// Any value is valid, 0.0 = glow centre placed on the top edge, 1.0 = glow centre placed on the bottom edge
		/// </summary>
		public double Y { get; }
		public double ScaleX { get; }
		public double ScaleY { get; }
		/// <summary>Clockwise, degrees</summary>
		public double RotateAngle { get; }
		public FFmpegColorChannelMixer ColorMixer { get; }

		/// <summary>
		/// Applied in addition to <see cref="ScaleX"/> and <see cref="ScaleY"/>.
		/// </summary>
		public const double AdditionalScaleFactor = 3;

		/// <summary>
		/// Fade in or out times below this value will be increased to this value.
		/// This is because when either fade time is zero, ffmpeg does not render the glow at all.
		/// <para/>
		/// At 30 fps, this duration is equivalent to 0.001*30 = 0.03 frames.
		/// </summary>
		public const double MinimumFadeTime = 0.001;

		/// <param name="fadeInTime">
		/// Values below <see cref="MinimumFadeTime"/> will be increased to <see cref="MinimumFadeTime"/>
		/// (see	<see cref="MinimumFadeTime"/> for why).
		/// <para/>
		/// Values below zero will cause an error.
		/// </param>
		/// <param name="fadeOutTime">
		/// Values below <see cref="MinimumFadeTime"/> will be increased to <see cref="MinimumFadeTime"/>
		/// (see	<see cref="MinimumFadeTime"/> for why).
		/// <para/>
		/// Values below zero will cause an error.
		/// </param>
		public Glow(
			GlowType glowType,
			double fadeInTime = 0.05,
			double fadeOutTime = 0.2,
			double x = 0.5,
			double y = 0.5,
			double scaleX = 1,
			double scaleY = 1,
			double rotateAngle = 0,
			FFmpegColorChannelMixer colorMixer = null
		) {
			ErrorUtils.ThrowIfArgNull(glowType, nameof(glowType));
			ErrorUtils.ThrowIfArgLessThan(fadeInTime, 0, nameof(fadeInTime));
			ErrorUtils.ThrowIfArgLessThan(fadeOutTime, 0, nameof(fadeOutTime));

			this.GlowType = glowType;
			this.FadeInTime = Math.Max(fadeInTime, MinimumFadeTime);
			this.FadeOutTime = Math.Max(fadeOutTime, MinimumFadeTime);
			this.X = x;
			this.Y = y;
			this.ScaleX = scaleX;
			this.ScaleY = scaleY;
			this.RotateAngle = rotateAngle;
			this.ColorMixer = colorMixer;
		}

		public Glow With(
			GlowType glowType = null,
			double? fadeInTime = null,
			double? fadeOutTime = null,
			double? x = null,
			double? y = null,
			double? scaleX = null,
			double? scaleY = null,
			double? rotateAngle = null,
			FFmpegColorChannelMixer colorMixer = null
		) {
			return new Glow(
				glowType: glowType ?? this.GlowType,
				fadeInTime: fadeInTime ?? this.FadeInTime,
				fadeOutTime: fadeOutTime ?? this.FadeOutTime,
				x: x ?? this.X,
				y: y ?? this.Y,
				scaleX: scaleX ?? this.ScaleX,
				scaleY: scaleY ?? this.ScaleY,
				rotateAngle: rotateAngle ?? this.RotateAngle,
				colorMixer: colorMixer ?? this.ColorMixer
			);
		}

		public override string ToString()
		{
			return "{[" + nameof(Glow) + "]" + (
				nameof(GlowType) + ": " + GlowType + ", " +
				nameof(FadeInTime) + ": " + FadeInTime + ", " +
				nameof(FadeOutTime) + ": " + FadeOutTime + ", " +
				nameof(X) + ": " + X + ", " +
				nameof(Y) + ": " + Y + ", " +
				nameof(ScaleX) + ": " + ScaleX + ", " +
				nameof(ScaleY) + ": " + ScaleY + ", " +
				nameof(RotateAngle) + ": " + RotateAngle + ", " +
				nameof(ColorMixer) + ": " + ColorMixer + ", "
			) + "}";
		}
	}
}

//*/