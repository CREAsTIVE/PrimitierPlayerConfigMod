// See https://aka.ms/new-console-template for more information
using PrimitierPlayerConfig;
using PrimitierPlayerConfig.FieldOverrider;

var env = new PrimitierPlayerConfig.FieldOverrider.Environment();

env.AddVariable(new ValueVariable(5), "a");
var testHook = new SettableHookedVariable().Apply(e => e.SettableValue = 10);
env.AddVariable(testHook, "b");
var expr = new Expression("$a+$b", env);
env.AddVariable(expr);
new HookedSetter(expr, () => Console.WriteLine(expr.Value));

testHook.SettableValue = 4;