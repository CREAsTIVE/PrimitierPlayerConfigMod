using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace PrimitierPlayerConfig
{
	public static class Utils
	{
		public static GameObject findGO(string name) =>
			GameObject.FindObjectsOfTypeAll(Il2CppType.Of<Transform>()).Select(e => e.Cast<Transform>()).First(t => t.gameObject.name == name).gameObject;
		public static bool NoneIsNull(params Il2CppSystem.Object[] objects) => !objects.Contains((e) => e == null);

		public static bool Contains<T>(this IEnumerable<T> values, Func<T, bool> predicate)
		{
			foreach (var value in values)
				if (predicate(value))
					return true;
			return false;
		}

		public static string FString(this float value) =>
			value.ToString("R", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
		public static string DString(this double value) =>
			value.ToString("R", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
	}
}
