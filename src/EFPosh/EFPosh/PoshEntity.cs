using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFPosh
{
    public class PoshEntity
    {
        public Type Type { get; set; }
        public string[] PrimaryKeys { get; set; }
        public bool Keyless { get; set; } = false;
        public string TableName { get; set; }
        public string Schema { get; set; }
        public string FromSql { get; set; }
    }
}
