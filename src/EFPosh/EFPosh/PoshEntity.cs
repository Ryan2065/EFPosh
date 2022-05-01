using System;

namespace EFPosh
{
    /// <summary>
    /// Created in PowerShell - gets from the user the special rules to set on the entity, like if it's keyless or has multiple primary keys
    /// </summary>
    /// 
    public class PoshEntity
    {
        public Type Type { get; set; }
        public string[] PrimaryKeys { get; set; }
        public bool Keyless { get; set; } = false;
        public string TableName { get; set; }
        public string Schema { get; set; }
        public string FromSql { get; set; }
        public string GetUniqueString()
        {
            var returnString = new System.Text.StringBuilder();
            returnString.Append($"{Type.AssemblyQualifiedName}{Type.Name}{Type.Namespace}{Keyless}{TableName}{Schema}{FromSql}");
            if(null != PrimaryKeys)
            {
                foreach (var k in PrimaryKeys)
                {
                    returnString.Append(k);
                }
            }
            foreach (var attrib in Type.GetCustomAttributes(false))
            {
                returnString.Append(attrib.GetType().Name);
            }
            return returnString.ToString();
        }
    }
}
