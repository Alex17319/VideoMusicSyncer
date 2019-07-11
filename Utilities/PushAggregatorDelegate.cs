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
	/// <summary>
	/// A function, generally implemented as an iterator, that takes provided source elements
	/// of a collection and returns an aggregate for each element along the way.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TAggregate"></typeparam>
	/// <param name="input">
	/// A view that switches between each element of the source collection,
	/// changing to the next one after each step of the iterator.
	/// </param>
	/// <returns>
	/// An aggregate after each element, optionally setting isValid to false if
	/// an aggregate cannot be returned after certain elements. An aggregate is
	/// considered valid if it is valid for the sub-collection observed so far.
	/// <para/>
	/// Note: Returning an aggregate for a functions like max(), min(), count(), etc when only the first
	/// few elements have been looked at, IS considered valid, as it is valid for the sub-collection examined
	/// so far.
	/// </returns>
	public delegate IEnumerator<(TAggregate currentAgg, bool isValid)> PushAggregatorFunc<T, TAggregate>(ItemView<T> input);
}

//*/