namespace PrimitierPlayerConfig.Utils
{
	internal static class ObjectUtils
	{

		public static T Apply<T>(this T obj, Action<T> act)
		{
			act(obj);
			return obj;
		}

		public static R Let<T, R>(this T obj, Func<T, R> act) => act(obj);
		public static bool NoneIsNull(params Il2CppSystem.Object[] objects) => !objects.Contains((e) => e == null);
	}
}