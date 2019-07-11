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
using VideoMusicSyncer;
using VideoMusicSyncer.FFmpegCommandBuilder;
using VideoMusicSyncer.Beats;
using VideoMusicSyncer.FluentDebugBreakPoint;

namespace VideoMusicSyncer.VideoGlowOverlay
{
	public static class GlowOverlayCommandBuilder
	{
		//Note: Use FFmpegInput and FFmpegOutput instead of FileInfos so that
		//codec information and other options can be easily passed in by the caller.
		public static FFmpegCommand BuildFFmpegCommand(
			FFmpegInput inputVideo,
			FFmpegOutput outputFile,
			IEnumerable<TaggedBeat> taggedBeats,
			BeatPositioner beatPositioner
		) {
			ErrorUtils.ThrowIfArgNull(inputVideo, nameof(inputVideo));
			ErrorUtils.ThrowIfArgNull(outputFile, nameof(outputFile));
			ErrorUtils.ThrowIfArgNull(taggedBeats, nameof(taggedBeats));
			ErrorUtils.ThrowIfArgNull(beatPositioner, nameof(beatPositioner));

			var glowTypeInfos = GetGlowTypeInfos(taggedBeats);

			int totalGlows = glowTypeInfos.Sum(x => x.Value.count);

			var inputs = GetInputs(inputVideo, glowTypeInfos).ToImmutableList();


			return new FFmpegCommand(
				inputs: GetInputs(inputVideo, glowTypeInfos).ToImmutableList(),
				filterGraph: new FFmpegComplexFilterGraph(
					ImmutableList.CreateRange(
						Enumerable.Concat(
							GetGlowDuplicatorFilterChains(glowTypeInfos),
							GetGlowModifyAndOverlayFilterChains(glowTypeInfos, taggedBeats, beatPositioner)
						)
					)
				),
				otherMidOptions: ImmutableList.Create(
					new FFmpegOption("pix_fmt", "yuv420p"),
					new FFmpegOption("c:a", "copy")
					//	new FFmpegOption("preset", "ultrafast")
				),
				outputs: ImmutableList.Create(
					outputFile
				),
				otherFinalOptions: ImmutableList.Create<FFmpegOption>()
			);
			
		}

		private static Dictionary<GlowType, (int index, int count)> GetGlowTypeInfos(IEnumerable<TaggedBeat> taggedBeats)
		{
			var glowTypeInfos = new Dictionary<GlowType, (int index, int count)>();
			foreach (var beat in taggedBeats)
			{
				foreach (Glow g in beat.Tags.OfType<Glow>())
				{
					(int index, int count) glowTypeInfo;
					if (glowTypeInfos.TryGetValue(g.GlowType, out glowTypeInfo)) {
						glowTypeInfo.count++;
					} else {
						glowTypeInfo = (index: glowTypeInfos.Count, count: 1);
					}
					glowTypeInfos[g.GlowType] = glowTypeInfo;
				}
			}
			return glowTypeInfos;
		}

		private static IEnumerable<FFmpegInput> GetInputs(FFmpegInput inputVideo, Dictionary<GlowType, (int index, int count)> glowTypeInfos)
		{
			return Enumerable.Concat(
				new[] { inputVideo },
				glowTypeInfos.Select(
					g => new FFmpegInput(
						file: g.Key.File,
						modifiers: ImmutableList.Create(new FFmpegOption("loop", "1"))
					)
				)
			);
		}

		private static IEnumerable<FFmpegFilterChain> GetGlowDuplicatorFilterChains(
			Dictionary<GlowType, (int index, int count)> glowTypeInfos
		) {
			return (
				from glowTypeInfo in glowTypeInfos.Values
				select new FFmpegFilterChain(
					new FFmpegFilterChainItem(
						inputStreams: ImmutableList.Create(
							new FFmpegPad($"{glowTypeInfo.index + 1}:v")
						),
						filter: new FFmpegFilter(
							name: "split",
							options: ImmutableList.Create(
								new FFmpegFilterOption(
									value: glowTypeInfo.count
								)
							)
						),
						outputStreams: Enumerable.Range(0, glowTypeInfo.count).Select(
							i => new FFmpegPad($"glow{IntToLetters(glowTypeInfo.index)}{i}")
						).ToImmutableList()
					)
				)
			);
		}

