using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using MathParserTK;
using Newtonsoft.Json.Linq;
using PrimitierPlayerConfig.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PrimitierPlayerConfig.FieldOverrider
{
    internal class FieldOverrider
    {
        /*Dictionary<string, string> context = new()
        {
            { "avatarHeight",  HeightCalibrator.avatarHeight.FString() }
        };
*/
        public static List<Variable> defaultVariables = new List<Variable>();

        static FieldOverrider()
        {
			var ah = new SettableHookedVariable(HeightCalibrator.avatarHeight * HeightCalibrator.avatarScale);
			AvatarHeightChangeHook.OnValueChange +=
				() => ah.SettableValue = HeightCalibrator.avatarHeight * HeightCalibrator.avatarScale;
            defaultVariables.Add(ah.Apply(e => e.Name = "avatarHeight"));
		}

        public FieldOverrider()
        {
            ResetEnv();
        }

        public void ResetEnv()
        {
            if (env != null) env.Clear();
			env = new(defaultVariables);
        }

        public Environment env = null!;

        public void DefineVariables(JObject variables)
        {
            foreach (var pair in variables)
            {
                try
                {
                    env.AddVariable(pair.Value?.Type switch
                    {
                        JTokenType.String => new Expression(((string)pair.Value)!, env),
						JTokenType.Integer => new ValueVariable((int)pair.Value),
						JTokenType.Float => new ValueVariable((float)pair.Value),
                        _ => throw new System.NotImplementedException("Unknown JSON type")
                    }, pair.Key);
                    /*context[pair.Key] = pair.Value?.Type switch
                    {
                        JTokenType.String => Compute(((string?)pair.Value)!).DString(),
                        JTokenType.Float => ((float)pair.Value).FString(),
                        JTokenType.Boolean => (bool)pair.Value ? "1" : "0",
                        JTokenType.Integer => ((int)pair.Value).ToString(),
                        _ => throw new System.NotImplementedException($"Type {pair.Value?.Type} doesn't support!")
                    };*/
                }
                catch (System.Exception ex)
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

                    baseObject = mappedObjects.TryGetValue(baseKey, out var obj) ? obj : Utils.findGO(baseKey);

                    var resolved = FieldResolver.resolveObject(baseObject, path);

                    if (resolved.fieldType.IsEquivalentTo(Il2CppType.Of<float>()))
                    {
                        if (pair.Value?.Type == JTokenType.Float || pair.Value?.Type == JTokenType.Integer)
                            resolved.setter(pair.Value.Value<float>());
                        else if (pair.Value?.Type == JTokenType.String)
                            new HookedSetter(new Expression(((string?)pair.Value)!, env), (v) => resolved.setter(((float)v)));
                        // resolved.setter(asFloat(pair.Value));
                        else
                            throw new System.NotImplementedException("Unknow json type");
					}
                    else
                        throw new System.NotImplementedException($"Value {resolved.fieldType.FullName} didn't supported :(");
                }
                catch (System.Exception ex)
                {
                    PrimitierPlayerConfigMod.Logger?.Warning(ex);
                    PrimitierPlayerConfigMod.Logger?.Warning($"Can't change \"{pair.Key}\" to {pair.Value}. Skipping...");
                }
            }
        }

        /*static MathParser parser = new MathParser('.');
        double Compute(string expr)
        {

            string resolved = resolveVariables(expr);
            PrimitierPlayerConfigMod.Logger?.Msg($"Calculating {resolved}");
            return parser.Parse(resolved);


        }
        float ComputeFloat(string expr) => (float)Compute(expr);*/

        /*float asFloat(JToken? token)
        {
            if (token?.Type == JTokenType.String)
                return ComputeFloat((string?)token ?? throw new("idk"));
            else
                return token?.Value<float>() ?? throw new System.ArgumentException("Float");
        }*/
    }
}
