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

namespace VideoMusicSyncer
{
	public class StringUtils
	{
		public static string JoinNonNull(string separator, params object[] values)
			=> string.Join(separator, values.Where(x => x != null));
	}
}

//*/