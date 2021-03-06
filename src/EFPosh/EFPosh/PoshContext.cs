﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;

namespace EFPosh
{
    public class PoshContext : DbContext
    {
        private PoshEntity[] _types;
        public PoshContext(DbContextOptions options, PoshEntity[] Types) : base(options)
        {
            _types = Types;
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
        }
    }
}
