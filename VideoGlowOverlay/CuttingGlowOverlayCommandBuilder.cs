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
using VideoMusicSyncer.FFmpegCommandBuilder;

namespace VideoMusicSyncer.VideoGlowOverlay
{

	public static class CuttingGlowOverlayCommandBuilder
	{
		public static string BuildBatchCommand(
			FFmpegInput inputVideo,
			FFmpegOutput outputFile,
			IEnumerable<TaggedBeat> taggedBeats,
			BeatPositioner beatPositioner,
			int approxGlowsPerCut,
			int approxGlowsPerSubstage
		)
		{
			var tempFilesFolder = new DirectoryInfo(
				Path.Combine(
					Path.GetTempPath(),
					Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
				)
			);
			tempFilesFolder.Create();

			var ffmpegCommands = BuildFFmpegCommandSeries(
				inputVideo,
				outputFile,
				tempFilesFolder,
				taggedBeats,
				beatPositioner,
				approxGlowsPerCut,
				approxGlowsPerSubstage
			);

			return ( // && == proceed if previous command succeeded, & == proceed regardless of success
				"ECHO Started at %DATE% %TIME%"
				+ Environment.NewLine
				+ string.Join(
					Environment.NewLine,
					ffmpegCommands
				)
				+ Environment.NewLine
				+ $"ECHO Finished processing at %DATE% %TIME%" //TODO: Fix the fact that this is evaluated at the start of the command, not when it's relevant
				+ Environment.NewLine
				+ $"DEL /P \"{tempFilesFolder.FullName}\"" //Gives the user a prompt before deleing, and empties the folder
				+ Environment.NewLine
				+ $"RMDIR \"{tempFilesFolder.FullName}\"" //Doesn't prompt, and only deletes the folder if it's empty
				+ Environment.NewLine
				+ $"ECHO Exited at %DATE% %TIME%"
			);
		}

		public static IEnumerable<FFmpegCommand> BuildFFmpegCommandSeries(
			FFmpegInput inputVideo,
			FFmpegOutput outputFile,
			DirectoryInfo tempFilesFolder,
			IEnumerable<TaggedBeat> taggedBeats,
			BeatPositioner beatPositioner,
			int approxGlowsPerCut,
			int approxGlowsPerSubstage
		) {
			List<IndependantGlowSeries> cuts = IndependantGlowSeries.Split(taggedBeats, beatPositioner, approxGlowsPerCut);

			var prevResult = inputVideo;

			var glowOverlayedCutVideos = new List<(FFmpegOutput file, double cutStartTime, double cutEndTime)>();

			for (int i = 0; i < cuts.Count; i++)
			{
				bool isLastCut = i < cuts.Count - 1;

				double cutStartTime = cuts[i].GlowStartTime;
				double cutEndTime = cuts[i].GlowEndTime;

				//Build a command that cuts the relevant region out of the source video
				yield return BuildCutCommand(
					originalInputVideo: inputVideo,
					cutStartTime: cutStartTime,
					cutEndTime: cutEndTime,
					tempFilesFolder: tempFilesFolder,
					cutVideo: out FFmpegOutput cutVideo
				);


				//Build commands that overlay glows onto the cut region
				IEnumerable<FFmpegCommand> glowOverlayCommands = BuildCutGlowOverlayCommands(
					cutVideo: new FFmpegInput(cutVideo.File),
					cutStartTime: cutStartTime,
					cutEndTime: cutEndTime,
					cutBeats: cuts[i].Beats,
					beatPositioner: beatPositioner,
					tempFilesFolder: tempFilesFolder,
					approxGlowsPerSubstage: approxGlowsPerSubstage,
					glowOverlayedCutVideo: out FFmpegOutput glowOverlayedCutVideo
				);
				glowOverlayedCutVideos.Add((
					file: glowOverlayedCutVideo,
					cutStartTime: cutStartTime,
					cutEndTime: cutEndTime
				));
				foreach (var goc in glowOverlayCommands) {
					yield return goc;
				}
			}

			//Build a command that overlays each cut region ontop of the source video, at their original times
			yield return BuildReoverlayCommand(
				originalInputVideo: inputVideo,
				glowOverlayedCutVideos: glowOverlayedCutVideos,
				outputFile: outputFile
			);
		}

		private static FFmpegCommand BuildCutCommand(
			FFmpegInput originalInputVideo,
			double cutStartTime,
			double cutEndTime,
			DirectoryInfo tempFilesFolder,
			out FFmpegOutput cutVideo
		) {
			cutVideo = new FFmpegOutput(
				file: new FileInfo(
					Path.Combine(
						tempFilesFolder.FullName,
						Path.ChangeExtension(
							Path.GetRandomFileName(),
							FFmpegOutput.LosslessOutputExtension
						)
					)
				),
				modifiers: FFmpegOutput.LosslessOutputOptions
			);
			return new FFmpegCommand(
				inputs: ImmutableList.Create(originalInputVideo),
				filterGraph: new FFmpegComplexFilterGraph(
					new FFmpegFilterChain(
						new FFmpegFilterChainItem(
							inputStreams: ImmutableList.Create(
								new FFmpegPad("0:v")
							),
							filter: new FFmpegFilter(
								"trim",
								//Format values to avoid scientific notation, which FFmpeg can't parse
								new FFmpegFilterOption("start", cutStartTime.ToString("F10")),
								new FFmpegFilterOption("end", cutEndTime.ToString("F10"))
							)
						),
						new FFmpegFilterChainItem(
							new FFmpegFilter(
								"setpts",
								new FFmpegFilterOption("expr", $"PTS-STARTPTS-1.433367")
								//Idk where 1.433367 comes from but every output file was saying
								//"start: 1.433367" when read by ffmpeg and I needed a quick fix.
							)
						)
					),
					new FFmpegFilterChain(
						new FFmpegFilterChainItem(
							inputStreams: ImmutableList.Create(
								new FFmpegPad("0:a")
							),
							new FFmpegFilter(
								"atrim",
								//Format values to avoid scientific notation, which FFmpeg can't parse
								new FFmpegFilterOption("start", cutStartTime.ToString("F10")),
								new FFmpegFilterOption("end", cutEndTime.ToString("F10"))
							)
						),
						new FFmpegFilterChainItem(
							new FFmpegFilter(
								"asetpts",
								new FFmpegFilterOption("expr", $"PTS-STARTPTS")
							)
						)
					)
				),
				otherMidOptions: ImmutableList.Create(
					new FFmpegOption("pix_fmt", "yuv420p")
					//new FFmpegOption("c:a", "copy")
					//	new FFmpegOption("preset", "ultrafast")
				),
				outputs: ImmutableList.Create(cutVideo),
				otherFinalOptions: ImmutableList.Create<FFmpegOption>()
			);
		}

