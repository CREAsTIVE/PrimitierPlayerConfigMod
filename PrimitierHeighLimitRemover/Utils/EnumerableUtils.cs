namespace PrimitierPlayerConfig.Utils
{
	internal static class EnumerableUtils
	{

		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var item in enumerable)
				action(item);
		}
	}
}