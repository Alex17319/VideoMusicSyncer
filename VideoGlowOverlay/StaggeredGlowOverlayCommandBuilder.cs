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
	public static class StaggeredGlowOverlayCommandBuilder
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="inputVideo"></param>
		/// <param name="outputFile"></param>
		/// <param name="taggedBeats"></param>
		/// <param name="beatPositioner"></param>
		/// <param name="approxGlowsPerStage">
		/// Multiple glows in a single beat cannot be divided, so in these cases this value may be exceeded.
		/// For the last stage, this value will not be reached if there are too few glows remaining.
		/// </param>
		/// <returns></returns>
		public static string BuildBatchCommand(
			FFmpegInput inputVideo,
			FFmpegOutput outputFile,
			IEnumerable<TaggedBeat> taggedBeats,
			BeatPositioner beatPositioner,
			int approxGlowsPerStage
		) {
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
				approxGlowsPerStage
			);

			return (
				string.Join(
					" && ",
					ffmpegCommands
				)
				+ $" & DEL /P \"{tempFilesFolder.FullName}\"" //Gives the user a prompt before deleing, and empties the folder
				+ $" && RMDIR \"{tempFilesFolder.FullName}\"" //Doesn't prompt, and only deletes the folder if it's empty
			);
		}

		public static IEnumerable<FFmpegCommand> BuildFFmpegCommandSeries(
			FFmpegInput inputVideo,
			FFmpegOutput outputFile,
			DirectoryInfo tempFilesFolder,
			IEnumerable<TaggedBeat> taggedBeats,
			BeatPositioner beatPositioner,
			int approxGlowsPerStage
		) {
			ErrorUtils.ThrowIfArgNull(inputVideo, nameof(inputVideo));
			ErrorUtils.ThrowIfArgNull(outputFile, nameof(outputFile));
			ErrorUtils.ThrowIfArgNull(taggedBeats, nameof(taggedBeats));
			ErrorUtils.ThrowIfArgNull(beatPositioner, nameof(beatPositioner));
			ErrorUtils.ThrowIfArgLessThan(approxGlowsPerStage, 1, nameof(approxGlowsPerStage));

			var beatGroupInfos = GetBeatGroupInfos(inputVideo, outputFile, tempFilesFolder, taggedBeats, approxGlowsPerStage);

			return beatGroupInfos.Select(
				(x, i) => GlowOverlayCommandBuilder.BuildFFmpegCommand(
					x.InputFile,
					x.OutputFile,
					x.BeatGroup,
					beatPositioner
				)
			);
		}

		private static List<BeatGroupInfo> GetBeatGroupInfos(
			FFmpegInput inputVideo,
			FFmpegOutput outputFile,
			DirectoryInfo tempFilesFolder,
			IEnumerable<TaggedBeat> taggedBeats,
			int approxGlowsPerStage
		) {
			//	var glows = taggedBeats.SelectMany(
			//		beat => beat.Tags.OfType<Glow>().Select(
			//			tag => (beat, tag)
			//		)
			//	);

			//Make this a list, as the last element needs to be different,
			//but we only know how long it is after iterating through the whole Chunk() query.
			//By using a list it can just be modified after instead of iterating twice (and each
			//iteration creates a whole series of ReadOnlyCollections with backing lists,
			//so there's no extra memory cost or anything)
			var beatGroupInfos = new List<BeatGroupInfo>();

			var beatGroups = taggedBeats.Chunk(
				chunkSize: approxGlowsPerStage,
				counter: beat => beat.Tags.OfType<Glow>().Count()
			);

			FFmpegInput prevFile = inputVideo;
			foreach (var beatGroup in beatGroups)
			{
				FFmpegOutput nextFile = new FFmpegOutput(
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
				beatGroupInfos.Add(new BeatGroupInfo(beatGroup, prevFile, nextFile));

				prevFile = new FFmpegInput(nextFile.File);
			}

			var lastBeatGroupInfo = beatGroupInfos[beatGroupInfos.Count - 1];
			beatGroupInfos[beatGroupInfos.Count - 1] = new BeatGroupInfo(
				beatGroup: lastBeatGroupInfo.BeatGroup,
				inputFile: lastBeatGroupInfo.InputFile,
				outputFile: outputFile
			);

			return beatGroupInfos;
		}



		private struct BeatGroupInfo
		{
			public ReadOnlyCollection<TaggedBeat> BeatGroup { get; }
			public FFmpegInput InputFile;
			public FFmpegOutput OutputFile;

			public BeatGroupInfo(ReadOnlyCollection<TaggedBeat> beatGroup, FFmpegInput inputFile, FFmpegOutput outputFile)
			{
				ErrorUtils.ThrowIfArgNull(beatGroup, nameof(beatGroup));
				ErrorUtils.ThrowIfArgNull(inputFile, nameof(inputFile));
				ErrorUtils.ThrowIfArgNull(outputFile, nameof(outputFile));

				this.BeatGroup = beatGroup;
				this.InputFile = inputFile;
				this.OutputFile = outputFile;
			}
		}
	}
}

//*/