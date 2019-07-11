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
	/// A readonly reference to an object, which can be modified by the code that created it.
	/// <para/>
	/// The <see cref="ItemView{T}"/> itself doesn't need to be modified, only the underlying <see cref="ItemView{T}.Editor"/>
	/// that it references, so <see cref="ItemView{T}"/> can be a struct.
	/// </summary>
	public struct ItemView<T>
	{
		private readonly Editor _editor;
		public T Item => _editor == null ? default : _editor.Item;

		public ItemView(Editor editor)
		{
			this._editor = editor;
		}

		public class Editor
		{
			public T Item { get; set; }

			public Editor(T item = default)
			{
				this.Item = item;
			}

			public ItemView<T> GetView() => new ItemView<T>(editor: this);
		}
	}
}

//*/