using System;
using System.Collections.Generic;
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

	public sealed class ExpressionInterceptor : ExpressionVisitor, IDisposable, IContextfulVisitor
	{
		private static readonly IEnumerable<Type> ExpressionTypes = typeof(Expression).Assembly.GetExportedTypes().Where(x => typeof(Expression).IsAssignableFrom(x)).OrderBy(x => x.Name).ToList();

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

		#region Private fields
		private readonly Stack<Expression> stack = new Stack<Expression>();
		#endregion

		#region Private methods
		private IDisposable Enqueue(Expression expression)
		{
			return (new Enqueuer(expression, this.stack));
		}
		#endregion

		#region Public events
		public event Func<BinaryExpression, BinaryExpression> Binary;
		public event Func<BlockExpression, BlockExpression> Block;
		public event Func<CatchBlock, CatchBlock> CatchBlock;
		public event Func<ConditionalExpression, ConditionalExpression> Conditional;
		public event Func<ConstantExpression, ConstantExpression> Constant;
		public event Func<DebugInfoExpression, DebugInfoExpression> DebugInfo;
		public event Func<DefaultExpression, DefaultExpression> Default;
		public event Func<DynamicExpression, DynamicExpression> Dynamic;
		public event Func<ElementInit, ElementInit> ElementInit;
		public event Func<Expression, Expression> Expression;
		public event Func<Expression, Expression> Extension;
		public event Func<GotoExpression, GotoExpression> Goto;
		public event Func<IndexExpression, IndexExpression> Index;
		public event Func<InvocationExpression, InvocationExpression> Invocation;
		public event Func<LabelExpression, LabelExpression> Label;
		public event Func<LabelTarget, LabelTarget> LabelTarget;
		public event Func<LambdaExpression, LambdaExpression> Lambda;
		public event Func<ListInitExpression, ListInitExpression> ListInit;
		public event Func<LoopExpression, LoopExpression> Loop;
		public event Func<MemberExpression, MemberExpression> Member;
		public event Func<MemberAssignment, MemberAssignment> MemberAssignment;
		public event Func<MethodCallExpression, MethodCallExpression> MethodCall;
		public event Func<MemberInitExpression, MemberInitExpression> MemberInit;
		public event Func<NewExpression, NewExpression> New;
		public event Func<NewArrayExpression, NewArrayExpression> NewArray;
		public event Func<ParameterExpression, ParameterExpression> Parameter;
		public event Func<RuntimeVariablesExpression, RuntimeVariablesExpression> RuntimeVariables;
		public event Func<SwitchExpression, SwitchExpression> Switch;
		public event Func<TryExpression, TryExpression> Try;
		public event Func<TypeBinaryExpression, TypeBinaryExpression> TypeBinary;
		public event Func<UnaryExpression, UnaryExpression> Unary;
		#endregion

		#region Public methods
		public IQueryable<T> Visit<T>(IQueryable<T> query)
		{
			return (this.Visit(query as IQueryable) as IQueryable<T>);
		}

		public IQueryable<T> Visit<T, TExpression>(IQueryable<T> query, Func<TExpression, TExpression> action) where TExpression : Expression
		{
			EventInfo evt = this.GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance).Where(x => x.EventHandlerType == typeof(Func<TExpression, TExpression>)).First();
			evt.AddEventHandler(this, action);

			query = this.Visit(query);

			evt.RemoveEventHandler(this, action);

			return (query);
		}

		public IQueryable Visit(IQueryable query)
		{
			return (query.Provider.CreateQuery(this.Visit(query.Expression)));
		}

		public IEnumerable<Expression> Flatten(IQueryable query)
		{
			ISet<Expression> list = new HashSet<Expression>();
			Func<Expression, Expression> action = delegate(Expression expression)
			{
				if (expression != null)
				{
					list.Add(expression);
				}

				return (expression);
			};

			this.Expression += action;

			this.Visit(query);

			this.Expression -= action;

			return (list);
		}
		#endregion

		#region Protected override methods
		protected override Expression VisitNew(NewExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.New != null) && (expression != null))
				{
					return (base.VisitNew(this.New(expression)));
				}
				else
				{
					return (base.VisitNew(expression));
				}
			}
		}

		protected override Expression VisitNewArray(NewArrayExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.NewArray != null) && (expression != null))
				{
					return (base.VisitNewArray(this.NewArray(expression)));
				}
				else
				{
					return (base.VisitNewArray(expression));
				}
			}
		}

		protected override Expression VisitParameter(ParameterExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Parameter != null) && (expression != null))
				{
					return (base.VisitParameter(this.Parameter(expression)));
				}
				else
				{
					return (base.VisitParameter(expression));
				}
			}
		}

		protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.RuntimeVariables != null) && (expression != null))
				{
					return (base.VisitRuntimeVariables(this.RuntimeVariables(expression)));
				}
				else
				{
					return (base.VisitRuntimeVariables(expression));
				}
			}
		}

		protected override Expression VisitSwitch(SwitchExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Switch != null) && (expression != null))
				{
					return (base.VisitSwitch(this.Switch(expression)));
				}
				else
				{
					return (base.VisitSwitch(expression));
				}
			}
		}

		protected override Expression VisitTry(TryExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Try != null) && (expression != null))
				{
					return (base.VisitTry(this.Try(expression)));
				}
				else
				{
					return (base.VisitTry(expression));
				}
			}
		}

		protected override Expression VisitTypeBinary(TypeBinaryExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.TypeBinary != null) && (expression != null))
				{
					return (base.VisitTypeBinary(this.TypeBinary(expression)));
				}
				else
				{
					return (base.VisitTypeBinary(expression));
				}
			}
		}

		protected override Expression VisitUnary(UnaryExpression expression)
		{
			using (this.Enqueue(expression))
			{
				using (this.Enqueue(expression))
				{
					if ((this.Unary != null) && (expression != null))
					{
						return (base.VisitUnary(this.Unary(expression)));
					}
					else
					{
						return (base.VisitUnary(expression));
					}
				}
			}
		}

		protected override Expression VisitMemberInit(MemberInitExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.MemberInit != null) && (expression != null))
				{
					return (base.VisitMemberInit(this.MemberInit(expression)));
				}
				else
				{
					return (base.VisitMemberInit(expression));
				}
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.MethodCall != null) && (expression != null))
				{
					return (base.VisitMethodCall(this.MethodCall(expression)));
				}
				else
				{
					return (base.VisitMethodCall(expression));
				}
			}
		}

		protected override Expression VisitLambda<T>(Expression<T> expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Lambda != null) && (expression != null))
				{
					return (base.VisitLambda<T>(this.Lambda(expression) as Expression<T>));
				}
				else
				{
					return (base.VisitLambda<T>(expression));
				}
			}
		}

		protected override Expression VisitBinary(BinaryExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Binary != null) && (expression != null))
				{
					return (base.VisitBinary(this.Binary(expression)));
				}
				else
				{
					return (base.VisitBinary(expression));
				}
			}
		}

		protected override Expression VisitBlock(BlockExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Block != null) && (expression != null))
				{
					return (base.VisitBlock(this.Block(expression)));
				}
				else
				{
					return (base.VisitBlock(expression));
				}
			}
		}

		protected override CatchBlock VisitCatchBlock(CatchBlock node)
		{
			if ((this.CatchBlock != null) && (node != null))
			{
				return (base.VisitCatchBlock(this.CatchBlock(node)));
			}
			else
			{
				return (base.VisitCatchBlock(node));
			}
		}

		protected override Expression VisitConditional(ConditionalExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Conditional != null) && (expression != null))
				{
					return (base.VisitConditional(this.Conditional(expression)));
				}
				else
				{
					return (base.VisitConditional(expression));
				}
			}
		}

		protected override Expression VisitConstant(ConstantExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Constant != null) && (expression != null))
				{
					return (base.VisitConstant(this.Constant(expression)));
				}
				else
				{
					return (base.VisitConstant(expression));
				}
			}
		}

		protected override Expression VisitDebugInfo(DebugInfoExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.DebugInfo != null) && (expression != null))
				{
					return (base.VisitDebugInfo(this.DebugInfo(expression)));
				}
				else
				{
					return (base.VisitDebugInfo(expression));
				}
			}
		}

		protected override Expression VisitDefault(DefaultExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Default != null) && (expression != null))
				{
					return (base.VisitDefault(this.Default(expression)));
				}
				else
				{
					return (base.VisitDefault(expression));
				}
			}
		}

		protected override Expression VisitDynamic(DynamicExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Dynamic != null) && (expression != null))
				{
					return (base.VisitDynamic(this.Dynamic(expression)));
				}
				else
				{
					return (base.VisitDynamic(expression));
				}
			}
		}

		protected override ElementInit VisitElementInit(ElementInit node)
		{
			if ((this.ElementInit != null) && (node != null))
			{
				return (base.VisitElementInit(this.ElementInit(node)));
			}
			else
			{
				return (base.VisitElementInit(node));
			}
		}

		protected override Expression VisitExtension(Expression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Extension != null) && (expression != null))
				{
					return (base.VisitExtension(this.Extension(expression)));
				}
				else
				{
					return (base.VisitExtension(expression));
				}
			}
		}

		protected override Expression VisitGoto(GotoExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Goto != null) && (expression != null))
				{
					return (base.VisitGoto(this.Goto(expression)));
				}
				else
				{
					return (base.VisitGoto(expression));
				}
			}
		}

		protected override Expression VisitIndex(IndexExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Index != null) && (expression != null))
				{
					return (base.VisitIndex(this.Index(expression)));
				}
				else
				{
					return (base.VisitIndex(expression));
				}
			}
		}

		protected override Expression VisitInvocation(InvocationExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Invocation != null) && (expression != null))
				{
					return (base.VisitInvocation(this.Invocation(expression)));
				}
				else
				{
					return (base.VisitInvocation(expression));
				}
			}
		}

		protected override Expression VisitLabel(LabelExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Label != null) && (expression != null))
				{
					return (base.VisitLabel(this.Label(expression)));
				}
				else
				{
					return (base.VisitLabel(expression));
				}
			}
		}

		protected override LabelTarget VisitLabelTarget(LabelTarget node)
		{
			if ((this.LabelTarget != null) && (node != null))
			{
				return (base.VisitLabelTarget(this.LabelTarget(node)));
			}
			else
			{
				return (base.VisitLabelTarget(node));
			}
		}

		protected override Expression VisitListInit(ListInitExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.ListInit != null) && (expression != null))
				{
					return (base.VisitListInit(this.ListInit(expression)));
				}
				else
				{
					return (base.VisitListInit(expression));
				}
			}
		}

		protected override Expression VisitLoop(LoopExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Loop != null) && (expression != null))
				{
					return (base.VisitLoop(this.Loop(expression)));
				}
				else
				{
					return (base.VisitLoop(expression));
				}
			}
		}

		protected override Expression VisitMember(MemberExpression expression)
		{
			using (this.Enqueue(expression))
			{
				if ((this.Member != null) && (expression != null))
				{
					return (base.VisitMember(this.Member(expression)));
				}
				else
				{
					return (base.VisitMember(expression));
				}
			}
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
		{
			if ((this.MemberAssignment != null) && (node != null))
			{
				return (base.VisitMemberAssignment(this.MemberAssignment(node)));
			}
			else
			{
				return (base.VisitMemberAssignment(node));
			}
		}
		#endregion

		#region IContextfulVisitor Members
		public override Expression Visit(Expression expression)
		{
			if ((this.Expression != null) && (expression != null))
			{
				return (base.Visit(this.Expression(base.Visit(expression))));
			}
			else
			{
				return (base.Visit(expression));
			}
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

		#region IDisposable Members

		public void Dispose()
		{
			this.Binary = null;
			this.Block = null;
			this.CatchBlock = null;
			this.Conditional = null;
			this.Constant = null;
			this.DebugInfo = null;
			this.Default = null;
			this.Dynamic = null;
			this.ElementInit = null;
			this.Expression = null;
			this.Extension = null;
			this.Goto = null;
			this.Index = null;
			this.Invocation = null;
			this.Label = null;
			this.LabelTarget = null;
			this.Lambda = null;
			this.ListInit = null;
			this.Loop = null;
			this.Member = null;
			this.MemberAssignment = null;
			this.MemberInit = null;
			this.MethodCall = null;
			this.New = null;
			this.NewArray = null;
			this.Parameter = null;
			this.RuntimeVariables = null;
			this.Switch = null;
			this.Try = null;
			this.TypeBinary = null;
			this.Unary = null;
		}

		#endregion
	}
}
