/*

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using VideoMusicSyncer;
using System.Linq;
using VideoMusicSyncer.FluentDebugBreakPoint;
using VideoMusicSyncer.VideoGlowOverlay;
using VideoMusicSyncer.FFmpegCommandBuilder;
using VideoMusicSyncer.Beats;
using VideoMusicSyncer.WmmCommandBuider;

namespace VideoMusicSyncer.Controller
{
	class Program
	{
		static void Main(string[] args)
		{
			//SyncAudioVideo();

			PrintFFmpegOverlayEffectsCommand();
		}

		static void SyncAudioVideo()
		{
			const double firstBeatVideoPos = 16.03; //13.70;
			const double firstBeatAudioPos = 37.365;
			const double beatInterval = (
				//0.26077241379310344827586206896552 //(75.177 - 37.365)/145 (my own measurements & counting - I think I miscounted or something)
				0.46875 //(60s)/(128 BPM) 
				//128 BPM based on:
				// - https://www.audiokeychain.com/track/tgi/laidback-luke-peking-duk-mufasa
				// - https://tunebat.com/Info/Mufasa-Original-Mix-Laidback-Luke-Peking-Duck/49mTyByYvtaQwedg3VM6ky
				// - https://www.beatport.com/track/mufasa-original-mix/6319471
			);
			const int barLength = 4;

			var beats = new BeatPositioner(
				firstBeat: firstBeatAudioPos,
				interval: beatInterval,
				barLength: barLength
			);

			//Intro music:
			//		https://open.spotify.com/track/1oDAFTOXZGSQedBa6hXGhT?si=jIQijymKSayXfa-l_erSkw (sugar plum v1)
			//			00:27 to 00:43
			//			01:37 to 01:55
			//		https://open.spotify.com/track/7a9g6fPCrxdZ8p4tLCWNdR?si=GrxBoHLVT5mNm7jjYcNEDQ (sugar plum v2)
			//			00:30 to 00:48
			//			01:44 to 02:01
			//		https://www.youtube.com/watch?v=45lNvNsdPLc (sugar plum v3)
			//			00:26 to 00:41 (use this one) (15 seconds)
			//			01:30 to 01:45

			WmmProject project = VideoMusicSync.Sync(
				video: new FileInfo(@"C:\Users\Alex\Pictures\From Phone\Pictures\Phone\Personal\Mum Sleeping TV\Z1_VID_20180425_205156-face-1-blurred-by-youtube-via-clipconverter.mp4"),
				audio: new FileInfo(@"C:\Users\Alex\Pictures\Meme Templates\Meme Making\Mum Headbanging\Peking Duk & Laidback Luke - Mufasa (Radio Edit) - edit 4.wav"),
				videoLength: 344.91,
				audioLength: 183.948,
				syncPoints: (
					//	new SyncPointCollection(
					//		ImmutableList.Create<SyncPoint>()
					//		.Add(new SyncPoint(13.70, beats[0, 0]))
					//		.Add(new SyncPoint(14.83, beats[0, 1]))
					//		.Add(new SyncPoint(16.03, beats[0, 2]))
					//		.Add(new SyncPoint(17.83, beats[0, 3]))
					//	//	.Add(new SyncPoint(19.30, beats[1, 0])) //sideways nod
					//		.Add(new SyncPoint(21.20, beats[1, 0]))
					//		.Add(new SyncPoint(22.23, beats[1, 1]))
					//		.Add(new SyncPoint(23.17, beats[1, 2]))
					//		.Add(new SyncPoint(24.97, beats[1, 3]))
					//		.Add(new SyncPoint(28.83, beats[2, 0]))
					//		.Add(new SyncPoint(30.63, beats[2, 1]))
					//	//	.Add(new SyncPoint(33.20, beats[2, 2])) //sideways nod
					//		.Add(new SyncPoint(35.27, beats[2, 2]))
					//		.Add(new SyncPoint(36.73, beats[2, 3]))
					//		.Add(new SyncPoint(40.83, beats[3, 0]))
					//		.Add(new SyncPoint(42.30, beats[3, 1]))
					//	)
					
					//	new BeatSyncSequence(
					//		firstSyncPoint: new SyncPoint(firstBeatVideoPos, firstBeatAudioPos),
					//		beatInterval: beatInterval,
					//		barLength: barLength
					//	)
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(14.83) //forward
					//	.SkipByBeats(2.0/3.0      ).SyncWithVideo(16.03) //back
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(17.83) //forward
					//	.SkipByBeats(1.0/3.0      ).SyncWithVideo(19.30) //sideways
					//	.SkipByBeats(2.0/3.0      ).SyncWithVideo(21.20) //back
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(22.23) //small forward
					//	.SkipByBeats(2.0/3.0      ).SyncWithVideo(23.17) //back
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(24.97) //forward
					//	.SkipByBeats(2.0/3.0      ).SyncWithVideo(28.83) //back
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(30.63) //forward
					//	.SkipByBeats(2.0/3.0      ).SyncWithVideo(33.20) //sideways
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(35.27)
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(36.73)
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(40.83)
					//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(42.30)

					//	new SyncPointCollection(
					//		new BeatSyncSequence(
					//			firstSyncPoint: new SyncPoint(firstBeatVideoPos, firstBeatAudioPos),
					//			beatInterval: beatInterval,
					//			barLength: barLength
					//		)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(14.83)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(16.03)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(17.83)
					//		.SkipByBeats(2.0/3.0      ).SyncWithVideo(19.30) //sideways nod
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(21.20)
					//		.SkipByBeats(2.0/3.0      ).SyncWithVideo(22.23) //small nod
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(23.17)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(24.97)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(28.83)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(30.63)
					//		.SkipByBeats(2.0/3.0      ).SyncWithVideo(33.20) //sideways nod
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(35.27)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(36.73)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(40.83)
					//		.SkipToBeat (Beat.NextBeat).SyncWithVideo(42.30)
					//	//	.SkipToBeat (Beat.NextBeat).SyncWithVideo()
					//	//	.SkipToBeat (Beat.NextBeat).SyncWithVideo()
					//	//	.SkipToBeat (Beat.NextBeat).SyncWithVideo()
					//	//	.SkipToBeat (Beat.NextBeat).SyncWithVideo()
					//		.SyncPoints
					//	)

					new SyncPointCollection(
						new BeatSyncSequence(
							firstSyncPoint: new SyncPoint(firstBeatVideoPos, firstBeatAudioPos),
							beatInterval: beatInterval,
							barLength: barLength
						)
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 16.49) //creep forward
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 17.83) //slam forward
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 19.30) //sideways nod
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 21.20) //slam back
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 22.23) //slow small nod forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 23.17) //small slam back
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 24.03) //creep forward
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 24.97) //slam forward
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 27.30) //creep back & sideways
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 28.83) //slam back on inserted beat
						.SkipToBeat (Beat.NextBeat)
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 29.00) //creep forwards until inserted beat
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 30.63) //slam forwards from inserted beat to actual beat
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 33.20) //slow sideways nod
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 35.27) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 35.70) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 36.73) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 37.80) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 40.43) //slam backwards //40.83
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 41.20) //stay still then creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 42.30) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 43.47) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 44.33) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 45.13) //hover
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 45.97) //slam further backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 46.27) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 46.90) //slam forwards
						//Normal beat, replaced by synced to notes version
						//.SkipByBeats(2.0/3.0      ).SyncWithVideo( 49.37) //hover & creep backwards
						//.SkipToBeat (Beat.NextBeat).SyncWithVideo( 50.90) //slam backwards
						//.SkipByBeats(2.0/3.0      ).SyncWithVideo( 51.27) //creep forwards
						//.SkipToBeat (Beat.NextBeat).SyncWithVideo( 52.17) //slam forwards
						//.SkipByBeats(2.0/3.0      ).SyncWithVideo( 53.67) //sideways nod
						//.SkipToBeat (Beat.NextBeat).SyncWithVideo( 54.97) //slam backwards
						//No idea but this didn't sync up properly with the beats. The next bit worked instead
						//.SkipToBeat (Beat.NextBeat)
						//.SkipByBeats(1.0/3.0      ).SyncWithVideo( 47.23) //almost freeze - extended //49.47
						//.SkipByBeats(1.0/3.0      ).SyncWithVideo( 50.90) //slam backwards on note
						//.SkipToBeat (Beat.NextBeat).SyncWithVideo( 52.17) //slam forwards on beat
						//.SkipByBeats(1.0/3.0      ).SyncWithVideo( 53.67) //sideways nod
						//.SkipByBeats(1.0/3.0      ).SyncWithVideo( 54.97) //slam backwards on note
						//.SkipToBeat (Beat.NextBeat).SyncWithVideo( 56.07) //slam forwards on beat
						//I have no idea why this bit works but it does
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 47.23) //almost freeze - extended
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 50.90) //slam backwards on note
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 52.17) //slam forwards on beat
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 53.67) //sideways nod
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 54.97) //slam backwards on note
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 56.07) //slam forwards on beat
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 56.83) //creep backwards
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 58.80) //slam backwards on note
						.SkipByBeats(1.0/3.0      ).SyncWithVideo( 61.17) //slam forwards on beat
						//Back to normal from here, makes sense again
						.SkipByBeats(2.5/3.0      ).SyncWithVideo( 62.00) //creep backwards
						//This one looked awkward as two slams back in a row, so I made it one stuttered
						//slam (as there's also a wobble, which adds a third pause)
						//.SkipByBeats(2.5/3.0      ).SyncWithVideo( 64.40) //wobble & creep backwards
						//.SkipToBeat (Beat.NextBeat).SyncWithVideo( 65.63) //mini slam backwards pre-beat
						//.SkipByBeats(2.5/3.0      ).SyncWithVideo( 66.73) //hover
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 67.27) //stuttered slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 67.50) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 68.33) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 69.00) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 69.70) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 70.97) //hover
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 72.27) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 72.63) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 73.73) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 78.87) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 80.10) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 80.87) //hover
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 81.67) //slam further backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 82.13) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 83.27) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 83.90) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 86.37) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 87.37) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo( 88.47) //slam forwards
						.SkipToBeat (Beat.NextBeat)
						.SkipToBeat (Beat.NextBeat)
						.SkipToBeat (Beat.NextBeat)
						.SkipToBeat (Beat.NextBeat)
						.SkipToBeat (Beat.NextBeat) //5 total
						.SkipByBeats(2.0/3.0      ).SyncWithVideo( 94.57) //intermission (half wake up)
						//.SkipByBeats(2.0/3.0      ).SyncWithVideo( 98.93) //intermission
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(100.33) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(100.67) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(101.70) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(102.03) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(103.23) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(103.53) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(104.67) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(105.27) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(106.00) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(108.13) //hover & creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(109.23) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(111.43) //side glance & creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(112.97) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(113.33) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(114.33) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(115.13) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(116.10) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(120.73) //hover & creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(121.83) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(127.93) //hover & creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(129.83) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(130.37) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(131.07) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(131.80) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(135.03) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(135.23) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(136.20) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(137.50) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(138.80) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(139.30) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(140.07) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(141.43) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(142.10) //slam backwards (small)
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(142.63) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(143.33) //slam forwards (small)
						//.SkipByBeats(5)
						//.SkipToBeat (Beat.NextBeat).SyncWithVideo(152.83) //intermission
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(152.83) //long hover & creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(154.60) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(154.97) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(155.70) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(156.20) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(158.13) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(158.50) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(159.43) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(160.77) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(162.40) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(165.00) //hover & creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(166.20) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(167.73) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(170.03) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(170.50) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(171.50) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(172.70) //creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(173.33) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(173.90) //hover
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(175.20) //slam further backwards //174.97
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(175.63) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(176.87) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(179.40) //creep backwards (half wake up)
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(181.50) //slam backwards (with one stutter)
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(182.87) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(183.87) //slam forwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(187.00) //hover & creep backwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(190.03) //slam backwards
						.SkipByBeats(2.0/3.0      ).SyncWithVideo(190.60) //creep forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(191.77) //slam forwards
						.SkipToBeat (Beat.NextBeat).SyncWithVideo(193.20) //end video between last beat and here
						//	.SkipByBeats(2.0/3.0      ).SyncWithVideo(193.20) //sideways nod
						//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(195.77) //slam backwards (with one stutter)
						//	.SkipByBeats(2.0/3.0      ).SyncWithVideo(196.00) //creep forwards
						//	.SkipToBeat (Beat.NextBeat).SyncWithVideo(196.80) //slam forwards
						//	.SkipByBeats(10.0).SyncWithVideo(196.80 + 5) //wake up
						//	.SkipByBeats(2.0/3.0      ).SyncWithVideo()
						//	.SkipToBeat (Beat.NextBeat).SyncWithVideo()
						.SyncPoints.ToImmutableList()
					)
				)
			);

			project.ToXml().Save(
				@"C:\Users\Alex\Pictures\Meme Templates\Meme Making\Mum Headbanging\"
				+ "Sync " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")
				+ ".wlmp"
			);
		}

		static void PrintFFmpegOverlayEffectsCommand()
		{
			const double firstBeatAudioPos = 16.483; //Different as this is using the synced & edited video
			const double beatInterval = 0.46875;
			const int barLength = 4;

			var beats = new BeatPositioner(
				firstBeat: firstBeatAudioPos,
				interval: beatInterval,
				barLength: barLength
			);

			const string mainFolder = @"C:\Users\Alex\Pictures\Meme Templates\Meme Making\Mum Headbanging";

			var GlowWhiteCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-white.png")));
			var GlowLargeWhiteCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-white-2x.png")));
			//var GlowDiamond = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-diamond-white.png")));
			var GlowTRedCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-red-transparent1.png")));
			var GlowTBlueCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-blue-transparent1.png")));
			var GlowTGreenCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-green-transparent1.png")));

			//	colorMixer: new FFmpegColorChannelMixer(
			//		redPart: (1, 1, 1, 0),
			//		greenPart: (0, 0, 0, 0),
			//		bluePart: (0, 0, 0, 0),
			//		alphaPart: (0, 0, 0, 0.5)
			//	),
			Glow fullBeatEven = new Glow(GlowLargeWhiteCircle, x: 0, y: 0);
			Glow fullBeatOdd = new Glow(GlowLargeWhiteCircle, x: 1, y: 0);
			Glow synthBeatRed = new Glow(GlowTRedCircle, x: 0.3, y: 0);
			Glow synthBeatBlue = new Glow(GlowTBlueCircle, x: 0.3, y: 0);
			Glow synthBeatGreen = new Glow(GlowTGreenCircle, x: 0.3, y: 0);
			Glow minorSynthBeatRed = new Glow(GlowTRedCircle, x: 0.7, y: 0);
			Glow minorSynthBeatBlue = new Glow(GlowTBlueCircle, x: 0.7, y: 0);
			Glow minorSynthBeatGreen = new Glow(GlowTGreenCircle, x: 0.7, y: 0);



			var taggedBeats = new BeatTagSequence(
				beatPositioner: beats,
				firstBeatTags: ImmutableList.Create<object>(fullBeatEven, synthBeatRed)
			)
			.SkipByBeats(2.0/3.0).TagAndAdd(minorSynthBeatRed)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd, synthBeatRed)
			.SkipByBeats(1.0/3.0).TagAndAdd(minorSynthBeatRed) //;
			.SkipByBeats(1.0/3.0).TagAndAdd(minorSynthBeatRed)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven, synthBeatRed)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd, synthBeatRed)
			.SkipByBeats(2.0/3.0).TagAndAdd(synthBeatRed)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipByBeats(2.0/3.0).TagAndAdd(synthBeatRed)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd) //;
			.SkipByBeats(2.0/3.0).TagAndAdd(synthBeatRed)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipByBeats(2.0/3.0).TagAndAdd(synthBeatRed)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven) //;
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd);
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//	.SkipToBeat(Beat.NextBeat).TagAndAdd(fullBeatEven);

			
			
			Console.WriteLine("Tagged beat timings:");
			Console.WriteLine();

			foreach (var b in taggedBeats.TaggedBeats)
			{
				Console.WriteLine(beats.BeatToTime(b.Beat).ToString(".0000") + " [" + string.Join(", ", b.Tags) + "]");
			}

			Console.WriteLine();
			Console.WriteLine("FFmpeg input:");
			Console.WriteLine();

			Console.WriteLine(
				//	GlowOverlayCommandBuilder.BuildFFmpegCommand(
				//		inputVideo: new FFmpegInput(
				//			new FileInfo(
				//				Path.Combine(mainFolder, "Meme Video 1.2.1.mp4")
				//			)
				//		),
				//		outputFile: new FFOutput(
				//			new FileInfo(
				//				Path.Combine(mainFolder, "Meme Video 5.3 - " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".mp4")
				//			)
				//		),
				//		taggedBeats: taggedBeats.TaggedBeats,
				//		beatPositioner: beats
				//	).ToString()
				//	StaggeredGlowOverlayCommandBuilder.BuildBatchCommand(
				//		inputVideo: new FFmpegInput(
				//			new FileInfo(
				//				Path.Combine(mainFolder, "Meme Video 1.2.1.mp4")
				//			)
				//		),
				//		outputFile: new FFOutput(
				//			new FileInfo(
				//				Path.Combine(mainFolder, "Meme Video 5.3 - " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".mp4")
				//			)
				//		),
				//		taggedBeats: taggedBeats.TaggedBeats,
				//		beatPositioner: beats,
				//		approxGlowsPerStage: 2
				//	)
				CuttingGlowOverlayCommandBuilder.BuildBatchCommand(
					inputVideo: new FFmpegInput(
							new FileInfo(
							Path.Combine(mainFolder, "Meme Video 1.2.1.mp4")
						)
					),
					outputFile: new FFmpegOutput(
						new FileInfo(
							Path.Combine(mainFolder, "Meme Video 5.3 - " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".mp4")
						)
					),
					taggedBeats: taggedBeats.TaggedBeats,
					beatPositioner: beats,
					approxGlowsPerCut: 7,
					approxGlowsPerSubstage: 10 //I.e. don't bother with substages
				)
			);

			

			Console.ReadLine();

			//	const double defaultGlowFadeIn = 0.05;
			//	const double defaultGlowFadeOut = 0.2;
			//	
			//	const string fadeInTimeTag = "fade-in:";
			//	const string fadeOutTimeTag = "fade-out:";
			//	
			//	var inputs = ImmutableList.Create(
			//		Enumerable.Concat(
			//			new[] { new FFmpegInput(new FileInfo(Path.Combine(mainFolder, "Meme Video 1.2.1.mp4"))) },
			//			GlowType.GlowTypes.Select(
			//				g => new FFmpegInput(
			//					file: new FileInfo(Path.Combine(mainFolder, g.FileName)),
			//					modifiers: ImmutableList.Create(new FFmpegOption(""
			//				)
			//			)
			//		)
			//	);
			//	
			//	Console.WriteLine("ffmpeg");
			//	Console.WriteLine($@"-i ""{mainFolder}\Meme Video 1.2.1.mp4""");
			//	
			//	Console.WriteLine(string.Join(" ", GlowType.GlowTypes.Select(g => $@"-loop 1 -i ""{mainFolder}\{g.FileName}""")));
			//	
			//	//	int numFlashes = taggedBeats.TaggedBeats.Sum(
			//	//		b => b.Tags.Count(
			//	//			t => GlowType.GlowTypes.Any(
			//	//				g => g.TagName == t
			//	//			)
			//	//		)
			//	//	);
			//	
			//	Dictionary<GlowType, int> glowCounts = GlowType.GlowTypes.ToDictionary(
			//		keySelector: glowType => glowType,
			//		elementSelector: glowType => taggedBeats.TaggedBeats.Sum(
			//			beat => beat.Tags.Count(tag => tag == glowType.TagName)
			//		)
			//	);
			//	
			//	//Tried doing this part functionally with linq but it really didn't work well
			//	//and needed other foreaches beforehand and stuff
			//	//Instead, just doing it iteratively
			//	var sb = new StringBuilder(); //use instead of Console.WriteLine so we can easily delete
			//	                              //final separator characters and stuff like that (and for efficiency)
			//	
			//	sb.Append("-filter_complex \"");
			//	for (int glowNum = 0; glowNum < GlowType.GlowTypes.Count; glowNum++)
			//	{
			//		var glowType = GlowType.GlowTypes[glowNum];
			//		var glowCount = glowCounts[glowType];
			//	
			//		sb.Append($"[{glowNum + 1}:v]split={glowCounts[glowType]}");
			//	
			//		for (int i = 0; i < glowCount; i++) {
			//			sb.Append($"[glow{glowType.GlowLetters}{i}]");
			//		}
			//	
			//		sb.Append("; ");
			//	}
			//	//Don't need to remove last separator as it's needed to separate the previous section from the next section
			//	
			//	var glowIndices = GlowType.GlowTypes.ToDictionary(keySelector: g => g, elementSelector: g => 0);
			//	int watermarkIndex = 0;
			//	foreach (var beat in taggedBeats.TaggedBeats)
			//	{
			//		GlowType glowType = null;
			//		foreach (var tag in beat.Tags) {
			//			glowType = GlowType.GlowTypes.FirstOrDefault(g => g.TagName == tag);
			//			if (glowType != null) break;
			//		}
			//		if (glowType == null) continue;
			//	
			//		var fadeInTime = GetTagValue(beat, fadeInTimeTag, defaultGlowFadeIn, (string x, out double val) => double.TryParse(x, out val));
			//		var fadeOutTime = GetTagValue(beat, fadeOutTimeTag, defaultGlowFadeOut, (string x, out double val) => double.TryParse(x, out val));
			//	
			//		sb.Append(
			//			$"[glow{glowType.GlowLetters}{glowIndices[glowType]}]"
			//			+ $"fade=in:st={beat.Beat - fadeInTime}:d={fadeInTime}"
			//			+ $","
			//			+ $"fade=out:st={beat.Beat}:d={fadeOutTime}"
			//			+ $"[watermark{watermarkIndex}]"
			//		);
			//	
			//		sb.Append("; ");
			//	
			//		glowIndices[glowType] += 1;
			//		watermarkIndex += 1;
			//	}
			//	//Don't need to remove last separator as it's needed to separate the previous section from the next section
			//	
			//	int watermarkCount = watermarkIndex;
			//	
			//	string prevVidStreamName = "[v:0]";
			//	for (int i = 0; i < watermarkCount; i++)
			//	{
			//		sb.Append(
			//			String.Concat(
			//				prevVidStreamName,
			//				$"[watermark{i}]",
			//				$"overlay=25:25",
			//				$",",
			//				$"shortest=1"
			//			)
			//		);
			//	
			//		prevVidStreamName = i < (watermarkCount - 1) ? $"[tmp{i}]" : "";
			//	}
			//	
			//	sb.Append("\"");
			//	
			//	Console.WriteLine(sb.ToString());
			//	
			//	//	Console.WriteLine(
			//	//		String.Concat(
			//	//			,
			//	//			String.Join(
			//	//				"; ",
			//	//				taggedBeats.TaggedBeats.Aggregate(
			//	//					seed: ,
			//	//					func: (currGlowIndices, beat) => String.Join(
			//	//						"; ",
			//	//						from tag in beat.Tags
			//	//						let glowType = GlowType.GlowTypes.FirstOrDefault(g => g.TagName == tag)
			//	//						where glowType != null
			//	//						let nextGlowIndex = (currGlowIndices[glowType] += 1)
			//	//						select 
			//	//					)
			//	//				)
			//	//			)
			//	//			"\""
			//	//		)
			//	//	);
			//	
			//	Console.WriteLine("-pix_fmt yuv420p");
			//	Console.WriteLine("-c:a copy");
			//	Console.WriteLine($@"{mainFolder}\Meme Video 5.3 - " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".mp4");
			//	
			//	Console.ReadLine();
		}

		//	private static T GetTagValue<T>(TaggedBeat taggedBeat, string tagName, TagValueTryParser<T> parser)
		//	{
		//		Exception exception = TryGetTagValue(taggedBeat, tagName, parser, out T value);
		//		return exception == null ? value : throw exception;
		//	}
		//	
		//	private static T GetTagValue<T>(TaggedBeat taggedBeat, string tagName, T @default, TagValueTryParser<T> parser)
		//	{
		//		Exception exception = TryGetTagValue(taggedBeat, tagName, parser, out T value);
		//		return exception == null ? value : @default;
		//	}
		//	
		//	/// <summary>
		//	/// Returns null on success, and different exceptions on failure depending on the kind of failure.
		//	/// Throws exceptions for null arguments.
		//	/// </summary>
		//	private static Exception TryGetTagValue<T>(TaggedBeat taggedBeat, string tagName, TagValueTryParser<T> parser, out T value)
		//	{
		//		if (tagName == null) throw new ArgumentNullException(nameof(tagName));
		//		if (parser == null) throw new ArgumentNullException(nameof(parser));
		//	
		//		if (!tagName.EndsWith(":")) tagName += ":";
		//	
		//		string fullTag = taggedBeat.Tags.FirstOrDefault(t => t.StartsWith(tagName));
		//	
		//		if (fullTag == null) {
		//			value = default(T);
		//			return new ArgumentException(
		//				"No tag with name '" + tagName + "' (':' may have been added) "
		//				+ "could be found in " + nameof(TaggedBeat) + " '" + taggedBeat.ToString() + "'."
		//			);
		//		}
		//	
		//		string valueStr = fullTag.Substring(startIndex: tagName.Length);
		//	
		//		if (parser(valueStr, out value)) {
		//			return null;
		//		} else {
		//			return new ArgumentException(
		//				"tag '" + fullTag + "' "
		//				+ "with value string '" + valueStr + "' "
		//				+ "could not be parsed as type '" + typeof(T).FullName + "'."
		//			);
		//		}
		//	}
	}

	delegate bool TagValueTryParser<T>(string str, out T value);
}

//*/