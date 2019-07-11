using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoMusicSyncer.VideoGlowOverlay
{
	public class GlowType
	{
		//public readonly string TagName;
		public readonly FileInfo File;

		/// <summary>
		/// Note: In order for FFMpeg to fade the glow in and out correctly,
		/// the image must have at least some transparency somewhere (and this might need to
		/// be within the region that ends up in the resulting video). Otherwise,
		/// FFMpeg adds in a black background to the fade effects or something.
		/// </summary>
		public GlowType(FileInfo file) //, string tagName)
		{
			ErrorUtils.ThrowIfArgNull(file, nameof(file));
			//ErrorUtils.ThrowIfArgNull(tagName, nameof(tagName));

			this.File = file;
			//this.TagName = tagName;
		}

		public static bool Equals(GlowType a, GlowType b) => a?.File.FullName == b?.File.FullName;
		public override bool Equals(object obj) => obj is GlowType g && Equals(this, g);
		public static bool operator ==(GlowType a, GlowType b) => Equals(a, b);
		public static bool operator !=(GlowType a, GlowType b) => !(a == b);

		public override int GetHashCode() => this.File.FullName.GetHashCode();

		public override string ToString()
		{
			return (
				$"{{"
					+ $"[{nameof(GlowType)}] "
					//+ $"{nameof(GlowNum)}: {GlowNum}, "
					+ $"{nameof(File)}: {File} "
					//+ $"{nameof(TagName)}: {TagName}"
				+ $"}}"
			);
		}
	}
}

//*/