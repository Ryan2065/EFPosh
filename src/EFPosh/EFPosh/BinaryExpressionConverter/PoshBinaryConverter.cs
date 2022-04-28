using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace EFPosh
{
    /// <summary>
    /// Class to handle converting a PowerShell binary expression to Linq binary expression
    /// </summary>
    /// <typeparam name="T">Type of the collection we are searching</typeparam>
    public class PoshBinaryConverter<T>
    {
        private readonly ParameterExpression _p;
        private readonly SessionState _sState;
        private object[] arguments;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sState">sState is the PowerShell session from the Cmdlet. This allows us to run PowerShell code in the correct scope to find values if we need to</param>
        public PoshBinaryConverter(SessionState sState)
        {
            _p = Expression.Parameter(typeof(T), "p");
            _sState = sState;
        }
        /// <summary>
        /// Entry point to fulfill the Interface
        /// </summary>
        /// <param name="binaryExpression">Expression from user</param>
        /// <param name="Arguments">Parameters from user</param>
        /// <returns>Converted lambda expression - Listed as object because type isn't exactly known and posh gets it as an object anyway</returns>
        public Expression<Func<T, bool>> ConvertBinaryExpression(ScriptBlock sb, object[] Arguments)
        {
            var binaryExpression = sb.Ast.FindAll(p => p.GetType().Name.Equals("BinaryExpressionAst"), true).FirstOrDefault();
            arguments = Arguments;
            Expression finalExpression = null;
            if (binaryExpression != null)
            {
                finalExpression = BuildExpression((BinaryExpressionAst)binaryExpression);
            }
            else
            {
                var invokeExpression = sb.Ast.FindAll(p => p.GetType().Name.Equals("InvokeMemberExpressionAst"), true).FirstOrDefault();
                if(invokeExpression != null)
                {
                    finalExpression = BuildExpression((InvokeMemberExpressionAst)invokeExpression);
                }
            }
            return Expression.Lambda<Func<T, bool>>(finalExpression, _p);
        }
        /// <summary>
        /// Main control to get the Left and Right of the expression and then add in the operator.
        /// </summary>
        /// <param name="ast">PowerShell ast</param>
        /// <returns>Linq expression</returns>
        /// <exception cref="Exception">Will throw if something unexpected comes from PowerShell</exception>
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
                case TokenKind.Ceq:
                    return Expression.Equal(leftExpression, rightExpression);
                case TokenKind.Ine:
                case TokenKind.Cne:
                    return Expression.NotEqual(leftExpression, rightExpression);
                case TokenKind.And:
                    return Expression.And(leftExpression, rightExpression);
                case TokenKind.Or:
                    return Expression.Or(leftExpression, rightExpression);
                case TokenKind.Ccontains:
                case TokenKind.Icontains:
                    var method = leftExpression.Type.GetMethods().Where(p => p.Name == "Contains" && p.GetParameters().Length == 1).FirstOrDefault();
                    return Expression.Call(leftExpression, method, rightExpression);
                case TokenKind.Cnotcontains:
                case TokenKind.Inotcontains:
                    var notContainsmethod = leftExpression.Type.GetMethods().Where(p => p.Name == "Contains" && p.GetParameters().Length == 1).FirstOrDefault();
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
        private Expression BuildExpression(InvokeMemberExpressionAst ast)
        {
            return GetExpression(ast);
        }
        /// <summary>
        /// Converts a PowerShell expression to a Linq Expression
        /// </summary>
        /// <param name="expAst">PowerShell expression from BinaryExpression, usually going to be the left or right part of it</param>
        /// <param name="ensureType">Used if this was run once and an expression of the wrong base type was found</param>
        /// <returns>Linq expression</returns>
        /// <exception cref="Exception">Thrown if cannot make expression</exception>
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
                        returnValue = Expression.Constant(GetPoshValue(vexp, ensureType));
                    }
                    break;
                case InvokeMemberExpressionAst imexp:
                    List<Expression> arguments = new();
                    foreach(var arg in imexp.Arguments)
                    {
                        arguments.Add(GetExpression(arg));
                    }
                    var baseExp = GetExpression(imexp.Expression);
                    var imexValue = imexp.Member.SafeGetValue().ToString();
                    var methods = baseExp.Type.GetMethods().Where(p => p.Name == imexValue).ToList();
                    List<MethodInfo> acceptableMethods = new();
                    // trying to find an acceptable method - find methods that can accept the number of argumets we have
                    // Then looks to make sure the types are correct
                    foreach(var method in methods)
                    {
                        var methodParameters = method.GetParameters();
                        if(methodParameters.Length >= arguments.Count)
                        {
                            bool isAcceptable = true;
                            for (int i = 0; i < arguments.Count; i++)
                            {
                                if (methodParameters[i].ParameterType != arguments[i].Type)
                                {
                                    isAcceptable = false;
                                }
                            }
                            if (isAcceptable)
                            {
                                returnValue = Expression.Call(baseExp, method, arguments);
                                break;
                            }
                        }
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
                                if (propertyInfo == null && prop.Contains('$'))
                                {
                                    var value = GetPoshValue(ScriptBlock.Create(prop).Ast, ensureType);
                                    propertyInfo = ty.GetProperties().Where(p => p.Name.ToLower() == value.ToString().ToLower()).FirstOrDefault();
                                }
                                ty = propertyInfo.PropertyType;
                                returnValue = Expression.Property(returnValue, propertyInfo);
                            }
                        }
                    }
                    else
                    {
                        returnValue = Expression.Constant(GetPoshValue(mexp, ensureType));
                    }
                    break;
                case ParenExpressionAst pExp:
                    PipelineAst pipeAst = (PipelineAst)pExp.Pipeline;
                    if (pipeAst.PipelineElements[0] is CommandExpressionAst cExp)
                    {
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
                    returnValue = Expression.Constant(GetPoshValue(iexp, ensureType));
                    break;
                case ArrayExpressionAst aexp:
                    returnValue = Expression.Constant(GetPoshValue(aexp, ensureType));
                    break;
                default:
                    throw new Exception("Could not convert AST because it's an unknown type");
            }
            return returnValue;
        }
        /// <summary>
        /// Runs through a series of tries and tricks to try and get the value of something in the BinaryExpression, and get
        /// the value in the correct type
        /// </summary>
        /// <param name="script">PowerShell script to get the object - for instance, in the expression  { $_ -eq $Name }, this will be called and the script will be $Name</param>
        /// <param name="ensureType">EnsureType will be used if the wrong type was gotten once, and it needs to rerun to get the correct type</param>
        /// <returns>Hopefully an object of the correct type</returns>
        /// <exception cref="Exception">If the value cannot be found</exception>
        private object GetPoshValue(Ast expAst, Type ensureType = null)
        {
            try
            {
                var obj = expAst.SafeGetValue();
                return EnsureType(obj, ensureType);
            }
            catch { }
            var script = expAst.ToString().TrimStart('"').TrimEnd('"');
            var index = script.TrimStart('$').Trim();
            object value;
            if (int.TryParse(index, out int i))
            {
                value = arguments[i];
                if (value == null) { return value; }
                else if (ensureType != null)
                {
                    return EnsureType(value, ensureType);
                }
                if (value.GetType().IsArray)
                {
                    if (value.GetType().GetTypeInfo().GenericTypeArguments.Length > 0)
                    {
                        return value;
                    }
                    var arrayObject = value as Array;
                    foreach (var instance in arrayObject)
                    {
                        if (instance == null) { return instance; }
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
            PoshBinaryConverterObject returnObj = new();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(script);
            var base64 = System.Convert.ToBase64String(plainTextBytes);
            var values = _sState.InvokeCommand.InvokeScript($@"
                    $Expression = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{base64}'))
                    $returnObject = [EFPosh.PoshBinaryConverterObject]::new()
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
                if (value == null)
                {
                    return value;
                }
                else if (ensureType != null)
                {
                    return EnsureType(value, ensureType);
                }
                if (value.GetType().IsArray)
                {
                    if (value.GetType().GetTypeInfo().GenericTypeArguments.Length > 0)
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
                else if (value != null)
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

        /// <summary>
        /// PowerShell will automagically force an object to be the type a method requests. So this exists to use that feature.
        /// We have an expected type - so we create a generic PoshConverter with generic type ensureType. PoshConverter has one method - COnvertObject
        /// ConvertObject expects an object of type ensureType, and then will simply return it. When Posh sees it's sending an object and this expects a different
        /// type, it will do the conversion for us
        /// </summary>
        /// <param name="value">Value to be converted</param>
        /// <param name="ensureType">Type to be converted to</param>
        /// <returns></returns>
        private object EnsureType(object value, Type ensureType)
        {
            if(ensureType == null) { return value; }
            var poshConverterType = typeof(PoshConverter<>).MakeGenericType(new Type[] { ensureType });
            object poshConverter = Activator.CreateInstance(poshConverterType);
            var values = _sState.InvokeCommand.InvokeScript("param($value, $convertObj) return $convertObj.ConvertObject($value);", new[] { value, poshConverter });
            if(values.Count == 1)
            {
                return values[0].BaseObject;
            }
            return value;
        }
        /// <summary>
        /// Called with reflection. PowerShell likes to make everything an object[], even if all the same type. This tries to turn 
        /// an object[] into a strongly typed List<TY> so it can actually be used in Linq
        /// </summary>
        /// <typeparam name="TY">Type to convert to</typeparam>
        /// <param name="objects">All the objects in the list</param>
        /// <returns>Strongly typed list</returns>
        public List<TY> MakeList<TY>(Array objects)
        {
            var newList = new List<TY>();
            foreach (var instance in objects)
            {
                newList.Add((TY)instance);
            }
            return newList;
        }
    }
    /// <summary>
    /// Used to be able to get some data out of the script block we run this in
    /// </summary>
    public class PoshBinaryConverterObject
    {
        public object Value { get; set; }
        public bool IsArray { get; set; } = false;
    }
    /// <summary>
    /// Will be run inside of PowerShell. Forces PowerShell to use it's built in type conversion to convert a type of object to a strong type TItem
    /// </summary>
    /// <typeparam name="TItem">Type we need an object converted to</typeparam>
    public class PoshConverter<TItem>
    {
        public TItem ConvertObject(TItem obj)
        {
            return obj;
        }
    }
}
