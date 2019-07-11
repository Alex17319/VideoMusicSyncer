using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace VideoMusicSyncer
{
	public class IdGenerator
	{
		private int _lastID;

		public int FirstID { get; }
		public Func<int, int> Stepper { get; }

		public IdGenerator(int firstID, Func<int, int> stepper)
		{
			this.FirstID = firstID;
			this.Stepper = stepper ?? throw new ArgumentNullException(nameof(stepper));

			this._lastID = firstID;
		}

		/// <summary>
		/// Returns the first ID, then the next, then the next, and so on.
		/// </summary>
		public int GetNextID()
		{
			//On the first call, need to return the first ID before advancing. Just stick with this for all calls
			var res = _lastID;
			_lastID = Stepper(_lastID);
			return res;
		}
	}

	public class SequentialIdGenerator : IdGenerator
	{
		public SequentialIdGenerator(int firstID)
			: base(firstID, x => x + 1)
		{ }
	}
}

//*/