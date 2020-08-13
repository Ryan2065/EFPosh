using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace BinaryExpressionConverter
{
    [Cmdlet(VerbsData.ConvertTo, "BinaryExpression")]
    public class ConvertToBinaryExpression : PSCmdlet
    {
        [Parameter(Mandatory =true,
            Position =0)]
        public Type FuncType { get; set; }
        [Parameter(
            Position = 1,
            Mandatory = true)]
        public ScriptBlock Expression { get; set; }
        [Parameter(
            Position = 2,
            Mandatory = false)]
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
