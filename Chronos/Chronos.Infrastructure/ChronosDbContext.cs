using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Chronos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chronos.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class ChronosDbContext : DbContext
    {
        public ChronosDbContext(DbContextOptions<ChronosDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Examen> Examenes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Nombre).HasMaxLength(100);
                entity.Property(u => u.Apellido).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(200);
            });

            modelBuilder.Entity<Tarea>(entity =>
            {
                entity.Property(t => t.Prioridad).HasMaxLength(20);
                entity.Property(t => t.Estado).HasMaxLength(20);
                entity.HasOne(t => t.Usuario)
                      .WithMany(u => u.Tareas)
                      .HasForeignKey(t => t.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Examen>(entity =>
            {
                entity.Property(e => e.Prioridad).HasMaxLength(20);
                entity.Property(e => e.Estado).HasMaxLength(20);
                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Examenes)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}