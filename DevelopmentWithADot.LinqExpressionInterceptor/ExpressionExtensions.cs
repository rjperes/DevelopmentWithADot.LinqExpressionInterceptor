using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DevelopmentWithADot.LinqExpressionInterceptor
{
	public static class ExpressionExtensions
	{
		private static readonly MethodInfo takeMethod = typeof(Queryable).GetMethod("Take", BindingFlags.Public | BindingFlags.Static);
		private static readonly MethodInfo skipMethod = typeof(Queryable).GetMethod("Skip", BindingFlags.Public | BindingFlags.Static);
		private static readonly MethodInfo whereMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "Where").First();
		private static readonly MethodInfo selectMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "Select").First();
		private static readonly MethodInfo orderByMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "OrderBy").First();
		private static readonly MethodInfo orderByDescendingMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "OrderByDescending").First();
		private static readonly MethodInfo thenByMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "ThenBy").First();
		private static readonly MethodInfo thenByDescendingMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "ThenByDescending").First();

		public static Boolean IsInsideWhere(this IContextfulVisitor visitor)
		{
			return (visitor.Stack.OfType<MethodCallExpression>().Any(x => x.IsWhere() == true));
		}

		public static Boolean IsBoolean(this Expression expression)
		{
			return
			(
				((expression is UnaryExpression) && (expression as UnaryExpression).IsBoolean() == true) ||
				((expression is MethodCallExpression) && (expression as MethodCallExpression).IsBoolean() == true) ||
				((expression is LambdaExpression) && (expression as LambdaExpression).IsBoolean() == true) ||
				(expression.Type == typeof(Boolean))
			);
		}

		public static Boolean IsBoolean(this MethodCallExpression expression)
		{
			return (expression.Method.ReturnType == typeof(Boolean));
		}

		public static Boolean IsBoolean(this LambdaExpression expression)
		{
			return (expression.Body.Type == typeof(Boolean));
		}

		public static Boolean IsBoolean(this UnaryExpression expression)
		{
			return (expression.Operand.IsBoolean());
		}

		public static Boolean IsWhere(this MethodCallExpression expression)
		{
			return ((expression.Method.IsGenericMethod == true) && (expression.Method.GetGenericMethodDefinition() == whereMethod));
		}

		public static Boolean IsTake(this MethodCallExpression expression)
		{
			return ((expression.Method.IsGenericMethod == true) && (expression.Method.GetGenericMethodDefinition() == takeMethod));
		}

		public static Boolean IsSkip(this MethodCallExpression expression)
		{
			return ((expression.Method.IsGenericMethod == true) && (expression.Method.GetGenericMethodDefinition() == skipMethod));
		}

		public static Boolean IsSelect(this MethodCallExpression expression)
		{
			return ((expression.Method.IsGenericMethod == true) && (expression.Method.GetGenericMethodDefinition() == selectMethod));
		}

		public static Boolean IsOrderBy(this MethodCallExpression expression)
		{
			return ((expression.Method.IsGenericMethod == true) && (expression.Method.GetGenericMethodDefinition() == orderByMethod));
		}

		public static Boolean IsOrderByDescending(this MethodCallExpression expression)
		{
			return ((expression.Method.IsGenericMethod == true) && (expression.Method.GetGenericMethodDefinition() == orderByDescendingMethod));
		}

		public static Boolean IsThenBy(this MethodCallExpression expression)
		{
			return ((expression.Method.IsGenericMethod == true) && (expression.Method.GetGenericMethodDefinition() == thenByMethod));
		}

		public static Boolean IsThenByDescending(this MethodCallExpression expression)
		{
			return ((expression.Method.IsGenericMethod == true) && (expression.Method.GetGenericMethodDefinition() == thenByDescendingMethod));
		}

		public static Boolean IsExtension(this MethodCallExpression expression)
		{
			return ((expression.Method.DeclaringType != typeof(Queryable)) && (expression.Method.IsStatic == true) && (expression.Method.IsDefined(typeof(ExtensionAttribute), false) == true));
		}

		public static IEnumerable<Expression> GetChildren(this Expression expression)
		{
			List<Expression> expressions = new List<Expression>();

			foreach (PropertyInfo prop in expression.GetType().GetProperties().Where(x => typeof(Expression).IsAssignableFrom(x.PropertyType) || typeof(IEnumerable<Expression>).IsAssignableFrom(x.PropertyType) == true))
			{
				if (typeof(Expression).IsAssignableFrom(prop.PropertyType) == true)
				{
					Expression exp = prop.GetValue(expression, null) as Expression;

					if (exp != null)
					{
						expressions.Add(exp);
					}
				}
				else
				{
					expressions.AddRange(prop.GetValue(expression, null) as IEnumerable<Expression>);
				}
			}

			return (expressions);
		}
		
		/*public static IEnumerable<Expression> GetChildren(this Expression expression)
		{
			if (expression is BinaryExpression)
			{
				yield return ((expression as BinaryExpression).Left);
				yield return ((expression as BinaryExpression).Right);
				yield return ((expression as BinaryExpression).Conversion);
			}
			else if (expression is BlockExpression)
			{
				foreach (ParameterExpression argument in (expression as BlockExpression).Variables)
				{
					yield return (argument);
				}

				foreach (Expression argument in (expression as BlockExpression).Expressions)
				{
					yield return (argument);
				}
			}
			else if (expression is ConditionalExpression)
			{
				yield return ((expression as ConditionalExpression).Test);
				yield return ((expression as ConditionalExpression).IfTrue);
				yield return ((expression as ConditionalExpression).IfFalse);
			}
			else if (expression is DynamicExpression)
			{
				foreach (Expression argument in (expression as DynamicExpression).Arguments)
				{
					yield return (argument);
				}
			}
			else if (expression is GotoExpression)
			{
				yield return ((expression as GotoExpression).Value);
			}
			else if (expression is IndexExpression)
			{
				yield return ((expression as IndexExpression).Object);

				foreach (Expression argument in (expression as InvocationExpression).Arguments)
				{
					yield return (argument);
				}
			}
			else if (expression is InvocationExpression)
			{
				yield return ((expression as InvocationExpression).Expression);

				foreach (Expression argument in (expression as InvocationExpression).Arguments)
				{
					yield return (argument);
				}
			}
			else if (expression is LabelExpression)
			{
				yield return ((expression as LabelExpression).DefaultValue);
			}
			else if (expression is LambdaExpression)
			{
				yield return ((expression as LambdaExpression).Body);

				foreach (Expression parameter in (expression as LambdaExpression).Parameters)
				{
					yield return (parameter);
				}
			}
			else if (expression is ListInitExpression)
			{
				yield return ((expression as ListInitExpression).NewExpression);
			}
			else if (expression is LoopExpression)
			{
				yield return ((expression as LoopExpression).Body);
			}
			else if (expression is MemberExpression)
			{
				yield return ((expression as MemberExpression).Expression);
			}
			else if (expression is MemberInitExpression)
			{
				yield return ((expression as MemberInitExpression).NewExpression);
			}
			else if (expression is MethodCallExpression)
			{
				yield return ((expression as MethodCallExpression).Object);

				foreach (Expression argument in (expression as MethodCallExpression).Arguments)
				{
					yield return (argument);
				}
			}
			else if (expression is NewArrayExpression)
			{
				foreach (Expression argument in (expression as NewArrayExpression).Expressions)
				{
					yield return (argument);
				}
			}
			else if (expression is NewExpression)
			{
				foreach (Expression argument in (expression as NewExpression).Arguments)
				{
					yield return (argument);
				}
			}
			
			else if (expression is RuntimeVariablesExpression)
			{
				foreach (ParameterExpression argument in (expression as RuntimeVariablesExpression).Variables)
				{
					yield return (argument);
				}
			}
			else if (expression is SwitchExpression)
			{
				yield return ((expression as SwitchExpression).DefaultBody);
				yield return ((expression as SwitchExpression).SwitchValue);
			}
			else if (expression is TryExpression)
			{
				yield return ((expression as TryExpression).Body);
				yield return ((expression as TryExpression).Fault);
				yield return ((expression as TryExpression).Finally);
			}
			else if (expression is TypeBinaryExpression)
			{
				yield return ((expression as TypeBinaryExpression).Expression);
			}
			else if (expression is UnaryExpression)
			{
				yield return ((expression as UnaryExpression).Operand);
			}
		}*/
	}
}
