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
		public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
			base.OnSceneWasInitialized(buildIndex, sceneName);

			var player = GameObject.Find("Player");

			if (player == null)
			{
				LoggerInstance.Error("NO PLAYER FOUND!!!");
				return;
			}

			string configFilePath = Il2CppSystem.Environment.GetFolderPath(Il2CppSystem.Environment.SpecialFolder.MyDocuments)
				+ Il2CppSystem.IO.Path.DirectorySeparatorStr + "Primitier"
				+ Il2CppSystem.IO.Path.DirectorySeparatorStr + "playerConfig.json";
			if (!Il2CppSystem.IO.File.Exists(configFilePath))
			{
				LoggerInstance.Warning($"Config file at {configFilePath} didn't found!");
				return;
			}

			JObject configFile = JObject.Parse(Il2CppSystem.IO.File.ReadAllText(configFilePath));

			if (configFile.TryGetValue("overrideFields", out var overrideData))
			{
				FieldOverrider fieldOverrider = new(overrideData as JObject ?? throw new RuntimeArgumentException("\"override\" field in config file should be a object"));
				fieldOverrider.OverrideFileds();
			}

			if (((bool?)configFile["fixWorldScale"]) == true)
				MelonCoroutines.Start(new ScaleFixer() { }.FixScale());

			//new ReflectedProperty("Player.baseSpeed", playerMovement, "baseSpeed").set(1);
		}

		string varsResolver(string input)
		{
			Dictionary<string, string> vars = new()
			{

			};

			foreach (var v in vars) { input = input.Replace($"${v.Key}", v.Value); }

			return input;
		}
		public static GameObject findGO(string name) =>
			GameObject.FindObjectsOfTypeAll(Il2CppType.Of<Transform>()).Select(e => e.Cast<Transform>()).First(t => t.gameObject.name == name).gameObject;

		
	}
}
