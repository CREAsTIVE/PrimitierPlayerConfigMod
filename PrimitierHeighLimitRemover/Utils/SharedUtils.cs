using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace PrimitierPlayerConfig.Utils
{
    public static class SharedUtils
    {
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

		public static Vector3 withY(this Vector3 value, float y) =>
            new Vector3(value.x, y, value.z);
    }
}
