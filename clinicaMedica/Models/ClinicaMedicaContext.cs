using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace clinicaMedica.Models;

public partial class ClinicaMedicaContext : DbContext
{
    public ClinicaMedicaContext()
    {
    }

    public ClinicaMedicaContext(DbContextOptions<ClinicaMedicaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cita> Citas { get; set; }

    public virtual DbSet<Especialidad> Especialidads { get; set; }

    public virtual DbSet<Medico> Medicos { get; set; }

    public virtual DbSet<Paciente> Pacientes { get; set; }

    public virtual DbSet<Tratamiento> Tratamientos { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__citas__3213E83FA9512E66");

            entity.ToTable("citas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EspecialidadId).HasColumnName("especialidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("programada")
                .HasColumnName("estado");
            entity.Property(e => e.FechaHora)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.MedicoId).HasColumnName("medico_id");
            entity.Property(e => e.PacienteId).HasColumnName("paciente_id");

            entity.HasOne(d => d.Especialidad).WithMany(p => p.Cita)
                .HasForeignKey(d => d.EspecialidadId)
                .HasConstraintName("FK__citas__especiali__4222D4EF");

            entity.HasOne(d => d.Medico).WithMany(p => p.Cita)
                .HasForeignKey(d => d.MedicoId)
                .HasConstraintName("FK__citas__medico_id__412EB0B6");

            entity.HasOne(d => d.Paciente).WithMany(p => p.Cita)
                .HasForeignKey(d => d.PacienteId)
                .HasConstraintName("FK__citas__paciente___403A8C7D");
        });

        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__especial__3213E83F3BD34535");

            entity.ToTable("especialidad");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__medicos__3213E83FDE1F76FC");

            entity.ToTable("medicos");

            entity.HasIndex(e => e.Email, "UQ__medicos__AB6E61647245A1DB").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellido)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Especialidad)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("especialidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__paciente__3213E83F7682CCBF");

            entity.ToTable("pacientes");

            entity.HasIndex(e => e.Email, "UQ__paciente__AB6E6164F5E3FF00").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellido)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.Foto)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("foto");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Tratamiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tratamie__3213E83FEA77AD50");

            entity.ToTable("tratamientos");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CitaId).HasColumnName("cita_id");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Notas)
                .HasColumnType("text")
                .HasColumnName("notas");

            entity.HasOne(d => d.Cita).WithMany(p => p.Tratamientos)
                .HasForeignKey(d => d.CitaId)
                .HasConstraintName("FK__tratamien__cita___45F365D3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
