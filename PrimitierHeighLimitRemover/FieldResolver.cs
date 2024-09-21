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
		public static (Il2CppSystem.Reflection.FieldInfo field, Il2CppSystem.Object container) resolveField(Il2CppSystem.Object container, Il2CppSystem.Reflection.FieldInfo field, IEnumerable<string> path)
		{
			if (path.Count() == 0)
				return (field, container);
			return resolveObject(field.GetValue(container), path);
		}

		public static (Il2CppSystem.Reflection.FieldInfo field, Il2CppSystem.Object container) resolveObject(Il2CppSystem.Object anyObject, params string[] path) =>
			resolveObject(anyObject, (IEnumerable<string>)path);

		public static (Il2CppSystem.Reflection.FieldInfo field, Il2CppSystem.Object container) resolveObject(Il2CppSystem.Object anyObject, IEnumerable<string> path)
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

			return resolveField(anyObject, anyObject.GetIl2CppType().GetField(requested), path);
		}
	}
}
