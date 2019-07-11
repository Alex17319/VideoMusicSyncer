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

namespace VideoMusicSyncer.FFmpegCommandBuilder
{
	public class FFmpegColorChannelMixer : FFmpegFilter
	{
		public FFmpegColorChannelMixer(
			double rr = 1, double rg = 0, double rb = 0, double ra = 0,
			double gr = 0, double gg = 1, double gb = 0, double ga = 0,
			double br = 0, double bg = 0, double bb = 1, double ba = 0,
			double ar = 0, double ag = 0, double ab = 0, double aa = 1,
			ImmutableList<FFmpegFilterOption> options = null
		) : base(
			"colorchannelmixer",
			(options ?? ImmutableList.Create<FFmpegFilterOption>())
			.AddRange(
				new[] {
					new FFmpegFilterOption("rr", rr),
					new FFmpegFilterOption("rg", rg),
					new FFmpegFilterOption("rb", rb),
					new FFmpegFilterOption("ra", ra),
					new FFmpegFilterOption("gr", gr),
					new FFmpegFilterOption("gg", gg),
					new FFmpegFilterOption("gb", gb),
					new FFmpegFilterOption("ga", ga),
					new FFmpegFilterOption("br", br),
					new FFmpegFilterOption("bg", bg),
					new FFmpegFilterOption("bb", bb),
					new FFmpegFilterOption("ba", ba),
					new FFmpegFilterOption("ar", ar),
					new FFmpegFilterOption("ag", ag),
					new FFmpegFilterOption("ab", ab),
					new FFmpegFilterOption("aa", aa)
				}.Where(x => options?.Any(o => o.Name == x.Name) != true)
				// ^ Let colour mix settings in 'options' take priority
			)
		) {
			ErrorUtils.ThrowIfArgOutsideRange(rr, -2.0, 2.0, nameof(rr));
			ErrorUtils.ThrowIfArgOutsideRange(rg, -2.0, 2.0, nameof(rg));
			ErrorUtils.ThrowIfArgOutsideRange(rb, -2.0, 2.0, nameof(rb));
			ErrorUtils.ThrowIfArgOutsideRange(ra, -2.0, 2.0, nameof(ra));
			ErrorUtils.ThrowIfArgOutsideRange(gr, -2.0, 2.0, nameof(gr));
			ErrorUtils.ThrowIfArgOutsideRange(gg, -2.0, 2.0, nameof(gg));
			ErrorUtils.ThrowIfArgOutsideRange(gb, -2.0, 2.0, nameof(gb));
			ErrorUtils.ThrowIfArgOutsideRange(ga, -2.0, 2.0, nameof(ga));
			ErrorUtils.ThrowIfArgOutsideRange(br, -2.0, 2.0, nameof(br));
			ErrorUtils.ThrowIfArgOutsideRange(bg, -2.0, 2.0, nameof(bg));
			ErrorUtils.ThrowIfArgOutsideRange(bb, -2.0, 2.0, nameof(bb));
			ErrorUtils.ThrowIfArgOutsideRange(ba, -2.0, 2.0, nameof(ba));
			ErrorUtils.ThrowIfArgOutsideRange(ar, -2.0, 2.0, nameof(ar));
			ErrorUtils.ThrowIfArgOutsideRange(ag, -2.0, 2.0, nameof(ag));
			ErrorUtils.ThrowIfArgOutsideRange(ab, -2.0, 2.0, nameof(ab));
			ErrorUtils.ThrowIfArgOutsideRange(aa, -2.0, 2.0, nameof(aa));
		}

