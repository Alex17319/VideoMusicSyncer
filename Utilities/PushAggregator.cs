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
	public class PushAggregator<T, TAggregate> : IDisposable
	{
		private IPushingEnumerator<T> _sourceEnumerator;
		private PushAggregatorFunc<T, TAggregate> _iterator;

		private ItemView<T>.Editor _iteratorInput;

		private IEnumerator<(TAggregate currentAgg, bool isValid)> _iteratorResults;

		public TAggregate CurrentAggregate { get; private set; } = default;
		public bool CurrentAggregateIsValid { get; private set; } = false;
		public bool Finished { get; private set; } = false;

		public bool Disposed => _sourceEnumerator == null;

		/// <summary>
		/// Returns <see cref="CurrentAggregate"/> if <see cref="CurrentAggregateIsValid"/>
		/// is true, otherwise throws an exception
		/// </summary>
		public TAggregate CurrentResult => (
			CurrentAggregateIsValid
			? CurrentAggregate
			: throw new InvalidOperationException(
				"The current aggregate '" + CurrentAggregate + "' is not valid, "
				+ "so no current valid aggregate can be returned."
			)
		);
		/// <summary>
		/// Returns <see cref="CurrentAggregate"/> if <see cref="CurrentAggregateIsValid"/>
		/// and <see cref="Finished"/> are both true, otherwise throws an exception
		/// </summary>
		public TAggregate Result => (
			CurrentAggregateIsValid
			? Finished
				? CurrentAggregate
				: throw new InvalidOperationException(
					"The end of the collection has not been reached, nor has the aggregator finished processing, "
					+ "so no final aggregate can be returned."
				)
			: throw new InvalidOperationException(
				"The current aggregate '" + CurrentAggregate + "' is not valid, "
				+ "so no final valid aggregate can be returned."
			)
		);
		

		public PushAggregator(
			IPushingEnumerator<T> sourceEnumerator,
			PushAggregatorFunc<T, TAggregate> iterator
		) {
			if (sourceEnumerator == null) throw new ArgumentNullException(nameof(sourceEnumerator));
			if (iterator == null) throw new ArgumentNullException(nameof(iterator));

			this._sourceEnumerator = sourceEnumerator;
			this._iterator = iterator;

			this._iteratorInput = new ItemView<T>.Editor();
			
			this._sourceEnumerator.ItemPushed += this.ItemPushed;
			this._sourceEnumerator.AllItemsPushed += this.AllItemsPushed;
		}

		public PushAggregator(
			IPushingEnumerator<T> sourceEnumerator,
			PushAggregatorFunc<T, TAggregate> iterator,
			TAggregate initialAggregate
		) : this(
			sourceEnumerator: sourceEnumerator,
			iterator: iterator
		) {
			this.CurrentAggregate = initialAggregate;
			this.CurrentAggregateIsValid = true;
		}

		private void ItemPushed(object sender, ItemPushedEventArgs<T> e)
		{
			//Disposing deregisters this event handler, so given that this is being called,
			//there's no need to verify that Disposed is false

			this._iteratorInput.Item = e.CurrentItem;

			if (_iteratorResults == null)
			{
				//Make sure to do this after setting the input item, just in case the delegate
				//or returned IEnumerator (which together often form an iterator) does some
				//processing on the first item immediately, without waiting for the first call to MoveNext()
				this._iteratorResults = _iterator.Invoke(_iteratorInput.GetView());
			}

			if (this._iteratorResults.MoveNext())
			{
				var current = _iteratorResults.Current;
				this.CurrentAggregate = current.currentAgg;
				this.CurrentAggregateIsValid = current.isValid;
			}
			else
			{
				//This could be caused by eg. a query that only needs to aggregate the first x elements,
				//which is perfectly legitimate, so set Finished = true and Dispose(), instead of throwing an
				//exception or something.
				this.Finished = true;
				this.Dispose();
			}
		}

		private void AllItemsPushed(object sender, EventArgs e)
		{
			this.Finished = true;
			this.Dispose();
		}

		/// <summary>
		/// Returns true and outputs <see cref="CurrentAggregate"/> if <see cref="CurrentAggregateIsValid"/>
		/// and <see cref="Finished"/> are both true, otherwise returns false
		/// </summary>
		public bool TryGetResult(out TAggregate result)
		{
			if (Finished && CurrentAggregateIsValid) {
				result = CurrentAggregate;
				return true;
			} else {
				result = default;
				return false;
			}
		}

		public void Dispose()
		{
			this._sourceEnumerator.ItemPushed -= this.ItemPushed;
			this._sourceEnumerator = null;
			this._iterator = null;
			this._iteratorInput = null;
			this._iteratorResults = null;
		}
	}

	public static class PushAggregator
	{
		public static PushingEnumerator<T> AsPushingEnumerator<T>(this IEnumerable<T> source)
		{
			return new PushingEnumerator<T>(
				source
			);
		}

		public static PushAggregatorFunc<T, int> GetCountIterator<T>(Func<T, bool> predicate = null)
		{
			return CountIterator;

			IEnumerator<(int currentAgg, bool isValid)> CountIterator(ItemView<T> input)
			{
				int i = 0;
				while (true) //yup. no break. the calling code handles that
				{
					if (predicate?.Invoke(input.Item) ?? true)
					{
						i++;
					}
					yield return (i, true);
				}
			};
		}

		public static PushAggregator<T, int> Count<T>(this IPushingEnumerator<T> source, Func<T, bool> predicate = null)
			=> new PushAggregator<T, int>(source, GetCountIterator(predicate));



		public static PushAggregatorFunc<T, T> GetMaxIterator<T, TCompare>(Func<T, TCompare> selector)
		{
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			bool isComparableT = typeof(IComparable<TCompare>).IsAssignableFrom(typeof(TCompare));
			bool isComparable = typeof(IComparable).IsAssignableFrom(typeof(TCompare));
			if (!isComparableT && !isComparable) throw new ArgumentException(
				"Type '" + typeof(TCompare).FullName + "' "
				+ "does not implement '" + typeof(IComparable<TCompare>).FullName + "' "
				+ "or '" + typeof(IComparable).FullName + "', "
				+ "so values of this type cannot be compared to find a maximum."
			);

			return Max;

			IEnumerator<(T currentAgg, bool isValid)> Max(ItemView<T> input)
			{
				T max = input.Item;
				TCompare maxCompare = selector(max);
				yield return (max, true);

				while (true)
				{
					TCompare itemCompare = selector(input.Item);
					int comparison;
					if (isComparableT) comparison = ((IComparable<TCompare>)itemCompare).CompareTo(maxCompare);
					else comparison = ((IComparable)itemCompare).CompareTo(maxCompare);

					if (comparison > 0) {
						max = input.Item;
						maxCompare = itemCompare;
					}

					yield return (max, true);
				}
			}
		}

		public static PushAggregator<T, T> Max<T, TCompare>(this IPushingEnumerator<T> source, Func<T, TCompare> selector)
			=> new PushAggregator<T, T>(source, GetMaxIterator(selector));



		public static PushAggregatorFunc<T, T> GetMinIterator<T, TCompare>(Func<T, TCompare> selector)
		{
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			bool isComparableT = typeof(IComparable<TCompare>).IsAssignableFrom(typeof(TCompare));
			bool isComparable = typeof(IComparable).IsAssignableFrom(typeof(TCompare));
			if (!isComparableT && !isComparable) throw new ArgumentException(
				"Type '" + typeof(TCompare).FullName + "' "
				+ "does not implement '" + typeof(IComparable<TCompare>).FullName + "' "
				+ "or '" + typeof(IComparable).FullName + "', "
				+ "so values of this type cannot be compared to find a minimum."
			);

			return Min;

			IEnumerator<(T currentAgg, bool isValid)> Min(ItemView<T> input)
			{
				T min = input.Item;
				TCompare minCompare = selector(min);
				yield return (min, true);

				while (true)
				{
					TCompare itemCompare = selector(input.Item);
					int comparison;
					if (isComparableT) comparison = ((IComparable<TCompare>)itemCompare).CompareTo(minCompare);
					else comparison = ((IComparable)itemCompare).CompareTo(minCompare);

					if (comparison < 0) {
						min = input.Item;
						minCompare = itemCompare;
					}

					yield return (min, true);
				}
			}
		}

		public static PushAggregator<T, T> Min<T, TCompare>(this IPushingEnumerator<T> source, Func<T, TCompare> selector)
			=> new PushAggregator<T, T>(source, GetMinIterator(selector));

		public static PushAggregatorFunc<T, T> GetLastIterator<T>()
		{
			return Last;

			IEnumerator<(T currentAgg, bool isValid)> Last(ItemView<T> input)
			{
				while (true)
				{
					T last = input.Item;
					yield return (last, true);
				}
			}
		}

		public static PushAggregator<T, T> Last<T>(this IPushingEnumerator<T> source)
			=> new PushAggregator<T, T>(source, GetLastIterator<T>());
	}
}

//*/