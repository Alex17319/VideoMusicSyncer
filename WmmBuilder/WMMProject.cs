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
using System.Xml.Linq;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;

namespace VideoMusicSyncer.WmmCommandBuider
{
	public class WmmProject
	{
		public const int DefaultVersion = 65540;
		public const int DefaultThemeID = 0;
		public const string DefaultTemplateID = "SimpleProjectTemplate";

		public string ProjectName { get; }
		public IEnumerable<WmmMediaItem> MediaItems { get; }
		public IEnumerable<WmmExtent> Extents { get; }
		public int Version { get; }
		public int ThemeID { get; }
		public string TemplateID { get; }

		
		private WmmProject(
			string projectName,
			IEnumerable<WmmMediaItem> mediaItems,
			IEnumerable<WmmExtent> extents,
			int version = DefaultVersion,
			int themeID = DefaultThemeID,
			string templateID = "SimpleProjectTemplate"
		) {
			if (string.IsNullOrEmpty(projectName)) throw new ArgumentException("Cannot be null or empty", nameof(projectName));
			if (version < 0) throw new ArgumentOutOfRangeException(nameof(version), version, "Cannot be negative");

			this.ProjectName = projectName;
			this.MediaItems = mediaItems ?? Enumerable.Empty<WmmMediaItem>();
			this.Extents = extents ?? Enumerable.Empty<WmmExtent>();
			this.Version = version;
			this.ThemeID = themeID;
			this.TemplateID = templateID;
		}

		/// <summary>
		/// WARNING: This method does NOT set up the addtional extents correctly, and unless you do that yourself,
		/// the project will be considered corrupt by WMM. It is recommended that you use a different method.
		/// </summary>
		public static WmmProject CreateRaw(
			string projectName,
			IEnumerable<WmmMediaItem> mediaItems,
			IEnumerable<WmmExtent> extents,
			int version = DefaultVersion,
			int themeID = DefaultThemeID,
			string templateID = "SimpleProjectTemplate"	
		){
			return new WmmProject(
				projectName: projectName,
				mediaItems : mediaItems ,
				extents    : extents    ,
				version    : version    ,
				themeID    : themeID    ,
				templateID : templateID
			);
		}

		public static WmmProject Create(
			string projectName,
			IEnumerable<WmmMediaItem> mediaItems,
			IEnumerable<WmmVideoClip> videoClips,
			IEnumerable<WmmAudioClip> audioClips,
			int version = DefaultVersion,
			int themeID = DefaultThemeID,
			string templateID = "SimpleProjectTemplate"
		) {
			//These are iterated over multiple times here (in called methods)
			//Ensure they do not change between iterations (eg. if the extent ids are generated again)
			videoClips = videoClips.ToArray();
			audioClips = audioClips.ToArray();

			return new WmmProject(
				projectName: projectName,
				mediaItems: mediaItems,
				extents: (
					Enumerable.SelectMany( //Concat more than 2 collections
						new IEnumerable<WmmExtent>[] {
							videoClips,
							audioClips,
							new WmmExtent[] {
								new WmmExtentSelector(
									extentID: ExtentIdGenerator.ExtentSelector1ExtentID,
									primaryTrack: true,
									extentRefIDs: videoClips.Select(v => v.ExtentID)
								),
								new WmmExtentSelector(
									extentID: ExtentIdGenerator.ExtentSelector2ExtentID,
									primaryTrack: false,
									extentRefIDs: audioClips.Select(a => a.ExtentID)
								),
								new WmmExtentSelector( //Don't really know what this is for but it's needed
									extentID: ExtentIdGenerator.ExtentSelector3ExtentID,
									primaryTrack: false,
									extentRefIDs: Enumerable.Empty<int>()
								),
								new WmmExtentSelector( //Don't really know what this is for but it's needed
									extentID: ExtentIdGenerator.ExtentSelector4ExtentID,
									primaryTrack: false,
									extentRefIDs: Enumerable.Empty<int>()
								),
							}
						},
						x => x
					)
				)
			);
		}

		public XDocument ToXml()
		{
			return new XDocument(
				new XDeclaration("1.0", "utf-8", standalone: "yes"),
				new XE(
					"Project",
					new XA("name", this.ProjectName),
					new XA("themeID", this.ThemeID),
					new XA("version", this.Version),
					new XA("templateID", this.TemplateID),
					new XE(
						"MediaItems",
						from m in this.MediaItems
						select m.ToXml()
					),
					new XE(
						"Extents",
						from e in this.Extents
						select e.ToXml()
					),
					new XE(
						"BoundPlaceholders",
						new XE("BoundPlaceholder", new XA("placeholderID", "SingleExtentView"), new XA("extentID", 0)),
						new XE("BoundPlaceholder", new XA("placeholderID", "Main"), new XA("extentID", 1)),
						new XE("BoundPlaceholder", new XA("placeholderID", "SoundTrack"), new XA("extentID", 2)),
						new XE("BoundPlaceholder", new XA("placeholderID", "Narration"), new XA("extentID", 3)),
						new XE("BoundPlaceholder", new XA("placeholderID", "Text"), new XA("extentID", 4))
					),
					new XE(
						"BoundProperties",
						new XE(
							"BoundPropertyFloatSet",
							new XA("Name", "AspectRatio"),
							new XE(
								"BoundPropertyFloatElement",
								new XA("Value", "1.7777776718139648")
							)
						),
						new XE("BoundPropertyFloat", new XA("Name", "DuckedNarrationAndSoundTrackMix"), new XA("Value", 1)),
						new XE("BoundPropertyFloat", new XA("Name", "DuckedVideoAndNarrationMix"), new XA("Value", 0)),
						new XE("BoundPropertyFloat", new XA("Name", "DuckedVideoAndSoundTrackMix"), new XA("Value", 0.5)),
						new XE("BoundPropertyFloat", new XA("Name", "SoundTrackMix"), new XA("Value", 0.5))
					),
					new XE(
						"ThemeOperationLog",
						new XA("themeID", 0),
						new XE("MonolithicThemeOperations")
					),
					new XE(
						"AudioDuckingProperties",
						new XA("emphasisPlaceholderID", "Narration")
					)
				)
			);
		}
	}
}

//*/