		public FFmpegColorChannelMixer(
			(double r, double g, double b, double a) redPart,
			(double r, double g, double b, double a) greenPart,
			(double r, double g, double b, double a) bluePart,
			(double r, double g, double b, double a) alphaPart,
			ImmutableList<FFmpegFilterOption> options = null
		) : this(
			rr: redPart  .r,
			rg: redPart  .g,
			rb: redPart  .b,
			ra: redPart  .a,
			gr: greenPart.r,
			gg: greenPart.g,
			gb: greenPart.b,
			ga: greenPart.a,
			br: bluePart .r,
			bg: bluePart .g,
			bb: bluePart .b,
			ba: bluePart .a,
			ar: alphaPart.r,
			ag: alphaPart.g,
			ab: alphaPart.b,
			aa: alphaPart.a,
			options: options
		) { }

		//	/// <summary>Note: Defualt (no tint) is all values = 0.5</summary>
		//	/// <param name="redTint">In the range [0.0 to 1.0] (inclusive)</param>
		//	/// <param name="greenTint">In the range [0.0 to 1.0] (inclusive)</param>
		//	/// <param name="blueTint">In the range [0.0 to 1.0] (inclusive)</param>
		//	/// <param name="alphaTint">In the range [0.0 to 1.0] (inclusive)</param>
		//	public FFmpegColorChannelMixer Create(double redTint, double greenTint, double blueTint, double alphaTint)
		//	{
		//		ErrorUtils.ThrowIfArgOutsideRange(redTint, 0.0, 1.0, nameof(redTint));
		//		ErrorUtils.ThrowIfArgOutsideRange(greenTint, 0.0, 1.0, nameof(greenTint));
		//		ErrorUtils.ThrowIfArgOutsideRange(blueTint, 0.0, 1.0, nameof(blueTint));
		//		ErrorUtils.ThrowIfArgOutsideRange(alphaTint, 0.0, 1.0, nameof(alphaTint));
		//	
		//		//TODO: Test if these equations work well
		//		//Nope, can't tint white (and idk about black, don't know if that works)
		//		// r   g   b   result   how to achieve
		//		// 0   0   0   black    set all to zero
		//		// 1   1   1   white    set all to 2
		//		// 0.5 0.5 0.5 original set all same colours to 1, all change colours to 0
		//		// 1   0   0   red      rr = 2, rg = 1, rb = 1, ra = 1, g* = 0, b* = 0
		//		return new FFmpegColorChannelMixer(
		//			rr: Math.Max(redTint              , 0),
		//			rg: Math.Max(redTint - greenTint  , 0),
		//			rb: Math.Max(redTint - blueTint   , 0),
		//			ra: Math.Max(redTint - alphaTint  , 0),
		//			gr: Math.Max(greenTint - redTint  , 0),
		//			gg: Math.Max(greenTint            , 0),
		//			gb: Math.Max(greenTint - blueTint , 0),
		//			ga: Math.Max(greenTint - alphaTint, 0),
		//			br: Math.Max(blueTint - redTint   , 0),
		//			bg: Math.Max(blueTint - greenTint , 0),
		//			bb: Math.Max(blueTint             , 0),
		//			ba: Math.Max(blueTint - alphaTint , 0),
		//			ar: Math.Max(alphaTint - redTint  , 0),
		//			ag: Math.Max(alphaTint - greenTint, 0),
		//			ab: Math.Max(alphaTint - blueTint , 0),
		//			aa: Math.Max(alphaTint            , 0)
		//		);
		//	}
		//	
		//	public FFmpegColorChannelMixer Create((int r, int g, int b, int a) tint)
		//	{
		//		ErrorUtils.ThrowIfArgOutsideRange(tint.r, 0, 255, $"{nameof(tint)}.{nameof(tint.r)}");
		//		ErrorUtils.ThrowIfArgOutsideRange(tint.g, 0, 255, $"{nameof(tint)}.{nameof(tint.g)}");
		//		ErrorUtils.ThrowIfArgOutsideRange(tint.b, 0, 255, $"{nameof(tint)}.{nameof(tint.b)}");
		//		ErrorUtils.ThrowIfArgOutsideRange(tint.a, 0, 255, $"{nameof(tint)}.{nameof(tint.a)}");
		//	
		//		return Create(
		//			
		//		);
		//	}
	}
}

//*/