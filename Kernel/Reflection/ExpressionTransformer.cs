using System;
using System.Linq.Expressions;

namespace PSOK.Kernel.Reflection
{
    /// <summary>
    /// Changes the parameter type of an expression
    /// </summary>
    public class ExpressionTransformer
    {
        private readonly Type _targetType;

        /// <summary>
        /// Constructs a new <see cref="ExpressionTransformer"/>.
        /// </summary>
        /// <param name="targetType"></param>
        public ExpressionTransformer(Type targetType)
        {
            _targetType = targetType;
        }

        /// <summary>
        /// Changes the parameter type of an expression
        /// </summary>
        /// <typeparam name="TFunc">The function type of the expression</typeparam>
        /// <param name="expression">The expression whose parameter type should be changed</param>
        /// <returns></returns>
        public Expression Tranform<TFunc>(Expression<TFunc> expression)
        {
            ParameterExpression parameter = Expression.Parameter(_targetType);
            Expression body = new Visitor(parameter).Visit(expression.Body);
            return Expression.Lambda(body, parameter);
        }

        /// <summary>
        /// An expression visitor that changes the parameter of an expression
        /// </summary>
        private class Visitor : ExpressionVisitor
        {
            private readonly ParameterExpression _parameterExpression;

            public Visitor(ParameterExpression parameterExpression)
            {
                _parameterExpression = parameterExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parameterExpression;
            }
        }
    }
}