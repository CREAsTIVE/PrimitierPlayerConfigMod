using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitierPlayerConfig.Excpetions
{
	public class RuntimeException : Exception { public RuntimeException(string msg) : base(msg) { } public RuntimeException(string msg, Exception inner) : base(msg, inner) { } }
	public class RuntimeArgumentException : RuntimeException { public RuntimeArgumentException(string msg) : base(msg) { } }
}
