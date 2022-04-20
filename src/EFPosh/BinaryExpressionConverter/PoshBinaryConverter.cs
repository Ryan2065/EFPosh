using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace BinaryExpressionConverter
{
    public class PoshBinaryConverter<T> : IPoshBinaryConverter
    {
        private ParameterExpression _p;
        private SessionState _sState;
        private object[] arguments;
        public PoshBinaryConverter(SessionState sState)
        {
            _p = Expression.Parameter(typeof(T), "p");
            _sState = sState;
        }
        private Expression BuildExpression(BinaryExpressionAst ast)
        {
            var leftExpression = GetExpression(ast.Left);
            var rightExpression = GetExpression(ast.Right);

            if(leftExpression.Type != rightExpression.Type && (leftExpression.ToString() != "null" && rightExpression.ToString() != "null"))
            {
                if (ast.Left.ToString().Contains("$_"))
                {
                    rightExpression = GetExpression(ast.Right, leftExpression.Type);
                }
                else if(ast.Right.ToString().Contains("$_"))
                {
                    leftExpression = GetExpression(ast.Left, rightExpression.Type);
                }
            }

            switch (ast.Operator)
            {
                case TokenKind.Ieq:
                    if (leftExpression.Type == typeof(string) && rightExpression.Type == typeof(string) && ast.Extent.ToString().ToLower().Contains("-ieq"))
                    {
                        var mi = typeof(String).GetMethods().Where(p => p.GetParameters().Count() == 3 && p.Name == "Equals").FirstOrDefault();
                        var ordinalCase = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                        return Expression.Call(mi, new Expression[] { leftExpression, rightExpression, ordinalCase });
                    }
                    return Expression.Equal(leftExpression, rightExpression);
                case TokenKind.Ceq:
                    var smi = typeof(String).GetMethods().Where(p => p.GetParameters().Count() == 3 && p.Name == "Equals").FirstOrDefault();
                    var sordinalCase = Expression.Constant(StringComparison.Ordinal);
                    return Expression.Call(smi, new Expression[] { leftExpression, rightExpression, sordinalCase });
                case TokenKind.Ine:
                case TokenKind.Cne:
                    return Expression.NotEqual(leftExpression, rightExpression);
                case TokenKind.And:
                    return Expression.And(leftExpression, rightExpression);
                case TokenKind.Or:
                    return Expression.Or(leftExpression, rightExpression);
                case TokenKind.Ccontains:
                case TokenKind.Icontains:
                    var method = leftExpression.Type.GetMethods().Where(p => p.Name == "Contains" && p.GetParameters().Count() == 1).FirstOrDefault();
                    return Expression.Call(leftExpression, method, rightExpression);
                case TokenKind.Cnotcontains:
                case TokenKind.Inotcontains:
                    var notContainsmethod = leftExpression.Type.GetMethods().Where(p => p.Name == "Contains" && p.GetParameters().Count() == 1).FirstOrDefault();
                    return Expression.Not(Expression.Call(leftExpression, notContainsmethod, rightExpression));
                case TokenKind.Ige:
                case TokenKind.Cge:
                    return Expression.GreaterThanOrEqual(leftExpression, rightExpression);
                case TokenKind.Igt:
                case TokenKind.Cgt:
                    return Expression.GreaterThan(leftExpression, rightExpression);
                case TokenKind.Ilt:
                case TokenKind.Clt:
                    return Expression.LessThan(leftExpression, rightExpression);
                case TokenKind.Ile:
                case TokenKind.Cle:
                    return Expression.LessThanOrEqual(leftExpression, rightExpression);
                case TokenKind.Ilike:
                case TokenKind.Clike:
                    return Expression.Call(typeof(DbFunctionsExtensions), "Like", null, Expression.Constant(EF.Functions), leftExpression, rightExpression);
                default:
                    throw new Exception("Could not build a Linq query from supplied expression");
            }
        }
        public List<TY> MakeList<TY>(Array objects)
        {
            var newList = new List<TY>();
            foreach(var instance in objects)
            {
                newList.Add((TY)instance);
            }
            return newList;
        }
        
        private object EnsureType(object value, Type ensureType)
        {
            var poshConverterType = typeof(PoshConverter<>).MakeGenericType(new Type[] { ensureType });
            object poshConverter = Activator.CreateInstance(poshConverterType);
            var values = _sState.InvokeCommand.InvokeScript("param($value, $convertObj) return $convertObj.ConvertObject($value);", new[] { value, poshConverter });
            if(values.Count == 1)
            {
                return values[0].BaseObject;
            }
            return value;
        }
        private object GetPoshValue(string script, Type ensureType = null)
        {
            var index = script.TrimStart('$').Trim();
            object value;
            if(int.TryParse(index, out int i))
            {
                value = arguments[i];
                if(value == null) { return value; }
                else if(ensureType != null)
                {
                    return EnsureType(value, ensureType);
                }
                if (value.GetType().IsArray)
                {
                    if (value.GetType().GetTypeInfo().GenericTypeArguments.Count() > 0)
                    {
                        return value;
                    }
                    var arrayObject = value as Array;
                    foreach (var instance in arrayObject)
                    {
                        if(instance == null) { return instance; }
                        var baseType = typeof(PoshBinaryConverter<T>);
                        var methDef = baseType.GetMethods().Where(p => p.Name == "MakeList").FirstOrDefault();
                        var ty = instance.GetType();
                        if (ty.Name == "Object") { ty = ty.BaseType; }
                        return methDef.MakeGenericMethod(ty)
                            .Invoke(this, new object[] { value });
                    }
                }
                return value;
            }
            PoshBinaryConverterObject returnObj = new PoshBinaryConverterObject();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(script);
            var base64 = System.Convert.ToBase64String(plainTextBytes);
            var values = _sState.InvokeCommand.InvokeScript($@"
                    $Expression = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{base64}'))
                    $returnObject = [BinaryExpressionConverter.PoshBinaryConverterObject]::new()
                    $returnObject.Value = . ([scriptblock]::Create($Expression))
                    return $returnObject
                ");
            if (values.Count != 1)
            {
                throw new Exception($"Could not get value from {script}");
            }
            returnObj = (PoshBinaryConverterObject)(values[0].BaseObject);
            if (script.StartsWith("@("))
            {
                value = returnObj.Value;
                if(value == null)
                {
                    return value;
                }
                else if (ensureType != null)
                {
                    return EnsureType(value, ensureType);
                }
                if (value.GetType().IsArray)
                {
                    if (value.GetType().GetTypeInfo().GenericTypeArguments.Count() > 0)
                    {
                        return value;
                    }
                    var arrayObject = value as Array;
                    foreach (var instance in arrayObject)
                    {
                        var baseType = typeof(PoshBinaryConverter<T>);
                        var methDef = baseType.GetMethods().Where(p => p.Name == "MakeList").FirstOrDefault();
                        var ty = instance.GetType();
                        if (ty.Name == "Object") { ty = ty.BaseType; }
                        return methDef.MakeGenericMethod(ty)
                            .Invoke(this, new object[] { value });
                    }
                }
                else if(value != null)
                {
                    var baseType = typeof(PoshBinaryConverter<T>);
                    var methDef = baseType.GetMethods().Where(p => p.Name == "MakeList").FirstOrDefault();
                    var ty = value.GetType();
                    if (ty.Name == "Object") { ty = ty.BaseType; }
                    return methDef.MakeGenericMethod(ty)
                        .Invoke(this, new object[] { new[] { value } });
                }
            }

            throw new Exception($"Could not expand {script} - Consider using -ArgumentList to pass arguments!");
        }
        private Expression GetExpression(ExpressionAst expAst, Type ensureType = null)
        {
            Expression returnValue = null;
            switch (expAst)
            {
                case VariableExpressionAst vexp:
                    if (vexp.VariablePath.ToString().ToLower() == "$_")
                    {
                        returnValue = _p;
                    }
                    else
                    {
                        returnValue = Expression.Constant(GetPoshValue(vexp.ToString(), ensureType));
                    }
                    break;
                case MemberExpressionAst mexp:
                    if (mexp.Expression.ToString().ToLower() == "$_" || mexp.Expression.ToString().ToLower().StartsWith("$_"))
                    {
                        var stringArray = mexp.Extent.ToString().Split('.');
                        Type ty = typeof(T);
                        returnValue = _p;
                        foreach (var prop in stringArray)
                        {
                            if (prop.ToLower() != "$_")
                            {
                                var propertyInfo = ty.GetProperties().Where(p => p.Name.ToLower() == prop.ToLower()).FirstOrDefault();
                                // This case is for $_."$Name" to expand $Name and get the property name
                                if (propertyInfo == null && prop.Contains("$"))
                                {
                                    var value = GetPoshValue(prop.TrimStart('"').TrimEnd('"'), ensureType);
                                    propertyInfo = ty.GetProperties().Where(p => p.Name.ToLower() == value.ToString().ToLower()).FirstOrDefault();
                                }
                                ty = propertyInfo.PropertyType;
                                returnValue = Expression.Property(returnValue, propertyInfo);
                            }
                        }
                    }
                    else
                    {
                        returnValue = Expression.Constant(GetPoshValue(mexp.ToString(), ensureType));
                    }
                    break;
                case ParenExpressionAst pExp:
                    PipelineAst pipeAst = (PipelineAst)pExp.Pipeline;
                    if (pipeAst.PipelineElements[0] is CommandExpressionAst)
                    {
                        var cExp = (CommandExpressionAst)pipeAst.PipelineElements[0];
                        returnValue = GetExpression(cExp.Expression);
                    }
                    break;
                case BinaryExpressionAst bexp:
                    returnValue = BuildExpression(bexp);
                    break;
                case ConstantExpressionAst cexp:
                    returnValue = Expression.Constant(cexp.Value);
                    break;
                case IndexExpressionAst iexp:
                    returnValue = Expression.Constant(GetPoshValue(iexp.ToString(), ensureType));
                    break;
                case ArrayExpressionAst aexp:
                    returnValue = Expression.Constant(GetPoshValue(aexp.ToString(), ensureType));
                    break;
                default:
                    throw new Exception("Could not convert AST because it's an unknown type");
            }
            return returnValue;
        }
        public object ConvertBinaryExpression(BinaryExpressionAst binaryExpression, object[] Arguments)
        {
            arguments = Arguments;
            var bExp = BuildExpression(binaryExpression);
            return Expression.Lambda<Func<T, bool>>(bExp, _p);
        }
    }
    public class PoshBinaryConverterObject
    {
        public object Value { get; set; }
        public bool IsArray { get; set; } = false;
    }
    public class PoshConverter<TItem>
    {
        public TItem ConvertObject(TItem obj)
        {
            return obj;
        }
    }
}
