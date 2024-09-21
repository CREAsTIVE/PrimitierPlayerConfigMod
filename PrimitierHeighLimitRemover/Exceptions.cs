using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitierPlayerConfig.Excpetions
{
	class RuntimeException : Exception { public RuntimeException(string msg) : base(msg) { } }
	class RuntimeArgumentException : RuntimeException { public RuntimeArgumentException(string msg) : base(msg) { } }
}
