using Il2Cpp;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem.Reflection;
using MelonLoader;
using PrimitierPlayerConfig.Patches;
using PrimitierPlayerConfig.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

namespace PrimitierPlayerConfig
{
    class ScaleFixer
	{
		GameObject cameraOffsetObject = null!;
		List<GameObject> scaleBackObjects = new List<GameObject>();

		public float playerHeight = 1.5f;
		public void FixScale()
		{
			AvatarHeightChangeHook.OnValueChange += calibrate;
			cameraOffsetObject = GameObject.Find("Camera Offset");

			//scaleBackObjects.Add(GameObject.Find("MenuWindowR"));
			//scaleBackObjects.Add(GameObject.Find("MenuWindowL"));

			if (PrimitierPlayerConfigMod.VRMModel.active)
				OnAvatarEnabled();

			var avatarBtn = UnityUtils.FindGameObject("AvatarVisibilityButton")?.GetComponent<Button>() ?? throw new("Can't find a button");
			var calibrateBtn = UnityUtils.FindGameObject("RecalibrateButton")?.GetComponent<Button>() ?? throw new("Can't find a button");

			avatarBtn.onClick.AddListener(new Action(() => {
				if (PrimitierPlayerConfigMod.VRMModel.active)
					OnAvatarEnabled();
				else
					OnAvatarDisabled();
			}));

			try { calibrateBtn.onClick.RemoveAllListeners(); } catch (Exception) { }

			calibrateBtn.onClick.AddListener(new Action(calibrate));

			AvatarHeightChangeHook.OnValueChange += () =>
			{
				if (PrimitierPlayerConfigMod.VRMModel.active)
					OnAvatarEnabled();
			};
		}

		private void calibrate()
		{
			playerHeight = PrimitierPlayerConfigMod.XROrigin.CameraInOriginSpaceHeight;
			if (PrimitierPlayerConfigMod.VRMModel.active)
				OnAvatarEnabled();
		}


		float? prevHeight = 0;

		void OnAvatarEnabled()
		{
			var avatarHeight = HeightCalibrator.avatarHeight * HeightCalibrator.avatarScale;

			var scaleFactor = avatarHeight / playerHeight;
			cameraOffsetObject.transform.localPosition = new(cameraOffsetObject.transform.localPosition.x, 0, cameraOffsetObject.transform.localPosition.z);
			HeightCalibrator.avatarHeightOffset = 0;
			HeightCalibrator.realPoseHeightOffset = 0;
			var newScale = PrimitierPlayerConfigMod.XROrigin.transform.localScale = new Vector3(1, 1, 1) * scaleFactor;
			ConfigurableJointPatch.multiplier = scaleFactor;
			
			scaleBackObjects.ForEach(obj => obj.transform.localScale = new(1f/newScale.x, 1f/newScale.y, 1f/newScale.z));

			if (prevHeight != null)
				PrimitierPlayerConfigMod.XROrigin.transform.position = PrimitierPlayerConfigMod.XROrigin.transform.position.withY(0.1f);

			prevHeight = avatarHeight;
		}

		void OnAvatarDisabled()
		{
			PrimitierPlayerConfigMod.XROrigin.transform.localScale = new(1, 1, 1);
			ConfigurableJointPatch.multiplier = 1f;

			scaleBackObjects.ForEach(e => e.transform.localScale = new(1, 1, 1));
		}
	}
}
