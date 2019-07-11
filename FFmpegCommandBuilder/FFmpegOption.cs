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

namespace VideoMusicSyncer.FFmpegCommandBuilder
{
	public class FFmpegOption
	{
		public virtual string Name { get; }
		/// <summary>
		/// This must include any needed surrounding quotation marks - it is not automatically enquoted
		/// </summary>
		public virtual string Value { get; }

		protected FFmpegOption() { }

		public FFmpegOption(string name, string value = "")
		{
			ErrorUtils.ThrowIfArgNull(name, nameof(name));

			this.Name = name;
			this.Value = value ?? "";
		}

		public override string ToString()
		{
			return "-" + Name + (string.IsNullOrEmpty(Value) ? "" : (" " + Value));
		}
	}
}

//*/