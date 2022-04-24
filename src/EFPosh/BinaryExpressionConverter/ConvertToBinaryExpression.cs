﻿using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace BinaryExpressionConverter
{
    /// <summary>
    /// Creates PowerShell cmdlet for converting a PowerShell BinaryExpressionAst object to a System.Linq BinaryExpression object
    /// </summary>
    [Cmdlet(VerbsData.ConvertTo, "BinaryExpression")]
    public class ConvertToBinaryExpression : PSCmdlet
    {
        [Parameter(Mandatory =true,
            Position =0, HelpMessage = "Base linq type we are querying against. If a List<string>, this will be string")]
        public Type FuncType { get; set; }
        [Parameter(
            Position = 1,
            Mandatory = true, HelpMessage = "Expression to convert. Expecting a BinaryExpressionAst.")]
        public ScriptBlock Expression { get; set; }
        [Parameter(
            Position = 2,
            Mandatory = false, HelpMessage = "Any arguments needed for the Expression. Arguments will be in the expression in the form of $0 $1 $2, where 0, 1, and 2 are the index for the Argument array")]
        public object[] Arguments { get; set; }

        private IPoshBinaryConverter _binaryConverter;
        private BinaryExpressionAst _binaryExpressionAst;
        protected override void BeginProcessing() 
        {
            var poshBinaryType = typeof(PoshBinaryConverter<>);
            var constructedPoshBinaryType = poshBinaryType.MakeGenericType(FuncType);
            _binaryConverter = (IPoshBinaryConverter)Activator.CreateInstance(constructedPoshBinaryType, new object[] { SessionState });

            var expressions = Expression.Ast.FindAll(p => p.GetType().Name.Equals("BinaryExpressionAst"), true);
            if (expressions.Count() == 0)
            {
                throw new Exception("Error parsing expression - No binary expressions found!");
            }
            _binaryExpressionAst = (BinaryExpressionAst)expressions.First();
        }
        protected override void ProcessRecord() { }
        protected override void EndProcessing() 
        {
            WriteObject(_binaryConverter.ConvertBinaryExpression(_binaryExpressionAst, Arguments));
        }
    }
}