		private static IEnumerable<FFmpegFilterChain> GetGlowModifyAndOverlayFilterChains(
			Dictionary<GlowType, (int index, int count)> glowTypeInfos,
			IEnumerable<TaggedBeat> taggedBeats,
			BeatPositioner beatPositioner
		) {
			var glowIndices = glowTypeInfos.ToDictionary(keySelector: g => g.Key, elementSelector: g => 0);
			int watermarkIndex = 0;
			var glowModifierFilterChains = new List<FFmpegFilterChain>();
			foreach (var beat in taggedBeats)
			{
				foreach (var g in beat.Tags.OfType<Glow>())
				{
					double glowPeakTime = beatPositioner.BeatToTime(beat.Beat);
					double glowBeginTime = glowPeakTime - g.FadeInTime;
					double glowEndTime = glowPeakTime + g.FadeOutTime;

					yield return new FFmpegFilterChain(
						ImmutableList.Create(
							new FFmpegFilterChainItem(
								inputStreams: ImmutableList.Create(
									new FFmpegPad(
										$"glow{IntToLetters(glowTypeInfos[g.GlowType].index)}{glowIndices[g.GlowType]}"
									)
								),
								filter: new FFmpegFilter(
									name: "fade",
									options: ImmutableList.Create(
										new FFmpegFilterOption("type", "in"),
										new FFmpegFilterOption(
											"start_time",
											glowBeginTime.ToString("F10") //Avoid scientific notation, which FFmpeg can't parse
										),
										new FFmpegFilterOption(
											"duration",
											g.FadeInTime.ToString("F10") //Avoid scientific notation, which FFmpeg can't parse
										)
									)
								)
							),
							//?//	new FFmpegFilterChainItem(
							//?//		filter: new FFmpegFilter(
							//?//			"scale",
							//?//			options: ImmutableList.Create(
							//?//				new FFmpegFilterOption("width", $"(in_w*{g.ScaleX}*{Glow.AdditionalScaleFactor})"),
							//?//				new FFmpegFilterOption("height", $"(in_h*{g.ScaleY}*{Glow.AdditionalScaleFactor})")
							//?//			)
							//?//		)
							//?//	),
							//?//	new FFmpegFilterChainItem(
							//?//		filter: new FFmpegFilter(
							//?//			"rotate",
							//?//			options: ImmutableList.Create(
							//?//				new FFmpegFilterOption("angle", (g.RotateAngle / 360) * 2 * Math.PI),
							//?//				new FFmpegFilterOption("fillcolor", "none"), //used where the rotation creates gaps
							//?//				new FFmpegEnableBetween(glowBeginTime, glowEndTime)
							//?//			)
							//?//		)
							//?//	),
							//?//	new FFmpegFilterChainItem(
							//?//		filter: (g.ColorMixer ?? new FFmpegColorChannelMixer()).WithChangedOptions(
							//?//			new FFmpegEnableBetween(glowBeginTime, glowEndTime)
							//?//		)
							//?//	),
							new FFmpegFilterChainItem(
								filter: new FFmpegFilter(
									name: "fade",
									options: ImmutableList.Create(
										new FFmpegFilterOption("type", "out"),
										new FFmpegFilterOption(
											"start_time",
											glowPeakTime.ToString("F10") //Avoid scientific notation, which FFmpeg can't parse
										),
										new FFmpegFilterOption(
											"duration",
											g.FadeOutTime.ToString("F10") //Avoid scientific notation, which FFmpeg can't parse
										)
									)
								),
								outputStreams: ImmutableList.Create(
									new FFmpegPad(
										$"watermark{watermarkIndex}"
									)
								)
							),
							new FFmpegFilterChainItem(
								inputStreams: (
									ImmutableList.Create(
										watermarkIndex == 0 ? new FFmpegPad("v:0") : new FFmpegPad($"tmp{watermarkIndex - 1}"),
										new FFmpegPad($"watermark{watermarkIndex}")
									)
								),
								filter: new FFmpegFilter(
									name: "overlay",
									options: ImmutableList.Create(
										//TODO: Check: //Don't know why, but it works differently for x and y
										new FFmpegFilterOption("x", $"(-(overlay_w/2) + {g.X}*main_w)"),
										new FFmpegFilterOption("y", $"(-(overlay_h/2) + {g.Y}*main_h)"),
										new FFmpegFilterOption("shortest", 1),
										new FFmpegEnableBetween(glowBeginTime, glowEndTime)
									)
								),
								outputStreams: ImmutableList.Create(
									new FFmpegPad($"tmp{watermarkIndex}")
								)
							)
						)
					);

					glowIndices[g.GlowType] += 1;
					watermarkIndex += 1;
				}
			}

			//Use the last tmp stream as the output
			yield return new FFmpegFilterChain(
				new FFmpegFilterChainItem(
					inputStreams: ImmutableList.Create(new FFmpegPad($"tmp{watermarkIndex - 1}")),
					filter: new FFmpegFilter("copy")
				)
			);
		}

		//Adapted from: https://codereview.stackexchange.com/a/44094
		/// <summary>
		/// Converts a number to an alphabetical representation. I.e. base 26 with A = 0, B = 1, etc
		/// </summary>
		private static string IntToLetters(int value)
		{
			value++; //Make A correspond to input 0, B correspond to 1, etc

			string result = string.Empty;
			while (--value >= 0)
			{
				result = (char)('A' + value % 26 ) + result;
				value /= 26;
			}
			return result;
		}
	}
}

//*/