using System.Collections.Generic;
using System.Linq.Expressions;

namespace DevelopmentWithADot.LinqExpressionInterceptor
{
	public interface IContextfulVisitor
	{
		Expression Visit(Expression expression);

		Stack<Expression> Stack
		{
			get;
		}

		Expression Current
		{
			get;
		}

		Expression Previous
		{
			get;
		}
	}
}
