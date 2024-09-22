using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppSystem;
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
		Dictionary<string, string> context = new()
		{
			{ "avatarHeight",  HeightCalibrator.avatarHeight.FString() }
		};

		public FieldOverrider()
		{
			
		}
		string resolveVariables(string input)
		{
			foreach (var v in context) { input = input.Replace($"${v.Key}", v.Value); }
			return input;
		}

		public void DefineVariables(JObject variables)
		{
			foreach (var pair in variables)
			{
				try
				{
					context[pair.Key] = pair.Value?.Type switch
					{
						JTokenType.String => Compute(((string?)pair.Value)!).DString(),
						JTokenType.Float => ((float)pair.Value).FString(),
						JTokenType.Boolean => ((bool)pair.Value) ? "1" : "0",
						JTokenType.Integer => ((int)pair.Value).ToString(),
						_ => throw new System.NotImplementedException($"Type {pair.Value?.Type} doesn't support!")
					};
				} catch (System.Exception ex)
				{
					PrimitierPlayerConfigMod.Logger?.Warning(ex);
					PrimitierPlayerConfigMod.Logger?.Warning($"Failed to calculate variable {pair.Key}. Skipping...");
				}
			}
		}

		public void OverrideFileds(JObject overrides)
		{
			Dictionary<string, Il2CppSystem.Object> mappedObjects = new()
			{
				{ "Player", GameObject.Find("Player")}
			};

			foreach (var pair in overrides)
			{
				try
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
						throw new System.NotImplementedException($"Value {resolved.fieldType.FullName} didn't supported :(");
				} catch (System.Exception ex)
				{
					PrimitierPlayerConfigMod.Logger?.Warning(ex);
					PrimitierPlayerConfigMod.Logger?.Warning($"Can't change \"{pair.Key}\" to {pair.Value }. Skipping...");
				}
			}
		}

		static MathParser parser = new MathParser('.');
		double Compute(string expr)
		{

			string resolved = resolveVariables(expr);
			PrimitierPlayerConfigMod.Logger?.Msg($"Calculating {resolved}");
			return parser.Parse(resolved);

		
		}
		float ComputeFloat(string expr) => (float)Compute(expr);

		float asFloat(JToken? token)
		{
			if (token?.Type == JTokenType.String)
				return ComputeFloat(((string?)token) ?? throw new("idk"));
			else
				return token?.Value<float>() ?? throw new System.ArgumentException("Float");
		}
	}
}
