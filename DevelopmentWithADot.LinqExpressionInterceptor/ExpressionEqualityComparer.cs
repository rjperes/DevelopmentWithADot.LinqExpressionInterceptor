using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DevelopmentWithADot.LinqExpressionInterceptor
{
	/*
	BinaryExpression
	BlockExpression
	ConditionalExpression
	ConstantExpression
	DebugInfoExpression
	DefaultExpression
	DynamicExpression
	Expression
	Expression<T> = LambaExpression
	GotoExpression
	IndexExpression
	InvocationExpression
	LabelExpression
	LambdaExpression
	ListInitExpression
	LoopExpression
	MemberExpression
	MemberInitExpression
	MethodCallExpression
	NewArrayExpression
	NewExpression
	ParameterExpression
	RuntimeVariablesExpression
	SwitchExpression
	TryExpression
	TypeBinaryExpression
	UnaryExpression	  
	*/

	public sealed class ExpressionEqualityComparer : IEqualityComparer<Expression>, IContextfulVisitor
	{
		private sealed class Enqueuer : IDisposable
		{
			private readonly Stack<Expression> stack;

			internal Enqueuer(Expression expression, Stack<Expression> stack)
			{
				this.stack = stack;
				this.stack.Push(expression);
			}

			void IDisposable.Dispose()
			{
				this.stack.Pop();
			}
		}

		#region Private methods
		private IDisposable Enqueue(Expression expression)
		{
			return (new Enqueuer(expression, this.stack));
		}
		#endregion

		#region Private fields
		private Int32 hashCode;
		private readonly Stack<Expression> stack = new Stack<Expression>();
		#endregion

		#region Hash Code
		private void Visit(UnaryExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if (expression.Method != null)
				{
					this.hashCode ^= expression.Method.GetHashCode();
				}

				this.hashCode ^= expression.IsLifted.GetHashCode();
				this.hashCode ^= expression.IsLiftedToNull.GetHashCode();

				this.Visit(expression.Operand);
			}
		}

		private void Visit(BinaryExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if (expression.Method != null)
				{
					this.hashCode ^= expression.Method.GetHashCode();
				}
				
				this.hashCode ^= expression.IsLifted.GetHashCode();
				this.hashCode ^= expression.IsLiftedToNull.GetHashCode();

				this.Visit(expression.Left);
				this.Visit(expression.Right);
				this.Visit(expression.Conversion);
			}
		}

		private void Visit(BlockExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.Visit(expression.Variables);
				this.Visit(expression.Result);
				this.Visit(expression.Expressions);
			}
		}

		private void Visit(MethodCallExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Method.GetHashCode();

				this.Visit(expression.Object);
				this.Visit(expression.Arguments);
			}
		}

		private void Visit(ConditionalExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.Visit(expression.Test);
				this.Visit(expression.IfTrue);
				this.Visit(expression.IfFalse);
			}
		}

		private void Visit(DefaultExpression expression)
		{
			using (this.Enqueue(expression))
			{
			}
		}

		private void Visit(DynamicExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Binder.GetHashCode();
				this.hashCode ^= expression.DelegateType.GetHashCode();
				this.Visit(expression.Arguments);
			}
		}

		private void Visit(DebugInfoExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Document.GetHashCode();
				this.hashCode ^= expression.EndColumn.GetHashCode();
				this.hashCode ^= expression.EndLine.GetHashCode();
				this.hashCode ^= expression.IsClear.GetHashCode();
				this.hashCode ^= expression.StartColumn.GetHashCode();
				this.hashCode ^= expression.StartLine.GetHashCode();
			}
		}

		private void Visit(ConstantExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if (expression.Value != null)
				{
					this.hashCode ^= expression.Value.GetHashCode();
				}
			}
		}

		private void Visit(GotoExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Kind.GetHashCode();
				this.hashCode ^= expression.Target.GetHashCode();

				this.Visit(expression.Value);
			}
		}

		private void Visit(IndexExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Indexer.GetHashCode();

				this.Visit(expression.Object);
				this.Visit(expression.Arguments);
			}
		}

		private void Visit(InvocationExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.Visit(expression.Expression);
				this.Visit(expression.Arguments);
			}
		}

		private void Visit(LabelExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Target.GetHashCode();

				this.Visit(expression.DefaultValue);
			}
		}

		private void Visit(LambdaExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.Visit(expression.Body);
				this.Visit(expression.Parameters);
			}
		}

		private void Visit(LoopExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.BreakLabel.GetHashCode();
				this.hashCode ^= expression.ContinueLabel.GetHashCode();

				this.Visit(expression.Body);
			}				
		}

		private void Visit(ListInitExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.Visit(expression.NewExpression);
				this.Visit(expression.Initializers);
			}
		}

		private void Visit(MemberExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Member.GetHashCode();
				this.Visit(expression.Expression);
			}
		}

		private void Visit(MemberInitExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.Visit(expression.NewExpression);
				this.Visit(expression.Bindings);
			}
		}

		private void Visit(NewExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Constructor.GetHashCode();

				this.Visit(expression.Members);
				this.Visit(expression.Arguments);
			}
		}

		private void Visit(NewArrayExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.Visit(expression.Expressions);
			}
		}

		private void Visit(ParameterExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if (expression.Type != null)
				{
					this.hashCode ^= expression.Type.GetHashCode();
				}
			}
		}

		private void Visit(RuntimeVariablesExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.Visit(expression.Variables);
			}
		}

		private void Visit(SwitchExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.Comparison.GetHashCode();
				this.Visit(expression.DefaultBody);
				this.Visit(expression.SwitchValue);
				this.Visit(expression.Cases);
			}
		}

		private void Visit(TryExpression expression)
		{
			using (this.Enqueue(expression))
			{				
				this.Visit(expression.Body);
				this.Visit(expression.Fault);
				this.Visit(expression.Finally);
				this.Visit(expression.Handlers);
			}
		}

		private void Visit(TypeBinaryExpression expression)
		{
			using (this.Enqueue(expression))
			{
				this.hashCode ^= expression.TypeOperand.GetHashCode();
				this.Visit(expression.Expression);
			}
		}

		private void Visit(SwitchCase switchCase)
		{
			this.Visit(switchCase.Body);
			this.Visit(switchCase.TestValues);
		}

		private void Visit(CatchBlock block)
		{
			this.hashCode ^= (Int32)block.Test.GetHashCode();

			this.Visit(block.Body);
			this.Visit(block.Filter);
			this.Visit(block.Variable);
		}

		private void Visit(MemberBinding binding)
		{
			this.hashCode ^= (Int32)binding.BindingType ^ binding.Member.GetHashCode();

			switch (binding.BindingType)
			{
				case MemberBindingType.Assignment:
					this.Visit((MemberAssignment)binding);
					break;

				case MemberBindingType.MemberBinding:
					this.Visit((MemberMemberBinding)binding);
					break;

				case MemberBindingType.ListBinding:
					this.Visit((MemberListBinding)binding);
					break;
				
				default:
					throw (new ArgumentException("Unhandled binding type"));
			}
		}

		private void Visit(MemberAssignment assignment)
		{
			this.Visit(assignment.Expression);
		}

		private void Visit(MemberMemberBinding binding)
		{
			this.Visit(binding.Bindings);
		}

		private void Visit(MemberListBinding binding)
		{
			this.Visit(binding.Initializers);
		}

		private void Visit(ElementInit initializer)
		{
			this.hashCode ^= initializer.AddMethod.GetHashCode();

			this.Visit(initializer.Arguments);
		}

		private void Visit(ReadOnlyCollection<Expression> list)
		{
			if (list != null)
			{
				for (Int32 i = 0; i < list.Count; i++)
				{
					this.Visit(list[i]);
				}
			}
		}

		private void Visit(ReadOnlyCollection<SwitchCase> list)
		{
			if (list != null)
			{
				for (Int32 i = 0; i < list.Count; i++)
				{
					this.Visit(list[i]);
				}
			}
		}

		private void Visit(ReadOnlyCollection<CatchBlock> list)
		{
			if (list != null)
			{
				for (Int32 i = 0; i < list.Count; i++)
				{
					this.Visit(list[i]);
				}
			}
		}

		private void Visit(ReadOnlyCollection<ParameterExpression> list)
		{
			if (list != null)
			{
				for (Int32 i = 0; i < list.Count; i++)
				{
					this.Visit(list[i]);
				}
			}
		}

		private void Visit(ReadOnlyCollection<MemberBinding> list)
		{
			if (list != null)
			{
				for (Int32 i = 0; i < list.Count; i++)
				{
					this.Visit(list[i]);
				}
			}
		}

		private void Visit(ReadOnlyCollection<ElementInit> list)
		{
			if (list != null)
			{
				for (Int32 i = 0; i < list.Count; i++)
				{
					this.Visit(list[i]);
				}
			}
		}

		private void Visit(ReadOnlyCollection<MemberInfo> list)
		{
			if (list != null)
			{
				for (Int32 i = 0; i < list.Count; i++)
				{
					this.hashCode ^= list[i].GetHashCode();
				}
			}
		}
		#endregion

		#region Equality
		private Boolean Equals(UnaryExpression x, UnaryExpression y)
		{
			return ((x.Method == y.Method) &&
					(x.IsLifted == y.IsLifted) &&
					(x.IsLiftedToNull == y.IsLiftedToNull) &&
				    (this.Equals(x.Operand, y.Operand)));
		}

		private Boolean Equals(BlockExpression x, BlockExpression y)
		{
			if ((x.Variables.Count != y.Variables.Count) || (x.Expressions.Count != y.Expressions.Count))
			{
				return (false);
			}

			for (Int32 i = 0; i < x.Variables.Count; i++)
			{
				if (this.Equals(x.Variables[i], y.Variables[i]) == false)
				{
					return (false);
				}
			}

			for (Int32 i = 0; i < x.Expressions.Count; i++)
			{
				if (this.Equals(x.Expressions[i], y.Expressions[i]) == false)
				{
					return (false);
				}
			}

			return (this.Equals(x.Result, y.Result));
		}

		private Boolean Equals(BinaryExpression x, BinaryExpression y)
		{
			return ((x.Method == y.Method) &&
				   (x.IsLifted == y.IsLifted) &&
				   (x.IsLiftedToNull == y.IsLiftedToNull) &&
				   (this.Equals(x.Right, y.Right)) &&
				   (this.Equals(x.Left, y.Left)) &&
				   (this.Equals(x.Right, y.Right)) &&
				   (this.Equals(x.Conversion, y.Conversion)));
		}

		private Boolean Equals(MethodCallExpression x, MethodCallExpression y)
		{
			return ((x.Method == y.Method) &&
				   (this.Equals(x.Object, y.Object)) &&
				   (this.Equals(x.Arguments, y.Arguments)));
		}

		private Boolean Equals(ConditionalExpression x, ConditionalExpression y)
		{
			return ((this.Equals(x.Test, y.Test)) &&
				   (this.Equals(x.IfTrue, y.IfTrue)) &&
				   (this.Equals(x.IfFalse, y.IfFalse)));
		}

		private Boolean Equals(ConstantExpression x, ConstantExpression y)
		{
			return (Object.Equals(x.Value, y.Value));
		}

		private Boolean Equals(DebugInfoExpression x, DebugInfoExpression y)
		{
			return ((Object.Equals(x.Document, y.Document)) &&
				(x.EndColumn == y.EndColumn) &&
				(x.EndLine == y.EndLine) &&
				(x.IsClear == y.IsClear) &&
				(x.StartColumn == y.StartColumn) &&
				(x.StartLine == y.StartLine));
		}

		private Boolean Equals(GotoExpression x, GotoExpression y)
		{
			return ((x.Kind == y.Kind) &&
				(this.Equals(x.Target, y.Target) &&
				(this.Equals(x.Value, y.Value))));
		}

		private Boolean Equals(IndexExpression x, IndexExpression y)
		{
			if (x.Arguments.Count != y.Arguments.Count)
			{
				return (false);
			}

			for (Int32 i = 0; i < x.Arguments.Count; i++)
			{
				if (this.Equals(x.Arguments[i], y.Arguments[i]) == false)
				{
					return (false);
				}
			}

			return((x.Indexer == y.Indexer) &&
				(this.Equals(x.Object, y.Object)));
		}

		private Boolean Equals(InvocationExpression x, InvocationExpression y)
		{
			return ((this.Equals(x.Expression, y.Expression)) &&
				   (this.Equals(x.Arguments, x.Arguments)));
		}

		private Boolean Equals(LabelExpression x, LabelExpression y)
		{
			return ((this.Equals(x.DefaultValue, y.DefaultValue)) &&
				   (this.Equals(x.Target, y.Target)));
		}

		private Boolean Equals(LambdaExpression x, LambdaExpression y)
		{
			return ((this.Equals(x.Body, y.Body)) &&
				   (this.Equals(x.Parameters, y.Parameters)));
		}

		private Boolean Equals(LoopExpression x, LoopExpression y)
		{
			return ((this.Equals(x.Body, y.Body)) &&
				   (this.Equals(x.BreakLabel, y.BreakLabel)) &&
				   (this.Equals(x.ContinueLabel, x.ContinueLabel)));
		}

		private Boolean Equals(ListInitExpression x, ListInitExpression y)
		{
			return ((this.Equals(x.NewExpression, y.NewExpression)) &&
				   (this.Equals(x.Initializers, y.Initializers)));
		}

		private Boolean Equals(MemberExpression x, MemberExpression y)
		{
			return ((x.Member == y.Member) &&
				   (this.Equals(x.Expression, y.Expression)));
		}

		private Boolean Equals(MemberInitExpression x, MemberInitExpression y)
		{
			return ((this.Equals(x.NewExpression, y.NewExpression)) &&
				   (this.Equals(x.Bindings, y.Bindings)));
		}

		private Boolean Equals(NewExpression x, NewExpression y)
		{
			return ((x.Constructor == y.Constructor) &&
				   (this.Equals(x.Members, y.Members)) &&
				   (this.Equals(x.Arguments, y.Arguments)));
		}

		private Boolean Equals(NewArrayExpression x, NewArrayExpression y)
		{
			return (this.Equals(x.Expressions, y.Expressions));
		}

		private Boolean Equals(ParameterExpression x, ParameterExpression y)
		{
			return ((x.Type == y.Type) && (x.IsByRef == y.IsByRef));
		}

		private Boolean Equals(RuntimeVariablesExpression x, RuntimeVariablesExpression y)
		{
			if (x.Variables.Count != y.Variables.Count)
			{
				return (false);
			}

			for (Int32 i = 0; i < x.Variables.Count; i++)
			{
				if (this.Equals(x.Variables[i], y.Variables[i]) == false)
				{
					return (false);
				}
			}

			return (true);
		}

		private Boolean Equals(SwitchCase x, SwitchCase y)
		{
			return ((this.Equals(x.Body, y.Body)) &&
				(this.Equals(x.TestValues, y.TestValues)));
		}

		private Boolean Equals(SwitchExpression x, SwitchExpression y)
		{
			if (x.Cases.Count != y.Cases.Count)
			{
				return (false);
			}

			for (Int32 i = 0; i < x.Cases.Count; i++)
			{
				if (this.Equals(x.Cases[i], y.Cases[i]) == false)
				{
					return (false);
				}
			}

			return ((x.Comparison == y.Comparison) &&
				(this.Equals(x.DefaultBody, y.DefaultBody)) &&
				(this.Equals(x.SwitchValue, y.SwitchValue)));
		}

		private Boolean Equals(TypeBinaryExpression x, TypeBinaryExpression y)
		{
			return ((x.TypeOperand == y.TypeOperand) &&
				   (this.Equals(x.Expression, y.Expression)));
		}

		private Boolean Equals(TryExpression x, TryExpression y)
		{
			if (x.Handlers.Count != y.Handlers.Count)
			{
				return(false);
			}

			for (Int32 i = 0; i < x.Handlers.Count; i++)
			{
				if (this.Equals(x.Handlers[i], y.Handlers[i]) == false)
				{
					return (false);
				}
			}

			return ((this.Equals(x.Body, y.Body) &&
				(this.Equals(x.Fault, y.Fault)) &&
				(this.Equals(x.Finally, y.Finally))));
		}

		private Boolean Equals(CatchBlock x, CatchBlock y)
		{
			return ((x.Test == y.Test) &&
				(this.Equals(x.Body, y.Body)) &&
				((this.Equals(x.Filter, y.Filter)) &&
				(this.Equals(x.Variable, y.Variable))));
		}

		private Boolean Equals(LabelTarget x, LabelTarget y)
		{
			return ((x.Name == y.Name) &&
				(x.Type == y.Type));
		}

		private Boolean Equals(MemberBinding x, MemberBinding y)
		{
			if ((x.BindingType != y.BindingType) || (x.Member != y.Member))
			{
				return (false);
			}

			switch (x.BindingType)
			{
				case MemberBindingType.Assignment:
					return (this.Equals((MemberAssignment)x, (MemberAssignment)y));

				case MemberBindingType.MemberBinding:
					return (this.Equals((MemberMemberBinding)x, (MemberMemberBinding)y));

				case MemberBindingType.ListBinding:
					return (this.Equals((MemberListBinding)x, (MemberListBinding)y));

				default:
					throw (new ArgumentException("Unhandled binding type"));
			}
		}

		private Boolean Equals(MemberAssignment x, MemberAssignment y)
		{
			return (this.Equals(x.Expression, y.Expression));
		}

		private Boolean Equals(MemberMemberBinding x, MemberMemberBinding y)
		{
			return (this.Equals(x.Bindings, y.Bindings));
		}

		private Boolean Equals(MemberListBinding x, MemberListBinding y)
		{
			return (this.Equals(x.Initializers, y.Initializers));
		}

		private Boolean Equals(DefaultExpression x, DefaultExpression y)
		{
			return (true);
		}

		private Boolean Equals(DynamicExpression x, DynamicExpression y)
		{
			if (x.Arguments.Count != y.Arguments.Count)
			{
				return (false);
			}

			for (Int32 i = 0; i < x.Arguments.Count; i++)
			{
				if (this.Equals(x.Arguments[i], y.Arguments[i]) == false)
				{
					return (false);
				}
			}

			return ((Object.Equals(x.Binder, y.Binder)) &&
				(x.DelegateType == y.DelegateType));
		}

		private Boolean Equals(ElementInit x, ElementInit y)
		{
			return ((x.AddMethod == y.AddMethod) &&
				   (this.Equals(x.Arguments, y.Arguments)));
		}

		private Boolean Equals(ReadOnlyCollection<Expression> x, ReadOnlyCollection<Expression> y)
		{
			if (x == y)
			{
				return (true);
			}

			if ((x != null) && (y != null) && (x.Count == y.Count))
			{
				for (Int32 i = 0; i < x.Count; i++)
				{
					if (this.Equals(x[i], y[i]) == false)
					{
						return (false);
					}
				}

				return (true);
			}

			return (false);
		}

		private Boolean Equals(ReadOnlyCollection<ParameterExpression> x, ReadOnlyCollection<ParameterExpression> y)
		{
			if (x == y)
			{
				return (true);
			}

			if ((x != null) && (y != null) && (x.Count == y.Count))
			{
				for (Int32 i = 0; i < x.Count; i++)
				{
					if (this.Equals(x[i], y[i]) == false)
					{
						return (false);
					}
				}

				return (true);
			}

			return (false);
		}

		private Boolean Equals(ReadOnlyCollection<MemberBinding> x, ReadOnlyCollection<MemberBinding> y)
		{
			if (x == y)
			{
				return (true);
			}

			if ((x != null) && (y != null) && (x.Count == y.Count))
			{
				for (Int32 i = 0; i < x.Count; i++)
				{
					if (this.Equals(x[i], y[i]) == false)
					{
						return (false);
					}
				}

				return (true);
			}

			return (false);
		}

		private Boolean Equals(ReadOnlyCollection<ElementInit> x, ReadOnlyCollection<ElementInit> y)
		{
			if (x == y)
			{
				return (true);
			}

			if ((x != null) && (y != null) && (x.Count == y.Count))
			{
				for (Int32 i = 0; i < x.Count; i++)
				{
					if (this.Equals(x[i], y[i]) == false)
					{
						return (false);
					}
				}

				return (true);
			}

			return (false);
		}

		private Boolean Equals(ReadOnlyCollection<MemberInfo> x, ReadOnlyCollection<MemberInfo> y)
		{
			if (x == y)
			{
				return (true);
			}

			if ((x != null) && (y != null) && (x.Count == y.Count))
			{
				for (Int32 i = 0; i < x.Count; i++)
				{
					if (x[i] != y[i])
					{
						return (false);
					}
				}

				return (true);
			}

			return (false);
		}
		#endregion

		#region IContextfulVisitor Members
		public Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				return (null);
			}

			this.hashCode ^= (Int32)expression.NodeType ^ expression.Type.GetHashCode();

			switch (expression.NodeType)
			{
				case ExpressionType.ArrayLength:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.Negate:
				case ExpressionType.UnaryPlus:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
					this.Visit((UnaryExpression)expression);
					break;

				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.ArrayIndex:
				case ExpressionType.Coalesce:
				case ExpressionType.Divide:
				case ExpressionType.Equal:
				case ExpressionType.ExclusiveOr:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LeftShift:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.Modulo:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.NotEqual:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.Power:
				case ExpressionType.RightShift:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
					this.Visit((BinaryExpression)expression);
					break;

				case ExpressionType.Block:
					this.Visit((BlockExpression)expression);
					break;

				case ExpressionType.Call:
					this.Visit((MethodCallExpression)expression);
					break;

				case ExpressionType.Conditional:
					this.Visit((ConditionalExpression)expression);
					break;

				case ExpressionType.Constant:
					this.Visit((ConstantExpression)expression);
					break;

				case ExpressionType.DebugInfo:
					this.Visit((DebugInfoExpression)expression);
					break;

				case ExpressionType.Default:
					this.Visit((DefaultExpression)expression);
					break;

				case ExpressionType.Dynamic:
					this.Visit((DynamicExpression)expression);
					break;

				case ExpressionType.Goto:
					this.Visit((GotoExpression)expression);
					break;

				case ExpressionType.Index:
					this.Visit((IndexExpression)expression);
					break;

				case ExpressionType.Invoke:
					this.Visit((InvocationExpression)expression);
					break;

				case ExpressionType.Label:
					this.Visit((LabelExpression)expression);
					break;

				case ExpressionType.Lambda:
					this.Visit((LambdaExpression)expression);
					break;

				case ExpressionType.ListInit:
					this.Visit((ListInitExpression)expression);
					break;

				case ExpressionType.Loop:
					this.Visit((LoopExpression)expression);
					break;

				case ExpressionType.MemberAccess:
					this.Visit((MemberExpression)expression);
					break;

				case ExpressionType.MemberInit:
					this.Visit((MemberInitExpression)expression);
					break;

				case ExpressionType.New:
					this.Visit((NewExpression)expression);
					break;

				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
					this.Visit((NewArrayExpression)expression);
					break;

				case ExpressionType.Parameter:
					this.Visit((ParameterExpression)expression);
					break;

				case ExpressionType.RuntimeVariables:
					this.Visit((RuntimeVariablesExpression)expression);
					break;

				case ExpressionType.Switch:
					this.Visit((SwitchExpression)expression);
					break;

				case ExpressionType.Try:
					this.Visit((TryExpression)expression);
					break;

				case ExpressionType.TypeIs:
					this.Visit((TypeBinaryExpression)expression);
					break;

				default:
					throw (new ArgumentException("Unhandled expression type"));
			}

			return (expression);
		}

		public Stack<Expression> Stack
		{
			get
			{
				return (this.stack);
			}
		}

		public Expression Current
		{
			get
			{
				return ((this.stack.Count != 0) ? this.stack.Peek() : null);
			}
		}

		public Expression Previous
		{
			get
			{
				return ((this.stack.Count > 1) ? this.stack.ElementAt(1) : null);
			}
		}
		#endregion

		#region IEqualityComparer<Expression> Members
		public Boolean Equals(Expression x, Expression y)
		{
			if (Object.ReferenceEquals(x, y) == true)
			{
				return (true);
			}

			if ((x == null) || (y == null))
			{
				return (false);
			}

			if ((x.NodeType != y.NodeType) || (x.Type != y.Type))
			{
				return (false);
			}

			switch (x.NodeType)
			{
				case ExpressionType.ArrayLength:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.Negate:
				case ExpressionType.UnaryPlus:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
					return (this.Equals((UnaryExpression)x, (UnaryExpression)y));

				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.ArrayIndex:
				case ExpressionType.Coalesce:
				case ExpressionType.Divide:
				case ExpressionType.Equal:
				case ExpressionType.ExclusiveOr:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LeftShift:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.Modulo:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.NotEqual:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.Power:
				case ExpressionType.RightShift:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
					return (this.Equals((BinaryExpression)x, (BinaryExpression)y));

				case ExpressionType.Block:
					return (this.Equals((BlockExpression)x, (BlockExpression)y));

				case ExpressionType.Call:
					return (this.Equals((MethodCallExpression)x, (MethodCallExpression)y));

				case ExpressionType.Conditional:
					return (this.Equals((ConditionalExpression)x, (ConditionalExpression)y));

				case ExpressionType.Constant:
					return (this.Equals((ConstantExpression)x, (ConstantExpression)y));

				case ExpressionType.DebugInfo:
					return (this.Equals((DebugInfoExpression)x, (DebugInfoExpression)y));

				case ExpressionType.Default:
					return (this.Equals((DefaultExpression)x, (DefaultExpression)y));

				case ExpressionType.Dynamic:
					return (this.Equals((DynamicExpression)x, (DynamicExpression)y));

				case ExpressionType.Goto:
					return (this.Equals((GotoExpression)x, (GotoExpression)y));

				case ExpressionType.Index:
					return (this.Equals((IndexExpression)x, (IndexExpression)y));

				case ExpressionType.Invoke:
					return (this.Equals((InvocationExpression)x, (InvocationExpression)y));

				case ExpressionType.Label:
					return (this.Equals((LabelExpression)x, (LabelExpression)y));

				case ExpressionType.Lambda:
					return (this.Equals((LambdaExpression)x, (LambdaExpression)y));

				case ExpressionType.ListInit:
					return (this.Equals((ListInitExpression)x, (ListInitExpression)y));

				case ExpressionType.Loop:
					return (this.Equals((LoopExpression)x, (LoopExpression)y));

				case ExpressionType.MemberAccess:
					return (this.Equals((MemberExpression)x, (MemberExpression)y));

				case ExpressionType.MemberInit:
					return (this.Equals((MemberInitExpression)x, (MemberInitExpression)y));

				case ExpressionType.New:
					return (this.Equals((NewExpression)x, (NewExpression)y));

				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
					return (this.Equals((NewArrayExpression)x, (NewArrayExpression)y));

				case ExpressionType.Parameter:
					return (this.Equals((ParameterExpression)x, (ParameterExpression)y));

				case ExpressionType.RuntimeVariables:
					return (this.Equals((RuntimeVariablesExpression)x, (RuntimeVariablesExpression)y));

				case ExpressionType.Switch:
					return (this.Equals((SwitchExpression)x, (SwitchExpression)y));

				case ExpressionType.Try:
					return (this.Equals((TryExpression)x, (TryExpression)y));

				case ExpressionType.TypeIs:
					return (this.Equals((TypeBinaryExpression)x, (TypeBinaryExpression)y));

				default:
					throw (new ArgumentException("Unhandled expression type"));
			}
		}

		public Int32 GetHashCode(Expression expression)
		{
			this.hashCode = 0;

			this.Visit(expression);

			return (this.hashCode);
		}
		#endregion
	}
}