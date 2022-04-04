using System.Management.Automation.Language;

namespace BinaryExpressionConverter
{
    public interface IPoshBinaryConverter
    {
        object ConvertBinaryExpression(BinaryExpressionAst binaryExpression, object[] Arguments);
    }
}