		private static IEnumerable<FFmpegCommand> BuildCutGlowOverlayCommands(
			FFmpegInput cutVideo,
			double cutStartTime,
			double cutEndTime,
			IEnumerable<TaggedBeat> cutBeats,
			BeatPositioner beatPositioner,
			DirectoryInfo tempFilesFolder,
			int approxGlowsPerSubstage,
			out FFmpegOutput glowOverlayedCutVideo
		) {
			glowOverlayedCutVideo = new FFmpegOutput(
				file: new FileInfo(
					Path.Combine(
						tempFilesFolder.FullName,
						Path.ChangeExtension(
							Path.GetRandomFileName(),
							FFmpegOutput.LosslessOutputExtension
						)
					)
				),
				modifiers: FFmpegOutput.LosslessOutputOptions
			);

			//?//	double cutFirstBeatTime = cutBeats.Min(b => beatPositioner.BeatToTime(b.Beat));

			Debug.WriteLine("cutStartTime: " + cutStartTime + ", beatPositioner.FirstBeat: " + beatPositioner.FirstBeat + ", first beat beat-time: " + cutBeats.First().Beat);

			var newBP = new BeatPositioner(
				firstBeat: beatPositioner.FirstBeat - cutStartTime,
				interval: beatPositioner.Interval,
				barLength: beatPositioner.BarLength
			);

			Debug.WriteLine("beatPositioner.FirstBeat - cutStartTime: " + (beatPositioner.FirstBeat - cutStartTime) + ", newBP.FirstBeat: " + newBP.FirstBeat);

			return StaggeredGlowOverlayCommandBuilder.BuildFFmpegCommandSeries(
				inputVideo: cutVideo,
				outputFile: glowOverlayedCutVideo,
				tempFilesFolder: tempFilesFolder,
				taggedBeats: cutBeats,
				beatPositioner: newBP,
				approxGlowsPerStage: approxGlowsPerSubstage
			);
		}

		private static FFmpegCommand BuildReoverlayCommand(
			FFmpegInput originalInputVideo,
			IReadOnlyCollection<(FFmpegOutput file, double cutStartTime, double cutEndTime)> glowOverlayedCutVideos,
			FFmpegOutput outputFile
		) {
			
			return new FFmpegCommand(
				inputs: ImmutableList.CreateRange(
					Enumerable.Concat(
						new[] { originalInputVideo },
						glowOverlayedCutVideos.Select(x => new FFmpegInput(x.file.File))
					)
				),
				filterGraph: new FFmpegComplexFilterGraph(
					new FFmpegFilterChain(
						glowOverlayedCutVideos.SelectMany(
							(x, i) => new[] {
								new FFmpegFilterChainItem(
									inputStreams: ImmutableList.Create(
										new FFmpegPad($"{i + 1}:v")
									),
									filter: new FFmpegFilter(
										//Set 'presentation time stamps' for each input frame, in order
										//to shift it forwards to the correct postion before overlaying
										"setpts",
										new FFmpegFilterOption("expr", $"PTS-STARTPTS+{x.cutStartTime}/TB")
									),
									outputStreams: ImmutableList.Create(
										new FFmpegPad($"shifted{i}")
									)
								),
								new FFmpegFilterChainItem(
									inputStreams: ImmutableList.Create(
										new FFmpegPad(i == 0 ? "0:v" : $"partialOverlayed{i - 1}"),
										new FFmpegPad($"shifted{i}")
									),
									filter: new FFmpegFilter(
										"overlay",
										new FFmpegFilterOption("x", 0),
										new FFmpegFilterOption("y", 0),
										new FFmpegEnableBetween(x.cutStartTime, x.cutEndTime)
									),
									outputStreams: (
										i < glowOverlayedCutVideos.Count - 1	
										? ImmutableList.Create(new FFmpegPad($"partialOverlayed{i}"))
										: ImmutableList.Create<FFmpegPad>()
									)
								)
							}
						)
					)
				),
				otherMidOptions: ImmutableList.Create(
					new FFmpegOption("pix_fmt", "yuv420p"),
					new FFmpegOption("c:a", "copy")
					//	new FFmpegOption("preset", "ultrafast")
				),
				outputs: ImmutableList.Create(outputFile),
				otherFinalOptions: ImmutableList.Create<FFmpegOption>()
			);
		}
	}
}

//*/