﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using NUnit.Framework;
        
namespace JsonCons.JsonPathLib
{

    interface IExpression
    {
         bool TryEvaluate(DynamicResources resources,
                          JsonElement root,
                          IJsonValue current, 
                          ResultOptions options,
                          out IJsonValue value);

         bool TryEvaluate(DynamicResources resources,
                          JsonElement root,
                          JsonElement current, 
                          ResultOptions options,
                          out IJsonValue value);
    }

    class Expression : IExpression
    {
        internal static bool IsFalse(IJsonValue val)
        {
            //TestContext.WriteLine($"IsFalse {val}");
            var comparer = JsonValueEqualityComparer.Instance;
            switch (val.ValueKind)
            {
                case JsonValueKind.False:
                    return true;
                case JsonValueKind.Null:
                    return true;
                case JsonValueKind.Array:
                    return val.GetArrayLength() == 0;
                case JsonValueKind.Object:
                    return val.EnumerateObject().MoveNext() == false;
                case JsonValueKind.String:
                    return val.GetString().Length == 0;
                case JsonValueKind.Number:
                {
                    Decimal dec;
                    if (val.TryGetDecimal(out dec))
                    {
                        return dec == 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                default:
                    return false;
            }
        }

        internal static bool IsTrue(IJsonValue val)
        {
            return !IsFalse(val);
        }

        IReadOnlyList<Token> _tokens;

        internal Expression(IReadOnlyList<Token> tokens)
        {
            _tokens = tokens;

            //TestContext.WriteLine("Expression constructor");
            //foreach (var token in _tokens)
            //{
            //    TestContext.WriteLine($"    {token}");
            //}
        }
        public  bool TryEvaluate(DynamicResources resources,
                                 JsonElement root,
                                 JsonElement current, 
                                 ResultOptions options,
                                 out IJsonValue result)
        {
            return this.TryEvaluate(resources, root, new JsonElementJsonValue(current), options, out result);
        }

        public  bool TryEvaluate(DynamicResources resources,
                                 JsonElement root,
                                 IJsonValue current, 
                                 ResultOptions options,
                                 out IJsonValue result)
        {
            Stack<IJsonValue> stack = new Stack<IJsonValue>();
            IList<IJsonValue> argStack = new List<IJsonValue>();

            /*
            TestContext.WriteLine("");
            TestContext.WriteLine("Expression tokens:");
            foreach (var item in _tokens)
            {
                TestContext.WriteLine($"    {item}");
            }
            TestContext.WriteLine("");
            */

            foreach (var token in _tokens)
            {
                switch (token.TokenKind)
                {
                    case JsonPathTokenKind.Value:
                    {
                        stack.Push(token.GetValue());
                        break;
                    }
                    case JsonPathTokenKind.UnaryOperator:
                    {
                        Debug.Assert(stack.Count >= 1);
                        var item = stack.Pop();
                        IJsonValue value;
                        if (!token.GetUnaryOperator().TryEvaluate(item, out value))
                        {
                            result = JsonConstants.Null;
                            return false;
                        }
                        stack.Push(value);
                        break;
                    }
                    case JsonPathTokenKind.BinaryOperator:
                    {
                        Debug.Assert(stack.Count >= 2);
                        var rhs = stack.Pop();
                        var lhs = stack.Pop();

                        IJsonValue value;
                        if (!token.GetBinaryOperator().TryEvaluate(lhs, rhs, out value))
                        {
                            result = JsonConstants.Null;
                            return false;
                        }
                        stack.Push(value);
                        break;
                    }
                    case JsonPathTokenKind.Selector:
                    {
                        IJsonValue value;
                        if (token.GetSelector().TryEvaluate(resources, root, new PathNode("@"), current.GetJsonElement(), options, out value))
                        {
                            stack.Push(value);
                        }
                        else
                        {
                            result = JsonConstants.Null;
                            return false;
                        }
                        break;
                    }
                    case JsonPathTokenKind.Argument:
                        Debug.Assert(stack.Count != 0);
                        argStack.Add(stack.Peek());
                        stack.Pop();
                        break;
                    case JsonPathTokenKind.Function:
                    {
                        if (token.GetFunction().Arity.HasValue && token.GetFunction().Arity.Value != argStack.Count)
                        {
                            result = JsonConstants.Null;
                            return false;
                        }

                        IJsonValue value;
                        if (!token.GetFunction().TryEvaluate(argStack, out value))
                        {
                            result = JsonConstants.Null;
                            return false;
                        }
                        argStack.Clear();
                        stack.Push(value);
                        break;
                    }
                    case JsonPathTokenKind.Expression:
                    {
                        IJsonValue value;
                        if (!token.GetExpression().TryEvaluate(resources, root, current, options, out value))
                        {
                            result = JsonConstants.Null;
                            return false;
                        }
                        else
                        {
                            stack.Push(value);
                        }
                        break;
                    }
                    default:
                        break;
                }
            }

            if (stack.Count == 0)
            {
                result = JsonConstants.Null;
                return false;
            }
            else
            {
                result = stack.Pop();
                return true;
            }
        }
    };

} // namespace JsonCons.JsonPathLib

