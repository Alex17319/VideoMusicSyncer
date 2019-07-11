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
	public class WmmMediaItem
	{
		public string FilePath { get; }
		public int MediaItemID { get; }
		public WmmMediaType MediaType { get; }

		public WmmMediaItem(string filePath, int mediaItemID, WmmMediaType mediaType)
		{
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("Cannot be null or empty", nameof(filePath));
			if (mediaItemID < 1) throw new ArgumentOutOfRangeException(nameof(mediaItemID), mediaItemID, "Cannot be less than 1");

			this.FilePath = filePath;
			this.MediaItemID = mediaItemID;
			this.MediaType = mediaType;
		}

		public XE ToXml()
		{
			return new XE(
				"MediaItem",
				new XA("id", this.MediaItemID),
				new XA("filePath", this.FilePath),
				new XA("arWidth", "0"), // Fixed my WMM automatically
				new XA("arHeight", "0"), // ''
				new XA("duration", "0"), // ''
				new XA("songTitle", ""), // Blank is fine
				new XA("songArtist", ""), // ''
				new XA("songAlbum", ""), // ''
				new XA("songCopyrightUrl", ""), // ''
				new XA("songArtistUrl", ""), // ''
				new XA("songAudioFileUrl", ""), // ''
				new XA("stabilizationMode", "0"),
				new XA("mediaItemType", (int)this.MediaType)
			);
		}
	}
}

//*/