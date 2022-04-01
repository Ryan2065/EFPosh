using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFPosh.Models
{
    public class EfEntity
    {
        public Type EntityType { get; set; }
        public bool Keyless { get; set; } = false;
        public string TableName { get; set; }
        public string Schema { get; set; } = "dbo";
        public string FromSql { get; set; }
    }
}
