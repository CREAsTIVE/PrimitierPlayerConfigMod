using Il2CppInterop.Runtime;
using System.Data;
using UnityEngine;

namespace PrimitierPlayerConfig.Utils
{
	internal static class UnityUtils
	{
		public static GameObject? FindGameObject(string name) =>
			FindGameObject(obj => obj.name == name);
		public static GameObject? FindGameObject(Func<GameObject, bool> predicate) =>
			FindObject<Transform>(t => predicate(t.gameObject))?.gameObject;
		public static UnityEngine.Object? FindObject(Func<UnityEngine.Object, bool> predicate) =>
			FindObject<UnityEngine.Object>(predicate);
		public static T? FindObject<T>(Func<T, bool> predicate) where T : UnityEngine.Object =>
			UnityEngine.Object.FindObjectsOfTypeAll(Il2CppType.Of<T>()).Select(obj => obj.Cast<T>()).FirstOrDefault(predicate);

		public static UnityEngine.Object? FindObjectOfTypeName(string typeName) =>
			FindObject(obj => obj.GetIl2CppType().FullName == typeName || obj.GetType().FullName == typeName);
	}
}