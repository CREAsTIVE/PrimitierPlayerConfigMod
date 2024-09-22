using Il2CppInterop.Runtime;
using Il2CppSystem.Reflection;
using PrimitierPlayerConfig.Excpetions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PrimitierPlayerConfig
{
	public class FieldResolveException : RuntimeException
	{
		public FieldResolveException(string msg) : base(msg) { }
		public FieldResolveException(string msg, Exception inner) : base(msg, inner) { }
	}
	static class FieldResolver
	{
		private const BindingFlags AllBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		private const System.Reflection.BindingFlags AllBindingsNative =
			System.Reflection.BindingFlags.Public |
			System.Reflection.BindingFlags.NonPublic |
			System.Reflection.BindingFlags.Static |
			System.Reflection.BindingFlags.Instance;

		public static (Action<Il2CppSystem.Object> setter, Il2CppSystem.Type fieldType) resolveField(Il2CppSystem.Object container, FieldInfo field, IEnumerable<string> path)
		{
			try
			{
				if (!path.Any())
					return ((val) => field.SetValue(container, val), field.FieldType);
				return resolveObject(field.GetValue(container), path);
			} catch (Exception e) { throw new FieldResolveException($"Can't find field {path.First()} at {container} of {container.GetIl2CppType().FullName} type", e); }
		}
		public static (Action<Il2CppSystem.Object> setter, Il2CppSystem.Type fieldType) resolveField(Il2CppSystem.Object container, PropertyInfo field, IEnumerable<string> path)
		{
			try
			{
				if (!path.Any())
					return ((val) => field.SetValue(container, val), field.PropertyType);
				return resolveObject(field.GetValue(container), path);
			}catch (Exception e) { throw new FieldResolveException($"Can't find field {path.First()} at {container} of {container.GetIl2CppType().FullName} type", e); }
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


			/*MemberInfo member = anyObject.GetIl2CppType().GetMember(requested, 
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic
				).First();*/

			

			/*var baseType = anyObject.GetIl2CppType()!;
			PrimitierPlayerConfigMod.Logger?.Msg($"Listing base types for {baseType.FullName}");
			do {
				PrimitierPlayerConfigMod.Logger?.Msg($"base type: {baseType.FullName}");
				PrimitierPlayerConfigMod.Logger?.Msg(
					$"fields:\n{string.Join("\n", baseType.GetMembers(AllBindings | BindingFlags.DeclaredOnly).Select(i => i.Name))}\n\n");
				baseType = baseType.BaseType!;
			} while (!(baseType.IsEquivalentTo(Il2CppType.Of<Il2CppSystem.Object>())));*/




			var cj = anyObject.TryCast<ConfigurableJoint>();


			if (cj != null && !path.Any())
			{
				if (requested == "massScale")
					return ((v) => cj.massScale = v.Unbox<float>(), Il2CppType.Of<float>());
				else if (requested == "connectedMassScale")
					return ((v) => cj.connectedMassScale = v.Unbox<float>(), Il2CppType.Of<float>());
					
			}

			MemberInfo member = anyObject.GetIl2CppType()
				.GetMember(requested, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.First();


			var field = member.TryCast<FieldInfo>();
			if (field != null)
				return resolveField(anyObject, field, path);

			var prop = member.TryCast<PropertyInfo>();
			if (prop != null)
				return resolveField(anyObject, prop, path);

			throw new FieldResolveException($"Unknow property type: {member.MemberType}");
		}

		static IEnumerable<MemberInfo> GetAllMembersRecursivly(Il2CppSystem.Type type) =>
			type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
			.Concat(type.BaseType == Il2CppType.Of<Il2CppSystem.Object>() ? new MemberInfo[0] : GetAllMembersRecursivly(type.BaseType));
	}
}
