
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PrimitierPlayerConfig.Patches
{
	[HarmonyPatch(typeof(UnityEngine.ConfigurableJoint), "set_targetPosition")]
	internal class ConfigurableJointPatch
	{
		public static float multiplier = 1;
		static void Prefix(ConfigurableJoint __instance, ref Vector3 __0)
		{
			if (__instance.gameObject.name == "XR Origin")
			{
				__0 *= multiplier;
			}
		}
	}
}
