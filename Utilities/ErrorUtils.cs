using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VideoMusicSyncer
{
	public static class ErrorUtils
	{
		public static T ThrowIfArgNull<T>(T arg, string name)
		{
			if (arg == null) throw new ArgumentNullException(name);
			else return arg;
		}

		public static string ThrowIfArgFailsMatch(string arg, string regex, string argName)
		{
			ThrowIfArgNull(arg, argName);
			if (!Regex.IsMatch(arg, regex)) throw new ArgumentException(
				$"{argName} \"{arg}\" is not valid as it does not match regex \"{regex}\"",
				argName
			);
			else return arg;
		}

		public static T ThrowIfArgLessThan<T>(T arg, T min, string argName, string minName = null)
			where T : IComparable<T>
		{
			ThrowIfArgNull(arg, argName);
			if (min == null) return arg;

			if (arg.CompareTo(min) < 0) throw new ArgumentOutOfRangeException(
				paramName: argName,
				actualValue: arg,
				message: (
					"Cannot be less than "
					+ (String.IsNullOrEmpty(minName) ? "" : minName + " = ") + "\"" + min + "\" "
					+ "(provided value: \"" + arg + "\")."
				)
			);

			return arg;
		}

		public static T ThrowIfArgGreaterThan<T>(T arg, T max, string argName, string maxName = null)
			where T : IComparable<T>
		{
			ThrowIfArgNull(arg, argName);
			if (max == null) return arg;

			if (arg.CompareTo(max) > 0) throw new ArgumentOutOfRangeException(
				paramName: argName,
				actualValue: arg,
				message: (
					"Cannot be greater than "
					+ (String.IsNullOrEmpty(maxName) ? "" : maxName + " = ") + "\"" + max + "\" "
					+ "(provided value: \"" + arg + "\")."
				)
			);

			return arg;
		}

		public static T ThrowIfArgOutsideRange<T>(T arg, T min, T max, string argName, string minName = null, string maxName = null)
			where T : IComparable<T>
		{
			ThrowIfArgLessThan(arg, min, argName, minName);
			ThrowIfArgGreaterThan(arg, max, argName, maxName);
			return arg;
		}
	}
}

//*/