using System.Management.Automation.Language;

namespace BinaryExpressionConverter
{
    /// <summary>
    /// Interface required to get around some reflection magic with generics.
    /// </summary>
    public interface IPoshBinaryConverter
    {
        /// <summary>
        /// Main method to do the actual conversion of the binary expression
        /// </summary>
        /// <param name="binaryExpression">PowerShell binary expression</param>
        /// <param name="Arguments">Any arguments given to the cmdlet for the expression</param>
        /// <returns></returns>
        object ConvertBinaryExpression(BinaryExpressionAst binaryExpression, object[] Arguments);
    }
}
