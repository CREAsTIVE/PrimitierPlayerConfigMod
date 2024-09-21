using Il2CppSystem.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PrimitierPlayerConfig
{
	static class FieldResolver
	{
		public static (Action<Il2CppSystem.Object> setter, Il2CppSystem.Type fieldType) resolveField(Il2CppSystem.Object container, FieldInfo field, IEnumerable<string> path)
		{
			if (path.Count() == 0)
				return ((val) => field.SetValue(container, val), field.FieldType);
			return resolveObject(field.GetValue(container), path);
		}
		public static (Action<Il2CppSystem.Object> setter, Il2CppSystem.Type fieldType) resolveField(Il2CppSystem.Object container, PropertyInfo field, IEnumerable<string> path)
		{
			if (path.Count() == 0)
				return ((val) => field.SetValue(container, val), field.PropertyType);
			return resolveObject(field.GetValue(container), path);
		}

		public static (Action<Il2CppSystem.Object> setter, Il2CppSystem.Type fieldType) resolveObject(Il2CppSystem.Object anyObject, params string[] path) =>
			resolveObject(anyObject, (IEnumerable<string>)path);

		public static (Action<Il2CppSystem.Object> setter, Il2CppSystem.Type fieldType) resolveObject(Il2CppSystem.Object anyObject, IEnumerable<string> path)
		{
			string requested = path.First();
			path = path.Skip(1);

			var asGO = anyObject.TryCast<GameObject>();
			if (asGO != null)
			{
				// Try to find child GO:
				for (var i = 0; i < asGO.transform.childCount; i++)
				{
					var child = asGO.transform.GetChild(i);

					if (child.name == requested)
						return resolveObject(child.gameObject, path);
				}

				// Try to find component: 
				var components = asGO.GetComponents<Component>();
				foreach (var component in components)
				{
					if (component.GetIl2CppType().Name == requested)
						return resolveObject(component, path);
				}
			}
			MemberInfo member = anyObject.GetIl2CppType().GetMember(requested, 
				BindingFlags.Static | 
				BindingFlags.Instance | 
				BindingFlags.Public | 
				BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy |
				BindingFlags.GetField |
				BindingFlags.GetProperty |
				BindingFlags.SetField |
				BindingFlags.SetProperty
			).First();
			var field = member.TryCast<FieldInfo>();
			if (field != null)
				return resolveField(anyObject, field, path);

			var prop = member.TryCast<PropertyInfo>();
			if (prop != null)
				return resolveField(anyObject, prop, path);

			throw new NotImplementedException($"Unknow property type: {member.MemberType}");
		}
	}
}
