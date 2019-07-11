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
using VideoMusicSyncer.WmmCommandBuider;

namespace VideoMusicSyncer
{
	public static class VideoMusicSync
	{
		public const string DefaultProjectName = "Synchronised";
		public const int VideoMediaItemID = 1;
		public const int AudioMediaItemID = 2;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="video"></param>
		/// <param name="audio"></param>
		/// <param name="syncPoints">A collection of points in the video that will be synced with specified points in the audio. These must be in ascending order, both for the video positions and audio positions. </param>
		/// <param name="projectName"></param>
		/// <param name="version"></param>
		/// <param name="themeID"></param>
		/// <param name="templateID"></param>
		/// <returns></returns>
		public static WmmProject Sync(
			FileInfo video,
			FileInfo audio,
			double videoLength,
			double audioLength,
			SyncPointCollection syncPoints, //Use SyncPointCollection not a collection of SyncedRanges to avoid having to validate as much stuff
			string projectName = DefaultProjectName,
			int version = WmmProject.DefaultVersion,
			int themeID = WmmProject.DefaultThemeID,
			string templateID = WmmProject.DefaultTemplateID
		) {
			if (video == null) throw new ArgumentNullException(nameof(video));
			if (audio == null) throw new ArgumentNullException(nameof(audio));
			if (videoLength < 0) throw new ArgumentException("Cannot be negative.", nameof(videoLength));
			if (audioLength < 0) throw new ArgumentException("Cannot be negative.", nameof(audioLength));
			if (syncPoints == null) throw new ArgumentNullException(nameof(syncPoints));
			projectName = string.IsNullOrEmpty(projectName) ? DefaultProjectName : projectName;
			templateID = string.IsNullOrEmpty(templateID) ? WmmProject.DefaultTemplateID : templateID;

			if (syncPoints.First.AudioPos > audioLength) throw new InvalidOperationException(
				$"The {nameof(SyncPoint.AudioPos)} of the first {nameof(SyncPoint)} is '{syncPoints.First.AudioPos}', "
				+ $"which is greater than the {nameof(audioLength)} ('{audioLength}')."
			);
			if (syncPoints.First.VideoPos > videoLength) throw new InvalidOperationException(
				$"The {nameof(SyncPoint.VideoPos)} of the first {nameof(SyncPoint)} is '{syncPoints.First.VideoPos}', "
				+ $"which is greater than the {nameof(videoLength)} ('{videoLength}')."
			);

			var extendIDs = new ExtentIdGenerator();

			//	//Section of audio skipped at the start, before the first sync point.
			//	//I don't think windows movie maker allows durations with no video,
			//	//so the video and audio both need to start at 0h:00m:00s, however there may
			//	//be some of either skipped at the start. As the audio time is used as
			//	//the absolute time, while the video is stretched and squished, it
			//	//makes sense to let this correspond to the duration of audio
			//	//before the first sync point.
			//	//This is subtracted from a bunch of calculations.
			//	var initialAudioSkipped = syncPoints.First.AudioPos;

			return WmmProject.Create(
				projectName: projectName,
				mediaItems: new[] {
					new WmmMediaItem(
						filePath: video.FullName,
						mediaItemID: VideoMediaItemID,
						mediaType: WmmMediaType.Video
					),
					new WmmMediaItem(
						filePath: audio.FullName,
						mediaItemID: AudioMediaItemID,
						mediaType: WmmMediaType.Audio
					),
				},
				videoClips: (
					syncPoints
					.GetSyncedRanges()
					.SelectMany(
						r => new WmmVideoClip[] {
							new WmmVideoClip( //TODO: Currently idk if the video order is actually specified
								extentID: extendIDs.GetNextID(),
								mediaItemID: VideoMediaItemID,
								inTime: r.Start.VideoPos,
								outTime: r.End.VideoPos,
								speed: r.SyncedVidSpeed
							),
						}
					)
				),
				audioClips: new WmmAudioClip[] {
					new WmmAudioClip( //TODO: Currently the time at which the audio should actually play isn't specified??
						extentID: extendIDs.GetNextID(),
						mediaItemID: AudioMediaItemID,
						inTime: syncPoints.First.AudioPos,
						outTime: Math.Min(syncPoints.Last.AudioPos, audioLength),
						speed: 1
					)
				},
				version: version,
				themeID: themeID,
				templateID: templateID
			);
		}

		
	}
}

//*/