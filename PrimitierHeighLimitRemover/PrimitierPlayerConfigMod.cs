using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppSystem.IO;
using MathParserTK;
using MelonLoader;
using Mono.CSharp;
using Newtonsoft.Json.Linq;
using PrimitierPlayerConfig.Excpetions;
using System.Collections;
using System.Data;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

namespace PrimitierPlayerConfig
{
	public class PrimitierPlayerConfigMod : MelonMod
	{
		public static MelonLogger.Instance? Logger;
		public override void OnEarlyInitializeMelon()
		{
			base.OnEarlyInitializeMelon();
			Logger = LoggerInstance;
		}

		public static GameObject AvatarParent = null!;
		public static GameObject Player = null!;
		public static GameObject XROriginObject = null!;
		public static XROrigin XROrigin = null!;
		public static GameObject VRMModel = null!;

		public static JObject Config = null!;

		IEnumerator WaitForAvatar()
		{
			LoggerInstance.Msg("Waiting for avatar...");

			while (AvatarParent.transform.childCount <= 0)
				yield return new WaitForEndOfFrame();
			yield return new WaitForSeconds(0.5f);

			VRMModel = AvatarParent.transform.GetChild(0).gameObject;

			LoggerInstance.Msg($"Avatar loaded! Avatar enabled status: {VRMModel.active}");

			// All lateinit vars was initialized!

			OnAvatarWasLoaded();
		}

		public void OnAvatarWasLoaded()
		{
			if (Config.TryGetValue("overrideFields", out var overrideData))
			{
				FieldOverrider fieldOverrider = new(overrideData as JObject ?? throw new RuntimeArgumentException("\"override\" field in config file should be a object"));
				fieldOverrider.OverrideFileds();
			}

			if (((bool?)Config["fixWorldScale"]) == true)
			{
				new ScaleFixer().FixScale();
			}
		}

		public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
			base.OnSceneWasInitialized(buildIndex, sceneName);

			// Loading vars:

			AvatarParent = GameObject.Find("AvatarParent");
			Player = GameObject.Find("Player");
			XROriginObject = GameObject.Find("XR Origin");
			XROrigin = XROriginObject.GetComponent<XROrigin>();

			/*if (Utils.NoneIsNull(AvatarParent, Player, XROrigin, XROriginObject))
			{
				LoggerInstance.Error($"One of GO didn't found: {AvatarParent==null}, {Player==null}, {XROrigin==null}, {XROriginObject==null}");
				return;
			}*/

			// Loading config:

			string configFilePath = Il2CppSystem.Environment.GetFolderPath(Il2CppSystem.Environment.SpecialFolder.MyDocuments)
				+ Il2CppSystem.IO.Path.DirectorySeparatorStr + "Primitier"
				+ Il2CppSystem.IO.Path.DirectorySeparatorStr + "playerConfig.json";
			if (!Il2CppSystem.IO.File.Exists(configFilePath))
			{
				LoggerInstance.Warning($"Config file at {configFilePath} didn't found!");
				return;
			}

			Config = JObject.Parse(Il2CppSystem.IO.File.ReadAllText(configFilePath));

			MelonCoroutines.Start(WaitForAvatar());
		}

		
	}
}
