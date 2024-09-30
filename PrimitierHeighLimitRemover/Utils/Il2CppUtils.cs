using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitierPlayerConfig.Utils
{
	public static class Il2CppUtils
	{
		public static Type GetTypeByName(string name) =>
			AppDomain.CurrentDomain.GetAssemblies().Reverse()
			.Select(assembly => assembly.GetType(name))
			.FirstOrDefault(name => name != null) ?? throw new();

		public static Il2CppSystem.Type GetIl2CppTypeByName(string name) =>
			Il2CppType.From(GetTypeByName(name));
	}
}
