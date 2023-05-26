using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

public static class ExpressionUtilities
{
    public static object GetValueWithoutCompiling(Expression expression)
    {
        return GetValue(expression, false);
    }

    public static object GetValueWithCompiling(Expression expression)
    {
        return GetValue(expression, true);
    }

    private static object GetValue(Expression expression, bool allowCompile)
    {
        if (expression == null)return null;

        switch (expression)
        {
            case ConstantExpression constantExpression:
                return GetValue(constantExpression);
            case MemberExpression memberExpression:
                return GetValue(memberExpression, allowCompile);
            case MethodCallExpression methodCallExpression:
                return GetValue(methodCallExpression, allowCompile);
        }

        if (allowCompile)
        {
            return GetValueUsingCompile(expression);
        }

        throw new Exception("Couldn't evaluate Expression without compiling: " + expression);
    }

    private static object GetValue(ConstantExpression constantExpression)
    {
        return constantExpression.Value;
    }

    private static object GetValue(MemberExpression memberExpression, bool allowCompile)
    {
        object value = GetValue(memberExpression.Expression, allowCompile);

        MemberInfo member = memberExpression.Member;
        if (member is FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue(value);
        }

        if (member is PropertyInfo propertyInfo)
        {
            try
            {
                return propertyInfo.GetValue(value);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        throw new Exception("Unknown member type: " + member.GetType());
    }

    private static object GetValue(MethodCallExpression methodCallExpression, bool allowCompile)
    {
        object[] paras = GetArray(methodCallExpression.Arguments, true);
        object obj = GetValue(methodCallExpression.Object, allowCompile);

        try
        {
            return methodCallExpression.Method.Invoke(obj, paras);
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException;
        }
    }

    private static object[] GetArray(IEnumerable<Expression> expressions, bool allowCompile)
    {
        List<object> list = new List<object>();
        foreach (Expression expression in expressions)
        {
            object value = GetValue(expression, allowCompile);
            list.Add(value);
        }

        return list.ToArray();
    }

    private static object GetValueUsingCompile(Expression expression)
    {
        LambdaExpression lambdaExpression = Expression.Lambda(expression);
        Delegate expressionDelegate = lambdaExpression.Compile();
        return expressionDelegate.DynamicInvoke();
    }

    /*
    namespace Tests
    {
        using NUnit.Framework;
        using System;
        using System.Text;
        using System.Linq.Expressions;

        public class ExpressionUtilitiesTests
        {
            [Test]
            public void GetValue_ConstantExpression_ReturnValue()
            {
                var expression = ExpressionBody(() => "__String__");
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            [Test]
            public void GetValue_Null_ReturnNull()
            {
                var expression = ExpressionBody(() => (string)null);
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            [Test]
            public void GetValue_MemberExpression_ReturnProperty()
            {
                var expression = ExpressionBody(() => Encoding.ASCII);
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            [Test]
            public void GetValue_PropertyExpression_ReturnValue()
            {
                var expression = ExpressionBody(() => InstanceProp);
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            string InstanceProp { get; } = "__InstanceProp__";

            [Test]
            public void GetValue_StaticProperty_ReturnValue()
            {
                var expression = ExpressionBody(() => StaticProperty);
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            static string StaticProperty { get; } = "__StaticProperty__";

            [Test]
            public void GetValue_MethodCallWithArguments_ReturnValue()
            {
                var expression = ExpressionBody(() => string.Format("{0} {1} {2}", "foo", "bar", 1));
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            [Test]
            public void GetValue_MethodExpression_ReturnValue()
            {
                var expression = ExpressionBody(() => 666.ToString());
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            [Test]
            public void GetValue_RecursiveMethodCalls_ReturnValue()
            {
                var expression = ExpressionBody(() => 666.ToString().ToString());
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            [Test]
            public void GetValue_Local_ReturnLocal()
            {
                var local = "__Local__";
                var expression = ExpressionBody(() => local);
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValueWithoutCompiling(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
            }

            [Test]
            public void GetValue_Add_GetValueWithoutCompilingThrowsException()
            {
                var x = 42;
                var expression = ExpressionBody(() => x + 666);
                var expectedValue = ExpressionUtilities.GetValueUsingCompile(expression);

                var value = ExpressionUtilities.GetValue(expression);

                Assert.That(value, Is.EqualTo(expectedValue));
                Assert.Throws<Exception>(
                    () => ExpressionUtilities.GetValueWithoutCompiling(expression));
            }

            [Test]
            public void GetValue_MethodThrowsException_ThrowsException()
            {
                var expression = ExpressionBody(() => methodThrowsException());

                Assert.Throws<Exception>(
                    () => ExpressionUtilities.GetValue(expression));
            }

            static int methodThrowsException()
            {
                throw new Exception("Boom!");
            }

            [Test]
            public void GetValue_PropertyThrowsException_ThrowsException()
            {
                var expression = ExpressionBody(() => PropertyThrowsException);

                Assert.Throws<Exception>(
                    () => ExpressionUtilities.GetValue(expression));
            }

            static int PropertyThrowsException
            {
                get
                {
                    throw new Exception("Boom!");
                }
            }

            static Expression ExpressionBody<T>(Expression<Func<T>> expression)
            {
                return expression.Body;
            }
        }

    }
        */
}
