using Il2Cpp;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem.Reflection;
using MelonLoader;
using PrimitierPlayerConfig.Patches;
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
		GameObject rig = null!;
		GameObject cameraOffsetObject = null!;
		List<GameObject> scaleBackObjects = new List<GameObject>();
		GameObject avatarParent = null!;

		public float playerHeight = 1.5f;
		public IEnumerator FixScale()
		{
			rig = GameObject.Find("XR Origin");
			var rigComponent = rig.GetComponent<XROrigin>();

			cameraOffsetObject = GameObject.Find("Camera Offset");
			scaleBackObjects.Add(GameObject.Find("MenuWindowR"));
			scaleBackObjects.Add(GameObject.Find("MenuWindowL"));
			avatarParent = GameObject.Find("AvatarParent");


			while (avatarParent.transform.childCount <= 0)
				yield return new WaitForEndOfFrame();

			var vrmModel = avatarParent.transform.GetChild(0).gameObject;
			

			yield return new WaitForSeconds(0.5f);

			if (vrmModel.active)
				OnAvatarEnabled();

			PrimitierPlayerConfigMod.Logger?.Msg($"Avatar loaded! Current avatar state: {(vrmModel.active ? "active" : "inactive")}");
			var avatarBtn = PrimitierPlayerConfigMod.findGO("AvatarVisibilityButton").GetComponent<Button>();
			var calibrateBtn = PrimitierPlayerConfigMod.findGO("RecalibrateButton").GetComponent<Button>();

			avatarBtn.onClick.AddListener(new Action(() => {
				if (vrmModel.active)
					OnAvatarEnabled();
				else
					OnAvatarDisabled();
			}));

			try { calibrateBtn.onClick.RemoveAllListeners(); } catch (Exception) { }

			calibrateBtn.onClick.AddListener(new Action(() =>
			{
				playerHeight = rigComponent.CameraInOriginSpaceHeight;
				if (vrmModel.active)
					OnAvatarEnabled();
			}));
		}

		void OnAvatarEnabled()
		{
			HeightCalibrator hc = avatarParent.GetComponent<HeightCalibrator>();
			
			var avaHeight = HeightCalibrator.avatarHeight;

			var scaleFactor = avaHeight/playerHeight;
			cameraOffsetObject.transform.localPosition = new(cameraOffsetObject.transform.localPosition.x, 0, cameraOffsetObject.transform.localPosition.z);
			//HeightCalibrator.avatarHeightOffset = 0;
			var newScale = rig.transform.localScale = new Vector3(1, 1, 1) * scaleFactor;
			ConfigurableJointPatch.multiplier = scaleFactor;
			
			//scaleBackObjects.ForEach(obj => obj.transform.localScale = new(1f/newScale.x, 1f/newScale.y, 1f/newScale.z));
		}

		void OnAvatarDisabled()
		{
			rig.transform.localScale = new(1, 1, 1);
			ConfigurableJointPatch.multiplier = 1f;
			//scaleBackObjects.ForEach(e => e.transform.localScale = new(1, 1, 1));
		}
	}
}
