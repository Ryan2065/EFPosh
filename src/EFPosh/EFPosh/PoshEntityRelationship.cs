using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFPosh
{
    public class PoshEntityRelationship
    {
        public string SourceTypeName { get; set; }
        public string TargetTypeName { get; set; }
        public PoshEntityRelationshipType RelationshipType { get; set; }
        public string SourceKey { get; set; }
        public string TargetKey { get; set; }
        public string SourceRelationshipProperty { get; set; }
        public string TargetRelationshipProperty { get; set; }
    }
    public enum PoshEntityRelationshipType
    {
        OneToOne,
        OneToMany,
        ManyToOne,
        ManyToMany
    }
}
