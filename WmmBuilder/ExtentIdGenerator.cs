using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace VideoMusicSyncer.WmmCommandBuider
{
	public class ExtentIdGenerator : SequentialIdGenerator
	{
		public const int ExtentSelector1ExtentID = 1;
		public const int ExtentSelector2ExtentID = 2;
		public const int ExtentSelector3ExtentID = 3;
		public const int ExtentSelector4ExtentID = 4;

		/// <param name="firstID">
		/// Must be at least 5 as IDs 1 to 4 are taken by the extent selectors
		/// (WMM considers the project corrupt if these IDs are changed)
		/// </param>
		public ExtentIdGenerator(int firstID = 5) : base(firstID: firstID)
		{
			if (firstID < 5) throw new ArgumentOutOfRangeException(
				nameof(firstID),
				firstID,
				"Must be greater than 5 as IDs 1 to 4 are taken by the extent selectors "
				+ "(WMM considers the project corrupt if these IDs are changed)."
			);
		}
	}
}

//*/