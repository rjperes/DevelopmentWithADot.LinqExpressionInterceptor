using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DevelopmentWithADot.LinqExpressionInterceptor.Tests
{
	class Program
	{
		static void Main(string[] args)
		{
			var range = Enumerable.Range(0, 10).AsQueryable().Select(x => new { A = x, B = x % 2 == 0, C = x.ToString() }).Where(x => !x.B).Select(x => x.A).OrderBy(x => x.ToString()).Take(1);
			var interceptor = new ExpressionInterceptor();
			interceptor.Expression += (source) =>
			{
				var @ran = range;
				var @int = interceptor;
				return (source);
			};

			interceptor.Visit(range.Expression);
			var children = range.Expression.GetChildren();


			var lettersArray = new String[] { "A", "B", "C" };	//a data source
			var lettersQuery = lettersArray.AsQueryable().Where(x => x == "A").OrderByDescending(x => x).Select(x => x.ToUpper());	//a silly query
			var lettersInterceptedQuery = interceptor.Visit<String, MethodCallExpression>(lettersQuery, x =>
			{
				if (x.Method.Name == "ToUpper")
				{
					//change from uppercase to lowercase
					x = Expression.Call(x.Object, typeof(String).GetMethods().First(y => y.Name == "ToLower"));
				}

				return (x);
			});
			lettersInterceptedQuery = interceptor.Visit<String, BinaryExpression>(lettersInterceptedQuery, x =>
			{
				//change from qual to not equal
				x = Expression.MakeBinary(ExpressionType.NotEqual, x.Left, x.Right);

				return (x);
			});
			var lettersExpressions = interceptor.Flatten(lettersQuery);	//all expressions found
			var lettersList = lettersQuery.ToList();	//"A"
			var lettersInterceptedList = lettersInterceptedQuery.ToList();	//"c", "b"
		}
	}
}
