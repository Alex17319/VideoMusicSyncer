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
			//TODO: ^ think this should actually be 37.675

			const double beatInterval = (
				//1st manual counting attempt:
				//	0.26077241379310344827586206896552 //(75.177 - 37.365)/145 (my own measurements & counting - I think I miscounted or something)
				//Online lookup:
				//	128 BPM.
				//	Results in interval of 0.46875s (exactly)
				//	Based on:
				//	 - https://www.audiokeychain.com/track/tgi/laidback-luke-peking-duk-mufasa
				//	 - https://tunebat.com/Info/Mufasa-Original-Mix-Laidback-Luke-Peking-Duck/49mTyByYvtaQwedg3VM6ky
				//	 - https://www.beatport.com/track/mufasa-original-mix/6319471
				//Due to possible innacuraccy in the results, I then did some more manual measurements:
				//2nd manual counting attempt:
				//	76 beats in 44.918 seconds = 101.5183222761476 bpm = interval of 0.59102631578947s //Completely wrong, it was meant to be 96 beats
				//3rd manual counting attempt:
				//	96 beats in 44.914 seconds (37.673...82.588) = 128.24509061762479405085274079352 bpm = interval of 0.46785416666666666666666666666665s
				//1st attempt checking the bpm of the build up of the song:
				//	80 beats in 37.495 seconds (00.178...37.673) = 128.0170689425256700893452460328 bpm
				//	.'. should be safe to assume bpm is constant throughout song (unless results differ significantly)
				//4th manual counting attempt:
				//	212 beats in 99.444 seconds (82.588...182.032) = 127.91118619524556534330879691082 bpm = interval of 0.46907547169811320754716981132077s
				//5th manual counting attempt:
				//	212 + 96 + 80 = 388 beats in 181.854 seconds (00.178...182.032) = 128.01478108812563924906793361708 bpm = interval of 0.46869587628865979381443298969071
				0.46869587628865979381443298969071 //5th manual count
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
			var GlowTransparentRedCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-red-opacity130.png")));
			var GlowTransparentBlueCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-blue-opacity130.png")));
			var GlowTransparentGreenCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-green-opacity130.png")));
			var GlowTransparentYellowCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-yellow-opacity170.png")));
			var GlowTransparentPurpleCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-purple-opacity150.png")));
			var GlowTransparentOrangeCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-orange-opacity170.png")));
			var GlowTransparentLightblueCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-lightblue-opacity180.png")));
			var GlowTransparentDarkgreenCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-darkgreen-opacity170.png")));

			var GlowRedCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-red.png")));
			var GlowBlueCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-blue.png")));
			var GlowGreenCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-green.png")));
			var GlowYellowCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-yellow.png")));
			var GlowPurpleCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-purple.png")));
			var GlowOrangeCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-orange.png")));
			var GlowLightblueCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-lightblue.png")));
			var GlowDarkgreenCircle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-darkgreen.png")));

			var GlowSemitransparentWhiteRectangle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-white-rectangle-opacity230.png")));
			var GlowTransparentWhiteRectangle = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-circlular-white-rectangle-opacity200.png")));

			//var GlowDiamond = new GlowType(new FileInfo(Path.Combine(mainFolder, "glow-diamond-white.png")));

			//	colorMixer: new FFmpegColorChannelMixer(
			//		redPart: (1, 1, 1, 0),
			//		greenPart: (0, 0, 0, 0),
			//		bluePart: (0, 0, 0, 0),
			//		alphaPart: (0, 0, 0, 0.5)
			//	),

			//Note:
			//  1 third of a beat = 0.15625
			//  To prevent a continuous overlap in the section with repeated beats 1 third of a beat apart,
			//  set the synth beat glow durations to, at most, 0.156 seconds (in total, ie. fade in + fade out)
			//Note:
			//  If a fade in or out time is zero - even just one, not both - the fade doesn't show at all.
			//  I've modified Glow() to handle this.
			Glow fullBeatEven            = new Glow(GlowSemitransparentWhiteRectangle , 0.001, 0.2 , x: 0.5, y: 0.5);
			Glow fullBeatOdd             = new Glow(GlowSemitransparentWhiteRectangle , 0.001, 0.2 , x: 0.5, y: 0.5);
			Glow leftSynthBeatRed        = new Glow(GlowRedCircle      , 0.001, 0.15, x: 0  , y: 0  );
			Glow leftSynthBeatBlue       = new Glow(GlowBlueCircle     , 0.001, 0.15, x: 0  , y: 0  );
			Glow leftSynthBeatGreen      = new Glow(GlowGreenCircle    , 0.001, 0.15, x: 0  , y: 0  );
			Glow leftSynthBeatYellow     = new Glow(GlowYellowCircle   , 0.001, 0.15, x: 0  , y: 0  );
			Glow leftSynthBeatPurple     = new Glow(GlowPurpleCircle   , 0.001, 0.15, x: 0  , y: 0  );
			Glow leftSynthBeatOrange     = new Glow(GlowOrangeCircle   , 0.001, 0.15, x: 0  , y: 0  );
			Glow leftSynthBeatLightblue  = new Glow(GlowLightblueCircle, 0.001, 0.15, x: 0  , y: 0  );
			Glow leftSynthBeatDarkgreen  = new Glow(GlowDarkgreenCircle, 0.001, 0.15, x: 0  , y: 0  );
			Glow rightSynthBeatRed       = new Glow(GlowRedCircle      , 0.001, 0.15, x: 1  , y: 0  );
			Glow rightSynthBeatBlue      = new Glow(GlowBlueCircle     , 0.001, 0.15, x: 1  , y: 0  );
			Glow rightSynthBeatGreen     = new Glow(GlowGreenCircle    , 0.001, 0.15, x: 1  , y: 0  );
			Glow rightSynthBeatYellow    = new Glow(GlowYellowCircle   , 0.001, 0.15, x: 1  , y: 0  );
			Glow rightSynthBeatPurple    = new Glow(GlowPurpleCircle   , 0.001, 0.15, x: 1  , y: 0  );
			Glow rightSynthBeatOrange    = new Glow(GlowOrangeCircle   , 0.001, 0.15, x: 1  , y: 0  );
			Glow rightSynthBeatLightblue = new Glow(GlowLightblueCircle, 0.001, 0.15, x: 1  , y: 0  );
			Glow rightSynthBeatDarkgreen = new Glow(GlowDarkgreenCircle, 0.001, 0.15, x: 1  , y: 0  );



			var taggedBeats = new BeatTagSequence(
				beatPositioner: beats,
				firstBeatTags: ImmutableList.Create<object>(fullBeatEven, leftSynthBeatRed, rightSynthBeatRed)
			)
			.SkipByBeats(2.5/3.0      ).TagAndAdd(leftSynthBeatBlue)
			.SkipByBeats(0.25/3.0     ).TagAndAdd(rightSynthBeatBlue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatGreen)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatBlue)
			//Prev: duhh, drrruhh du du duhh
			//Next: duhh, duh boof, duh boof, duh boof, duh boof
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatYellow, rightSynthBeatYellow)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatPurple)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(rightSynthBeatGreen)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatBlue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(rightSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//Prev: duhh, duh boof, duh boof, duh boof, duh boof
			//Next: duh duhh, drrruh du du duhh du du duhh
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatLightblue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, rightSynthBeatLightblue)
			.SkipByBeats(2.5/3.0      ).TagAndAdd(leftSynthBeatGreen)
			.SkipByBeats(0.25/3.0     ).TagAndAdd(rightSynthBeatGreen)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatPurple)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatPurple)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatPurple)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatPurple)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatGreen)
			//Prev: duh duhh, drrruh du du duhh du du duhh
			//Next: duhh, boof, duh duhh, duh duhh--
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatRed, rightSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatYellow, rightSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatGreen, rightSynthBeatGreen)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatYellow, rightSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatGreen, rightSynthBeatGreen)
			//Prev: duhh, boof, duh duhh, duh duhh--
			//Next: --duh duhh, drrruh du du duhh
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatYellow, rightSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatGreen, rightSynthBeatGreen)
			.SkipByBeats(2.5/3.0      ).TagAndAdd(leftSynthBeatRed)
			.SkipByBeats(0.25/3.0     ).TagAndAdd(rightSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatBlue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatRed)
			//Prev: --duh duhh, drrruh du du duhh
			//Next: duhh, duh boof, duh boof, duh boof, duh boof
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatLightblue, rightSynthBeatLightblue)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatPurple)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatBlue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatGreen)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd)
			//Prev: duhh, duh boof, duh boof, duh boof, duh boof
			//Next: duh duhh, drrruh du du duhh du du duhh
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatRed, rightSynthBeatRed)
			.SkipByBeats(2.5/3.0      ).TagAndAdd(leftSynthBeatLightblue)
			.SkipByBeats(0.25/3.0     ).TagAndAdd(rightSynthBeatLightblue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatLightblue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatPurple)
			//Prev: duh duhh, drrruh du du duhh du du duhh
			//Next: duhh, boof, duh duhh, duh duhh, duh-- (iighiahhh iighiahhh iighiahhh iighiahhh iighiahhh)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatRed.With(fadeOutTime: 0.6), rightSynthBeatRed.With(fadeOutTime: 0.6))
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatRed.With(fadeOutTime: 0.6), rightSynthBeatRed.With(fadeOutTime: 0.6))
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatRed) //looks better with  //leave these out for extra emphasis of the other beats
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatRed.With(fadeOutTime: 0.6), rightSynthBeatRed.With(fadeOutTime: 0.6))
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatRed) //looks better with  //leave these out for extra emphasis of the other beats
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatRed.With(fadeOutTime: 0.6), rightSynthBeatRed.With(fadeOutTime: 0.6))
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatRed) //looks better with //leave these out for extra emphasis of the other beats
			//Prev: duhh, boof, duh duhh, duh duhh, duh (iighiahhh iighiahhh iighiahhh iighiahhh iighiahhh)
			//Next: duhh du du duhh du du duhh du du duhh du du (#1)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatOrange)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatOrange)
			//Prev: duhh du du duhh du du duhh du du duhh du du (#1)
			//Next: duhh du du duhh du du duhh du du duhh du du (#2)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatOrange)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatOrange)
			//Prev: duhh du du duhh du du duhh du du duhh du du (#2)
			//Next: duhh du du duhh du du duhh du du duhh du du (#3)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, rightSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatGreen)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, rightSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatGreen)
			//Prev: duhh du du duhh du du duhh du du duhh du du (#3)
			//Next: duhh du du duhh du du duhh du du duhh du du (#4)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatBlue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, rightSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatBlue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, rightSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(rightSynthBeatRed)
			//Prev: duhh du du duhh du du duhh du du duhh du du (#4)
			//Next: duhh du du duhh du du duhh du du duhh du du (#5)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatLightblue, rightSynthBeatLightblue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatBlue, rightSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatBlue, rightSynthBeatBlue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatYellow, rightSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange, rightSynthBeatOrange)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatOrange, rightSynthBeatOrange)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatGreen, rightSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatDarkgreen, rightSynthBeatDarkgreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatDarkgreen, rightSynthBeatDarkgreen)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatRed, rightSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatRed)
			//Prev: duhh du du duhh du du duhh du du duhh du du (#5)
			//Next: duhh du du duhh du du duhh du du duhh du du (#6)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatBlue, rightSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatYellow, rightSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatBlue, rightSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatYellow, rightSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatYellow, rightSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatRed, rightSynthBeatPurple)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatPurple, rightSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatPurple)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatLightblue, rightSynthBeatGreen)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatGreen, rightSynthBeatLightblue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatLightblue, rightSynthBeatGreen)
			//Prev: duhh du du duhh du du duhh du du duhh du du (#6)
			//Next: duhh du du duhh du du duhh, duhh du du duhh, duhh boof, mu fa, sa boof, duh BOOF.
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatRed, leftSynthBeatRed, rightSynthBeatPurple, rightSynthBeatPurple)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatPurple, rightSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatPurple, rightSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatYellow, rightSynthBeatRed)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatYellow)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatRed, rightSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatYellow, rightSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd, leftSynthBeatBlue, rightSynthBeatPurple)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatPurple, rightSynthBeatBlue)
			.SkipByBeats(1.0/3.0      ).TagAndAdd(leftSynthBeatPurple, rightSynthBeatBlue)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatBlue, rightSynthBeatPurple)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatRed, leftSynthBeatRed, rightSynthBeatRed, rightSynthBeatRed)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(fullBeatEven, leftSynthBeatLightblue, rightSynthBeatGreen) //mu
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatEven, leftSynthBeatGreen, rightSynthBeatLightblue) //fa
			.SkipByBeats(2.0/3.0      ).TagAndAdd(fullBeatEven, leftSynthBeatLightblue, rightSynthBeatGreen) //sa
			.SkipToBeat (Beat.NextBeat).TagAndAdd(fullBeatOdd)
			.SkipByBeats(2.0/3.0      ).TagAndAdd(leftSynthBeatYellow, rightSynthBeatYellow)
			.SkipToBeat (Beat.NextBeat).TagAndAdd(
				fullBeatEven,
				leftSynthBeatRed.With(fadeOutTime: 0.6),
				rightSynthBeatRed.With(fadeOutTime: 0.6),
				leftSynthBeatRed.With(fadeOutTime: 0.6),
				rightSynthBeatRed.With(fadeOutTime: 0.6),
				leftSynthBeatYellow.With(fadeOutTime: 0.6),
				rightSynthBeatPurple.With(fadeOutTime: 0.6)
			);
			//Prev: duhh du du duhh du du duhh, duhh du du duhh, duhh boof, mu fa, sa boof, duh BOOF.
			
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
					approxGlowsPerSubstage: 5
				)
			);

			

			Console.ReadLine();
		}
	}
}
