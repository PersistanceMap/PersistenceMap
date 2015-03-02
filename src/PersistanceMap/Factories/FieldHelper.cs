﻿using PersistanceMap.Tracing;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace PersistanceMap.Factories
{
    public static class FieldHelper
    {
        public static string TryExtractPropertyName(LambdaExpression propertyExpression)
        {
            propertyExpression.EnsureArgumentNotNull("propertyExpression");

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                // try get the member from the operand of the unaryexpression
                var unary = propertyExpression.Body as UnaryExpression;
                if (unary != null)
                    memberExpression = unary.Operand as MemberExpression;

                if (memberExpression == null)
                {
                    var binary = propertyExpression.Body as BinaryExpression;
                    if (binary != null)
                        memberExpression = binary.Left as MemberExpression;
                }

                if (memberExpression == null)
                {
                    Logger.TraceLine("PersistanceMap - Property is not a MemberAccessExpression: {0}", propertyExpression.ToString());

                    try
                    {
                        return propertyExpression.Compile().DynamicInvoke().ToString();
                    }
                    catch (Exception e)
                    {
                        Logger.TraceLine(e.Message);
                        return propertyExpression.ToString();
                    }
                }
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                Logger.TraceLine("Property is not a PropertyInfo");
                return memberExpression.Member.ToString();
            }

            if (propertyInfo.GetGetMethod(true).IsStatic)
            {
                Logger.TraceLine("Property is static");
                return memberExpression.Member.ToString();
            }

            return memberExpression.Member.Name;
        }
    }
}
