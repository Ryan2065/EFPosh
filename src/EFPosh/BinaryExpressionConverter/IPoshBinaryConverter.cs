using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation.Language;
using System.Text;

namespace BinaryExpressionConverter
{
    public interface IPoshBinaryConverter
    {
        object ConvertBinaryExpression(BinaryExpressionAst binaryExpression, object[] Arguments);
    }
}
