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
	public abstract class WmmExtent
	{
		public abstract string XmlElementName { get; }
		protected virtual XE Effects => new XE("Effects");
		protected virtual XE Transitions => new XE("Transitions");
		protected virtual XE BoundProperties => new XE("BoundProperties");

		public int ExtentID { get; }

		/// <param name="extentID">
		/// Should either be generated using an <see cref="ExtentIdGenerator"/>
		/// or be one of the constants in that class.
		/// </param>
		public WmmExtent(int extentID)
		{
			if (extentID < 1) throw new ArgumentOutOfRangeException(nameof(extentID), extentID, "Cannot be less than 1.");

			this.ExtentID = extentID;
		}

		public virtual XE ToXml()
		{
			return new XE(
				XmlElementName,
				new XA("extentID", ExtentID),
				new XA("gapBefore", "0"),
				this.Effects,
				this.Transitions,
				this.BoundProperties
			);
		}
	}
}

//*/