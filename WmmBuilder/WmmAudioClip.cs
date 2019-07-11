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
	public class WmmAudioClip : WmmExtent
	{
		public override string XmlElementName => "AudioClip";

		protected override XE BoundProperties => new XE(
			"BoundProperties",
			new XE("BoundPropertyBool", new XA("Name", "Mute"), new XA("Value", "false")),
			new XE("BoundPropertyFloat", new XA("Name", "Volume"), new XA("Value", "1"))
		);

		public int MediaItemID { get; }
		public double Speed { get; }
		/// <summary>The position (in the clip) that the clip plays from</summary>
		public double InTime { get; }
		/// <summary>The position (in the clip) that the clip stops playing at</summary>
		public double OutTime { get; }

		/// <param name="inTime">The position (in the clip) that the clip plays from</param>
		/// <param name="outTime">The position (in the clip) that the clip stops playing at</param>
		/// <param name="extentID">Should be generated using an <see cref="ExtentIdGenerator"/></param>
		public WmmAudioClip(int extentID, int mediaItemID, double inTime, double outTime, double speed)
			: base(extentID)
		{
			if (mediaItemID < 1) throw new ArgumentOutOfRangeException(nameof(mediaItemID), mediaItemID, "Cannot be less than 1.");
			if (inTime < 0) throw new ArgumentOutOfRangeException(nameof(inTime), inTime, "Cannot be less than 0.");
			if (outTime < 0) throw new ArgumentOutOfRangeException(nameof(outTime), outTime, "Cannot be less than 0.");
			if (speed < 0) throw new ArgumentOutOfRangeException(nameof(speed), speed, "Cannot be less than 0.");
			
			this.MediaItemID = mediaItemID;
			this.InTime = inTime;
			this.OutTime = outTime;
			this.Speed = speed;
		}

		public override XE ToXml()
		{
			var res = base.ToXml();
			res.Add(
				new XA("mediaItemID", this.MediaItemID),
				new XA("inTime", this.InTime),
				new XA("outTime", this.OutTime),
				new XA("speed", this.Speed)
			);
			return res;
		}
	}
}

//*/