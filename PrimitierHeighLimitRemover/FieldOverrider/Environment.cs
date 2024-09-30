using Il2CppSystem.Linq.Expressions;
using Il2CppSystem.Reflection;
using PrimitierPlayerConfig.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitierPlayerConfig.FieldOverrider
{
    public class Environment
	{
		public Environment(List<Variable> defaultVariables)
		{
			variables = new (defaultVariables);
		}

		List<Variable> variables;

		public void AddVariable(Variable var) => variables.Add(var);
		public void AddVariable(Variable var, string name) => variables.Add(var.Apply(e => e.Name = name));

		public Variable RequestVariable(string name) =>
			variables.First(x => x.Name == name);
			

		public void Clear()
		{
			foreach (var variable in variables)
				if (variable is HookedVariable hooked) 
					hooked.Clear();
		}
	}

	public static class EnvironmentExtensions
	{
		public static HookedVariable SetEnv(this HookedVariable variable, Environment env) => variable.Apply((_) => env.AddVariable(variable));
	}

	public abstract class Variable
	{
		public string? Name = null;
		public abstract double Value { get; }
	}

	public class ValueVariable : Variable
	{
		public ValueVariable(double val) => value = val;

		double value;
		public override double Value => value;
	}

	// That variable affect on another variables
	public abstract class HookedVariable : Variable
	{
		protected LinkedList<HookedVariable> notifyChildrenHooked = new();
		public virtual void OnParentValueChange() =>
notifyChildrenHooked.ForEach(hook => hook.OnParentValueChange());

		// This variable value depends on hook variable value
		public void HookFor(HookedVariable hook) => hook.notifyChildrenHooked.AddLast(this);

		public void Clear() => notifyChildrenHooked.Clear();
	}

	public class SettableHookedVariable : HookedVariable
	{
		public SettableHookedVariable(double initialValue) =>
			value = initialValue;

		double value = 0;
		public double SettableValue { get => value; 
			set { this.value = value; OnParentValueChange(); } }
		public override double Value => value;
	}
	public class HookedSetter : HookedVariable
	{
		HookedVariable source;
		Action<double> setAction;
		public HookedSetter(HookedVariable source, Action<double> setAction)
		{
			this.source = source;
			HookFor(source);
			this.setAction = setAction;
			setAction(source.Value);
		}

		public override void OnParentValueChange()
		{
			setAction(source.Value);
			base.OnParentValueChange();
		}

		public override double Value => source.Value;
	}
	public class Expression : HookedVariable
	{
		protected List<(Variable variable, int insertPosition)> requiredVariables = new();

		double? lastCalculatedvalue = null;

		string expr;

		public Expression(string expression, Environment environment)
		{
			var (vars, fixedExpression) = extractVarables(expression);

			foreach (var varNamePos in vars)
			{
				var variable = environment.RequestVariable(varNamePos.Value);

				requiredVariables.Add((variable, varNamePos.Key));

				if (variable is HookedVariable hooked)
					HookFor(hooked);
			}

			expr = fixedExpression;

			requiredVariables.Sort((a, b) => a.insertPosition - b.insertPosition);
		}

		public override double Value => lastCalculatedvalue ?? (lastCalculatedvalue = calculateValue()) ?? throw new();
		public override void OnParentValueChange()
		{
			lastCalculatedvalue = null;
			base.OnParentValueChange();
		}

		static MathParser parser = new('.');

		double calculateValue()
		{
			
			var patchedExpr = expr;
			PrimitierPlayerConfigMod.Logger?.Msg($"Trying to insert for: \"{expr}\"");
			foreach (var variable in requiredVariables)
				patchedExpr = patchedExpr.Insert(variable.insertPosition, ((float)variable.variable.Value).FString());
			PrimitierPlayerConfigMod.Logger?.Msg($"Insertion for \"{expr}\": \"{patchedExpr}\". Calculating...");
			return parser.Parse(patchedExpr);
        }

		// WARNING: I m too lazy to write that function normaly, so there are laze way:
		(Dictionary<int, string> vars, string fixedExpression) extractVarables(string expr)
		{
			Dictionary<int, string> vars = new();

			string newExpr = "";

			bool reading = false;
			string currentVarName = "";
			int counter = 0;
			foreach (var s in expr)
			{
				if (!reading)
				{
					if (s == '$')
						reading = true;
					else
						newExpr += s;
				}
				else
				{
					if (char.IsLetter(s) || char.IsDigit(s))
						currentVarName += s;
					else
					{
						newExpr += s;
						vars[counter] = currentVarName;
						reading = false;
						currentVarName = "";
						counter++;
					}
				}
				if (!reading)
					counter++;
			}
			if (reading)
				vars[counter] = currentVarName;
			return (vars, newExpr);
		}
	}
}
