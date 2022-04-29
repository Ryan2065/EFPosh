using namespace System.ComponentModel.DataAnnotations.Schema;
using namespace System.ComponentModel.DataAnnotations;
using namespace System.Collections.Generic;

Class TestTableOne {
    [Key()]
    [DatabaseGenerated([DatabaseGeneratedOption]::Identity)]
    [int] $Id
    
    [string]$Name

    [TestTableTwo]$TableTwo
}

Class TestTableTwo {
    [Key()]
    [DatabaseGenerated([DatabaseGeneratedOption]::Identity)]
    [int]$Id

    [int]$TableOneId
    [ForeignKey("TableOneId")]
    [TestTableOne]$TableOne

    [string]$Name

    [int]$TableThreeId
    [ForeignKey("TableThreeId")]
    [TestTableThree]$TableThree
}

Class TestTableThree {
    [Key()]
    [DatabaseGenerated([DatabaseGeneratedOption]::Identity)]
    [int]$Id

    [string]$Name

    [ICollection[TestTableTwo]]$TableTwos

}