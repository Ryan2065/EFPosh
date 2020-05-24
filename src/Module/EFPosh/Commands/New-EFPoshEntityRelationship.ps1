Function New-EFPoshEntityRelationship {
    <#
    .SYNOPSIS
    Builds a relationship between two Entities
    
    .DESCRIPTION
    Builds a relationship (Foreign Key) between two entities
    
    .PARAMETER SourceTypeName
    The type of the source table
    
    .PARAMETER TargetTypeName
    The type of the target table
    
    .PARAMETER RelationshipType
    The type of relationship we are building
    
    .PARAMETER SourceKey
    The key of the source table
    
    .PARAMETER TargetKey
    The key of the target table
    
    .PARAMETER SourceRelationshipProperty
    The source property (Needs to be the Target table type)
    
    .PARAMETER TargetRelationshipProperty
    The target property (Needs to be the source table type)
    
    .NOTES
    .Author: Ryan Ephgrave
    #>#
    Param(
        [Parameter(Mandatory = $true)]
        [string]$SourceTypeName,
        [Parameter(Mandatory = $true)]
        [string]$TargetTypeName,
        [Parameter(Mandatory = $true)]
        [ValidateSet('OneToOne', 'OneToMany', 'ManyToOne')]
        [string]$RelationshipType,
        [Parameter(Mandatory = $true)]
        [string]$SourceKey,
        [Parameter(Mandatory = $true)]
        [string]$TargetKey,
        [Parameter(Mandatory = $true)]
        [string]$SourceRelationshipProperty,
        [Parameter(Mandatory = $true)]
        [string]$TargetRelationshipProperty
    )
    $NewRelationship = [EFPosh.PoshEntityRelationship]::new()
    $NewRelationship.SourceTypeName = $SourceTypeName
    $NewRelationship.TargetTypeName = $TargetTypeName
    $NewRelationship.RelationshipType = $RelationshipType
    $NewRelationship.SourceKey = $SourceKey
    $NewRelationship.TargetKey = $TargetKey
    $NewRelationship.SourceRelationshipProperty = $SourceRelationshipProperty
    $NewRelationship.TargetRelationshipProperty = $TargetRelationshipProperty
    return $NewRelationship
}