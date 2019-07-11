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
	public interface IPushingEnumerator<T> : IDisposable
	{
		event EventHandler<ItemPushedEventArgs<T>> ItemPushed;
		event EventHandler AllItemsPushed;
		bool AreAllItemsPushed { get; }
		bool Disposed { get; }
	}

	public interface IPushableEnumerator<T> : IPushingEnumerator<T>
	{
		/// <summary>Attempts to push the next item, returning false if there isn't one.</summary>
		bool Push();
		/// <summary>Attempts to push all remaining items one at a time, returning false if there are none.</summary>
		bool PushAll();
	}

	public class PushingEnumerator<T> : IPushableEnumerator<T>
	{
		private readonly IEnumerator<T> _enumerator;

		public event EventHandler<ItemPushedEventArgs<T>> ItemPushed;
		public event EventHandler AllItemsPushed;

		public bool AreAllItemsPushed { get; private set; }

		public bool Disposed { get; private set; }

		public PushingEnumerator(IEnumerator<T> enumerator)
		{
			if (enumerator == null) throw new ArgumentNullException(nameof(enumerator));

			this._enumerator = enumerator;
		}

		public PushingEnumerator(IEnumerable<T> enumerable)
			: this(enumerable?.GetEnumerator())
		{ }

		public bool Push()
		{
			if (_enumerator.MoveNext())
			{
				ItemPushed?.Invoke(this, new ItemPushedEventArgs<T>(_enumerator.Current));
				return true;
			}
			else
			{
				AreAllItemsPushed = true;
				AllItemsPushed?.Invoke(this, EventArgs.Empty);
				AllItemsPushed = null;
				return false;
			}
		}

		public bool PushAll()
		{
			bool result = false;
			while (Push())
			{
				result = true;
			}
			return result;
		}

		public void Dispose()
		{
			_enumerator.Dispose();
			this.Disposed = true;
		}
	}

	public class ItemPushedEventArgs<T>
	{
		public T CurrentItem { get; }

		public ItemPushedEventArgs(T currentItem)
		{
			this.CurrentItem = currentItem;
		}
	}
}

//*/