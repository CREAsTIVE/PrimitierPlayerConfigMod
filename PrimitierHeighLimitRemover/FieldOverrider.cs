using Il2Cpp;
using Il2CppInterop.Runtime;
using MathParserTK;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PrimitierPlayerConfig
{
	internal class FieldOverrider
	{
		JObject overrides;

		public FieldOverrider(JObject overrides)
		{
			this.overrides = overrides;
		}
		static string resolveVariables(string input)
		{
			Dictionary<string, string> vars = new()
			{
				{ "avatarHeight",  HeightCalibrator.avatarHeight.FString() }
			};

			foreach (var v in vars) { input = input.Replace($"${v.Key}", v.Value); }

			return input;
		}

		public void OverrideFileds()
		{
			Dictionary<string, Il2CppSystem.Object> mappedObjects = new()
			{
				{ "Player", GameObject.Find("Player")}
			};

			foreach (var pair in overrides)
			{
				IEnumerable<string> path = pair.Key.Split(".");
				string baseKey = path.First();
				path = path.Skip(1);

				Il2CppSystem.Object baseObject;

				if (mappedObjects.TryGetValue(baseKey, out var obj))
					baseObject = obj;
				else
					baseObject = GameObject.Find(baseKey);

				var resolved = FieldResolver.resolveObject(baseObject, path);

				if (resolved.fieldType.IsEquivalentTo(Il2CppType.Of<float>()))
					resolved.setter(asFloat(pair.Value));
				else
					throw new NotImplementedException($"Value {resolved.fieldType.FullName} didn't supported :(");
			}
		}

		static MathParser parser = new MathParser('.');
		static double Compute(string expr)
		{

			string resolved = resolveVariables(expr);
			PrimitierPlayerConfigMod.Logger?.Msg($"Calculating {resolved}");
			return parser.Parse(resolved);

		
		}
		static float ComputeFloat(string expr) => (float)Compute(expr);

		float asFloat(JToken? token)
		{
			if (token?.Type == JTokenType.String)
				return ComputeFloat(((string?)token) ?? throw new("idk"));
			else
				return token?.Value<float>() ?? throw new ArgumentException("Float");
		}
	}
}
