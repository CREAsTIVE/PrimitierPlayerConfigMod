using MelonLoader;
using Newtonsoft.Json.Linq;
using PrimitierPlayerConfig.Excpetions;
using PrimitierPlayerConfig.Patches;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

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

		Action FixedUpdateAction = delegate { };

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();
			FixedUpdateAction();
		}

		IEnumerator WaitForAvatar()
		{
			LoggerInstance.Msg("Waiting for avatar...");

			while (AvatarParent.transform.childCount <= 0)
				yield return new WaitForEndOfFrame();
			yield return new WaitForSeconds(0.5f);

			VRMModel = AvatarParent.transform.GetChild(0).gameObject;
			fieldOverrider = new();

			LoggerInstance.Msg($"Avatar loaded! Avatar enabled status: {VRMModel.active}");

			// All lateinit vars was initialized!

			OnAvatarWasLoaded();
		}

		FieldOverrider.FieldOverrider fieldOverrider = null!;

		public override void OnUpdate()
		{
			if (Input.GetKey(KeyCode.U))
			{
				LoadOverrides(loadConfig()!);
				LoggerInstance.Msg("overridedFields reloaded");
			}
		}

		void LoadOverrides(JObject config)
		{
			fieldOverrider.ResetEnv();
			if (config.TryGetValue("variables", out var vars))
				fieldOverrider.DefineVariables(vars as JObject ?? throw new RuntimeArgumentException("\"variables\" field in config should be a object"));
			if (config.TryGetValue("overrideFields", out var overrideData))
				fieldOverrider.OverrideFileds(overrideData as JObject ?? throw new RuntimeArgumentException("\"override\" field in config file should be a object"));
		}

		public void OnAvatarWasLoaded()
		{
			LoadOverrides(Config);

			if (((bool?)Config["fixWorldScale"]) == true)
				new ScaleFixer().FixScale();
			
			// FIXME: On any mass change (smart hooks)
			if (((bool?)Config["stickPointerToHand"]) == true)
			{
				var leftHand = GameObject.Find("LeftHand");
				var rightHand = GameObject.Find("RightHand");
				void fixHands()
				{
					Logger?.Msg("Fixing hands...");

					void fixHand(GameObject hand)
					{
						var rb = hand.GetComponent<Rigidbody>();

						rb.ResetInertiaTensor();

						var collider = hand.AddComponent<SphereCollider>();

						var c = rb.centerOfMass;
						var i = rb.inertiaTensor;

						GameObject.Destroy(collider);

						rb.centerOfMass = c;
						rb.inertiaTensor = i;
						
					}

					fixHand(leftHand);
					fixHand(rightHand);
				}

				fixHands();
				AvatarHeightChangeHook.OnValueChange += fixHands;

				/*(Grabber grabber, Joint joint, bool lastFrameState, float storedMassScale, float storedConnectedMassScale) makeHand(GameObject handGO)
				{
					(Grabber grabber, Joint joint, bool lastFrameState, float storedMassScale, float storedConnectedMassScale) hand = (
						handGO.GetComponent<Grabber>(),
						handGO.GetComponent<Hand>().bodyJoint,
						false, 0, 0
					);

					

					hand.storedMassScale = hand.joint.massScale;
					hand.storedConnectedMassScale = hand.joint.connectedMassScale;

					hand.joint.massScale = 0;
					hand.joint.connectedMassScale = 1;

					LoggerInstance.Msg($"Stored values for {handGO.name}: {{massScale: {hand.storedMassScale}, connectedMassScale: {hand.storedConnectedMassScale}}}");

					return hand;
				}

				var leftHand = makeHand(GameObject.Find("LeftHand"));
				var rightHand = makeHand(GameObject.Find("RightHand"));


				FixedUpdateAction += () =>
				{
					void updatePerHand(ref (Grabber grabber, Joint joint, bool lastFrameState, float storedMassScale, float storedConnectedMassScale) hand)
					{
						var holding = hand.grabber.bond != null;
						if (!holding && hand.lastFrameState)
						{
							hand.lastFrameState = holding;
							hand.joint.massScale = 0;
							hand.joint.connectedMassScale = 1;

						}
						else if (holding && !hand.lastFrameState)
						{
							hand.lastFrameState = holding;
							hand.joint.massScale = hand.storedMassScale;
							hand.joint.connectedMassScale = hand.storedConnectedMassScale;
						}
					}

					updatePerHand(ref leftHand);
					updatePerHand(ref rightHand);
				};*/
			}
		}

		JObject? loadConfig()
		{
			string primitierPath = Il2CppSystem.Environment.GetFolderPath(Il2CppSystem.Environment.SpecialFolder.MyDocuments)
				+ Il2CppSystem.IO.Path.DirectorySeparatorStr + "Primitier";

			var allFiles = System.IO.Directory.GetFiles(primitierPath)
				.Select(e => System.IO.Path.GetFileName(e))
				.Where(e => e.Contains("config") | e.Contains("Config"))
				.ToList();
			allFiles.Sort();
			if (allFiles.Count <= 0)
			{
				LoggerInstance.Warning($"Config file at {primitierPath} didn't found!");
				return null;
			}

			string configFilePath = primitierPath + Il2CppSystem.IO.Path.DirectorySeparatorStr + allFiles.First();

			return JObject.Parse(Il2CppSystem.IO.File.ReadAllText(configFilePath));
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

			var conf = loadConfig();
			if (conf == null) return;
			Config = conf;

			MelonCoroutines.Start(WaitForAvatar());
		}

		
	}
}
