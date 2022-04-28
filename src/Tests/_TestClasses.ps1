using namespace System.ComponentModel.DataAnnotations.Schema;
using namespace System.ComponentModel.DataAnnotations;
using namespace System.Collections.Generic;

Class TestTableOne {
    [Key()]
    [DatabaseGenerated([DatabaseGeneratedOption]::Identity)]
    [int] $Id
    [string]$Name

    [ForeignKey("TableOneId")]
    [TestTableTwo]$RelationshipOne
}

Class TestTableTwo {
    [Key()]
    [DatabaseGenerated([DatabaseGeneratedOption]::Identity)]
    [int]$Id

    [ForeignKey("RelationshipTwo")]
    [int]$TableOneId
    [TestTableOne]$RelationshipTwo

    [string]$Name

    [ForeignKey("TableThree")]
    [int]$TableThreeId
    [TestTableThree]$TableThree
}

Class TestTableThree {
    [Key()]
    [DatabaseGenerated([DatabaseGeneratedOption]::Identity)]
    [int]$Id

    [string]$Name

    [ForeignKey("TableThreeId")]
    [ICollection[TestTableTwo]]$TableTwos

}