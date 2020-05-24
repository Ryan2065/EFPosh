using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;

namespace EFPosh
{
    public class PoshContext : DbContext
    {
        private PoshEntity[] _types;
        private PoshEntityRelationship[] _relationships;
        public PoshContext(DbContextOptions options, PoshEntity[] Types, PoshEntityRelationship[] Relationships) : base(options)
        {
            _types = Types;
            _relationships = Relationships;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var t in _types)
            {
                if (t.Keyless)
                {
                    modelBuilder.Query(t.Type);
                }
                else
                {
                    modelBuilder.Entity(t.Type);
                }
                if (t.PrimaryKeys != null)
                {
                    modelBuilder.Entity(t.Type).HasKey(t.PrimaryKeys);
                }
                if (!string.IsNullOrEmpty(t.TableName))
                {
                    if (!string.IsNullOrEmpty(t.Schema))
                    {
                        if (t.Keyless)
                        {
                            modelBuilder.Query(t.Type).ToView(t.TableName, t.Schema);
                        }
                        else
                        {
                            modelBuilder.Entity(t.Type).ToTable(t.TableName, t.Schema);
                        }
                    }
                    else
                    {
                        if (t.Keyless)
                        {
                            modelBuilder.Query(t.Type).ToView(t.TableName);
                        }
                        else
                        {
                            modelBuilder.Entity(t.Type).ToTable(t.TableName);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(t.Schema))
                {
                    if (t.Keyless)
                    {
                        modelBuilder.Query(t.Type).ToView(t.Type.Name, t.Schema);
                    }
                    else
                    {
                        modelBuilder.Entity(t.Type).ToTable(t.Type.Name, t.Schema);
                    }
                }
            }
            if(_relationships != null)
            {
                foreach(var r in _relationships)
                {
                    var sourceType = modelBuilder.Model.GetEntityTypes().Where(p => p.Name.Equals(r.SourceTypeName)).FirstOrDefault();
                    if (r.RelationshipType == PoshEntityRelationshipType.ManyToOne)
                    {
                        modelBuilder.Entity(sourceType.ClrType)
                            .HasMany(r.SourceRelationshipProperty)
                            .WithOne(r.TargetRelationshipProperty)
                            .HasForeignKey(r.SourceKey)
                            .HasPrincipalKey(r.TargetKey);
                    }
                    else
                    {
                        ReferenceNavigationBuilder modelType = null;
                        if (sourceType.IsQueryType)
                        {
                            modelType = modelBuilder.Query(sourceType.ClrType).HasOne(r.SourceRelationshipProperty);
                        }
                        else
                        {
                            modelType = modelBuilder.Entity(sourceType.ClrType).HasOne(r.SourceRelationshipProperty);

                        }
                        switch (r.RelationshipType)
                        {
                            case PoshEntityRelationshipType.OneToOne:
                                modelType.WithOne(r.TargetRelationshipProperty)
                                    .HasForeignKey(r.SourceKey)
                                    .HasPrincipalKey(r.TargetKey);
                                break;
                            case PoshEntityRelationshipType.OneToMany:
                            default:
                                modelType.WithMany(r.TargetRelationshipProperty)
                                    .HasForeignKey(r.SourceKey)
                                    .HasPrincipalKey(r.TargetKey);
                                break;
                        }
                    }
                    
                    
                }
            }
        }
    }
}
