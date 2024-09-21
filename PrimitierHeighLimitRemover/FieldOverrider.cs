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

				if (resolved.field.FieldType.IsEquivalentTo(Il2CppType.Of<float>()))
					resolved.field.SetValue(resolved.container, asFloat(pair.Value));
				else
					throw new NotImplementedException($"Value {resolved.field.FieldType.FullName} didn't supported :(");
			}
		}

		static MathParser parser = new MathParser();
		static double Compute(string expr) => parser.Parse(expr);
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
