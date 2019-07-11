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
	public class WmmExtentSelector : WmmExtent
	{
		public override string XmlElementName => "ExtentSelector";

		public bool PrimaryTrack { get; }
		public IEnumerable<int> ExtentRefIDs { get; }

		/// <param name="extentID">Should be one of the constants in <see cref="ExtentIdGenerator"/></param>
		public WmmExtentSelector(int extentID, bool primaryTrack, IEnumerable<int> extentRefIDs)
			: base(extentID)
		{
			this.PrimaryTrack = primaryTrack;
			this.ExtentRefIDs = extentRefIDs ?? Enumerable.Empty<int>();
		}

		public override XE ToXml()
		{
			var res = base.ToXml();
			res.Add(
				new XA("primaryTrack", this.PrimaryTrack),
				new XE(
					"ExtentRefs",
					from refID in this.ExtentRefIDs
					select new XE(
						"ExtentRef",
						new XA("id", refID)
					)
				)
			);
			return res;
		}
	}
}

//*/