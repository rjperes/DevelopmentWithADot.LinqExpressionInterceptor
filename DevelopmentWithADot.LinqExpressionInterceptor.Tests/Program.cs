using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DevelopmentWithADot.LinqExpressionInterceptor.Te
{
	class Program
	{
		static void Main(string[] args)
		{
			var expressionTypes = typeof(Expression).Assembly.GetExportedTypes().Where(x => typeof(Expression).IsAssignableFrom(x)).OrderBy(x => x.Name).Select(x => x.Name.Split('.').First()).ToList();

			var range = Enumerable.Range(0, 10).Select(x => new { A = x, B = x % 2 == 0, C = x.ToString() }).AsQueryable().Where(x => !x.B).Select(x => x.A).OrderBy(x => x.ToString()).Take(1);
			ExpressionInterceptor interceptor = new ExpressionInterceptor();
			interceptor.Visit(range.Expression);
			IEnumerable<Expression> children = range.Expression.GetChildren();


			String[] lettersArray = new String[] { "A", "B", "C" };	//a data source
			IQueryable<String> lettersQuery = lettersArray.AsQueryable().Where(x => x == "A").OrderByDescending(x => x).Select(x => x.ToUpper());	//a silly query
			IQueryable<String> lettersInterceptedQuery = interceptor.Visit<String, MethodCallExpression>(lettersQuery, x =>
			{
				if (x.Method.Name == "ToUpper")
				{
					//change from uppercase to lowercase
					x = Expression.Call(x.Object, typeof(String).GetMethods().Where(y => y.Name == "ToLower").First());
				}

				return (x);
			});
			lettersInterceptedQuery = interceptor.Visit<String, BinaryExpression>(lettersInterceptedQuery, x =>
			{
				//change from qual to not equal
				x = Expression.MakeBinary(ExpressionType.NotEqual, x.Left, x.Right);

				return (x);
			});
			IEnumerable<Expression> lettersExpressions = interceptor.Flatten(lettersQuery);	//all expressions found
			IEnumerable<String> lettersList = lettersQuery.ToList();	//"A"
			IEnumerable<String> lettersInterceptedList = lettersInterceptedQuery.ToList();	//"c", "b"
		}
	}
}
