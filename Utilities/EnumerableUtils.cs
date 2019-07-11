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
	public static class EnumerableUtils
	{
		/// <summary>
		/// Splits a collection into a series of collections of the provided length (<paramref name="chunkSize"/>).
		/// The last collection in the series may end up shorter.
		/// </summary>
		/// <param name="counter">
		/// Can be used to make elements have a count other than 1 when determining how
		/// much of a chunk they make up. When this is used the chunkSize may be exceeded for some chunks.
		/// </param>
		public static IEnumerable<ReadOnlyCollection<T>> Chunk<T>(
			this IEnumerable<T> source,
			int chunkSize,
			Func<T, int> counter = null
		) {
			ErrorUtils.ThrowIfArgLessThan(chunkSize, 1, nameof(chunkSize));

			List<T> chunk = new List<T>(capacity: chunkSize);
			int chunkItemIndex = 0;

			foreach (var item in source)
			{
				if (chunkItemIndex >= chunkSize)
				{
					yield return chunk.AsReadOnly();
					chunk = new List<T>(capacity: chunkSize);
				}

				chunk.Add(item);

				chunkItemIndex += counter?.Invoke(item) ?? 1;
			}

			if (chunk.Count > 0) yield return chunk.AsReadOnly();
		}

		public static IEnumerable<(T x, int i)> Index<T>(this IEnumerable<T> source)
			=> source.Select((x, i) => (x, i));

		public static IEnumerable<T?> AsNullable<T>(this IEnumerable<T> source)
			where T : struct
			=> source.Select(x => new T?(x));

		public static bool TryGetLast<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T last)
		{
			bool anyMatched = false;
			last = default;
			foreach (var elem in source)
			{
				if (predicate(elem)) {
					last = elem;
					anyMatched = true;
				} else {
					return anyMatched;
				}
			}
			return anyMatched;
		}

		public static bool TryGetFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T first)
		{
			foreach (var elem in source)
			{
				if (predicate(elem)) {
					first = elem;
					return true;
				} else {
					first = default;
					return false;
				}
			}
			first = default;
			return false;
		}

		
	}
}

//*/