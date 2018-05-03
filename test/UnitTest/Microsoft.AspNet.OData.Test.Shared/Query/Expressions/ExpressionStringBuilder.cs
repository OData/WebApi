﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNet.OData.Query.Expressions;

namespace Microsoft.AspNet.OData.Test.Query.Expressions
{
    public class ExpressionStringBuilder : ExpressionVisitor
    {
        private StringBuilder _builder = new StringBuilder();

        private ExpressionStringBuilder()
        {
        }

        public static string ToString(Expression expression)
        {
            ExpressionStringBuilder visitor = new ExpressionStringBuilder();
            visitor.Visit(expression);
            return visitor._builder.ToString();
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Out(String.Join(",", node.Parameters.Select(n => n.Name)));
            Out(" => ");
            Visit(node.Body);
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Out("(");
            Visit(node.Left);
            Out(" ");
            Out(ToString(node.NodeType));
            Out(" ");
            Visit(node.Right);
            Out(")");
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Out(node.Name);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // If it is a static member expression the Expression is null.
            if (node.Expression == null && node.NodeType == ExpressionType.MemberAccess)
            {
                Visit(node.Expression);
                Out(node.Member.DeclaringType.Name + "." + node.Member.Name);
            }
            else if (node.Expression.NodeType == ExpressionType.Constant)
            {
                Visit(node.Expression);
            }
            else
            {
                Visit(node.Expression);
                Out("." + node.Member.Name);
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                Out("null");
            }
            else
            {
                LinqParameterContainer container = node.Value as LinqParameterContainer;
                string stringValue;
                if (container != null)
                {
                    stringValue = container.Property as string;
                    if (stringValue != null)
                    {
                        Out("\"" + stringValue + "\"");
                    }
                    else
                    {
                        stringValue = String.Format(CultureInfo.InvariantCulture, "{0}", container.Property);
                        Out(stringValue);
                    }
                }
                else
                {
                    stringValue = String.Format(CultureInfo.InvariantCulture, "{0}", node.Value);
                    Out(stringValue);
                }
            }

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert)
            {
                Out("Convert(");
                Visit(node.Operand);
                Out(")");
                return node;
            }
            else if (node.NodeType == ExpressionType.Not)
            {
                Out("Not(");
                Visit(node.Operand);
                Out(")");
                return node;
            }
            else if (node.NodeType == ExpressionType.TypeAs)
            {
                Out("(");
                Visit(node.Operand);
                Out(" As " + node.Type.Name + ")");
                return node;
            }

            return base.VisitUnary(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            Out("new " + node.Type.Name + "(");
            VisitArguments(node.Arguments.ToArray());
            Out(")");
            return node;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return base.VisitMemberListBinding(node);
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            VisitNew(node.NewExpression);
            Out(" {");
            foreach (MemberAssignment memberNode in node.Bindings)
            {
                Out(memberNode.Member.Name + " = ");

                Visit(memberNode.Expression);
                Out(", ");
            }
            Out("}");

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            int argindex = 0;
            Visit(node.Object);

            IEnumerable<Expression> arguments = node.Arguments;
            if (node.Method.IsStatic)
            {
                Visit(arguments.First());
                arguments = arguments.Skip(1);
                argindex++;
            }

            Out("." + node.Method.Name + "(");
            VisitArguments(arguments.ToArray());
            Out(")");
            return node;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            if (node.NodeType == ExpressionType.TypeIs)
            {
                Out("(");
                Visit(node.Expression);
                Out(" Is ");
                Out(node.TypeOperand.FullName + ")");
            }

            return node;
        }

        private void VisitArguments(Expression[] arguments)
        {
            int argindex = 0;
            while (argindex < arguments.Length)
            {
                Visit(arguments[argindex]);
                argindex++;

                if (argindex < arguments.Length)
                {
                    Out(", ");
                }
            }
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Out("IIF(");
            Visit(node.Test);
            Out(", ");
            Visit(node.IfTrue);
            Out(", ");
            Visit(node.IfFalse);
            Out(")");
            return node;
        }

        private static string ToString(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AndAlso";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Not:
                    return "!";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OrElse";
                case ExpressionType.Subtract:
                    return "-";
                default:
                    throw new NotImplementedException();
            }
        }

        private void Out(string s)
        {
            _builder.Append(s);
        }
    }
}
