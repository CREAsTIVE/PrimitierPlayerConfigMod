using HarmonyLib;
using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitierPlayerConfig.Patches
{
	[HarmonyPatch(typeof(HeightCalibrator), nameof(HeightCalibrator.OnSliderPointerUp))]
	internal class AvatarHeightChangeHook
	{
		public static Action OnValueChange = delegate { };
		public static void Postfix() { OnValueChange(); }
	}

	[HarmonyPatch(typeof(HeightCalibrator), nameof(HeightCalibrator.Calibrate))]
	class HeightCalibratorInvoked
	{
		public static Action OnCalibration = delegate { };
		public static void Postfix() { OnCalibration(); }
	}
}
