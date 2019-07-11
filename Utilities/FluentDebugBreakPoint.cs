using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace VideoMusicSyncer.FluentDebugBreakPoint
{
	/// <summary>
	/// Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)
	/// </summary>
	public static class FluentDebugBreakPoint
	{
		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		public static T DebugBreak<T>(this T obj) {
			return obj;
		}

		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows any number of unnamed watches. For named watches, pass a tuple into <see cref="DebugBreak{T, T1}(T, T1)"/></param>
		public static T DebugBreak<T>(this T obj, object[] extraWatches) {
			return obj;
		}

		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows one unnamed watch. For any number of named watches, pass in a tuple.</param>
		public static T DebugBreak<T, T1>(this T obj, T1 watch) {
			return obj;
		}
		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows two unnamed watches. For named watches, pass a tuple into <see cref="DebugBreak{T, T1}(T, T1)"/></param>
		public static T DebugBreak<T, T1, T2>(this T obj, T1 watch1, T2 watch2) {
			return obj;
		}
		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows three unnamed watches. For named watches, pass a tuple into <see cref="DebugBreak{T, T1}(T, T1)"/></param>
		public static T DebugBreak<T, T1, T2, T3>(this T obj, T1 watch1, T2 watch2, T3 watch3) {
			return obj;
		}
		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows four unnamed watches. For named watches, pass a tuple into <see cref="DebugBreak{T, T1}(T, T1)"/></param>
		public static T DebugBreak<T, T1, T2, T3, T4>(this T obj, T1 watch1, T2 watch2, T3 watch3, T4 watch4) {
			return obj;
		}
		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows five unnamed watches. For named watches, pass a tuple into <see cref="DebugBreak{T, T1}(T, T1)"/></param>
		public static T DebugBreak<T, T1, T2, T3, T4, T5>(this T obj, T1 watch1, T2 watch2, T3 watch3, T4 watch4, T5 watch5) {
			return obj;
		}
		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows six unnamed watches. For named watches, pass a tuple into <see cref="DebugBreak{T, T1}(T, T1)"/></param>
		public static T DebugBreak<T, T1, T2, T3, T4, T5, T6>(this T obj, T1 watch1, T2 watch2, T3 watch3, T4 watch4, T5 watch5, T6 watch6) {
			return obj;
		}
		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows seven unnamed watches. For named watches, pass a tuple into <see cref="DebugBreak{T, T1}(T, T1)"/></param>
		public static T DebugBreak<T, T1, T2, T3, T4, T5, T6, T7>(this T obj, T1 watch1, T2 watch2, T3 watch3, T4 watch4, T5 watch5, T6 watch6, T7 watch7) {
			return obj;
		}
		/// <summary>Allows breakpoints mid way through using a fluent API (eg. linq or immutable collections)</summary>
		/// <param name="extraWatches">Allows eight unnamed watches. For named watches, pass a tuple into <see cref="DebugBreak{T, T1}(T, T1)"/></param>
		public static T DebugBreak<T, T1, T2, T3, T4, T5, T6, T7, T8>(this T obj, T1 watch1, T2 watch2, T3 watch3, T4 watch4, T5 watch5, T6 watch6, T7 watch7, T8 watch8) {
			return obj;
		}
	}
}

//*/