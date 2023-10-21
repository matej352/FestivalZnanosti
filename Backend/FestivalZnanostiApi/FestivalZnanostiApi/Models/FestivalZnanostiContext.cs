﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FestivalZnanostiApi.Models;

public partial class FestivalZnanostiContext : DbContext
{
    public FestivalZnanostiContext()
    {
    }

    public FestivalZnanostiContext(DbContextOptions<FestivalZnanostiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrator> Administrator { get; set; }

    public virtual DbSet<Event> Event { get; set; }

    public virtual DbSet<FestivalYear> FestivalYear { get; set; }

    public virtual DbSet<Lecturer> Lecturer { get; set; }

    public virtual DbSet<Location> Location { get; set; }

    public virtual DbSet<ParticipantsAge> ParticipantsAge { get; set; }

    public virtual DbSet<Submitter> Submitter { get; set; }

    public virtual DbSet<TimeSlot> TimeSlot { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Administ__3214EC07BAC62675");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(1000)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Event__3214EC078D4BBE6F");

            entity.Property(e => e.Equipment)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Summary)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.FestivalYear).WithMany(p => p.Event)
                .HasForeignKey(d => d.FestivalYearId)
                .HasConstraintName("FK__Event__FestivalY__30F848ED");

            entity.HasOne(d => d.Location).WithMany(p => p.Event)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__Event__LocationI__2F10007B");

            entity.HasOne(d => d.Submitter).WithMany(p => p.Event)
                .HasForeignKey(d => d.SubmitterId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Event__Submitter__300424B4");

            entity.HasMany(d => d.ParticipantsAge).WithMany(p => p.Event)
                .UsingEntity<Dictionary<string, object>>(
                    "ForAge",
                    r => r.HasOne<ParticipantsAge>().WithMany()
                        .HasForeignKey("ParticipantsAgeId")
                        .HasConstraintName("FK__ForAge__Particip__3A81B327"),
                    l => l.HasOne<Event>().WithMany()
                        .HasForeignKey("EventId")
                        .HasConstraintName("FK__ForAge__EventId__398D8EEE"),
                    j =>
                    {
                        j.HasKey("EventId", "ParticipantsAgeId").HasName("PK__ForAge__F266F9B5B0DCD130");
                    });
        });

        modelBuilder.Entity<FestivalYear>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Festival__3214EC07E7FA97CC");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.EndDate).HasColumnType("date");
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Topic)
                .IsRequired()
                .HasMaxLength(300)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lecturer__3214EC07B37EB237");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Resume)
                .HasMaxLength(800)
                .IsUnicode(false);

            entity.HasOne(d => d.Event).WithMany(p => p.Lecturer)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Lecturer__EventI__36B12243");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC072A0D73DD");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ParticipantsAge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Particip__3214EC07EA298C39");

            entity.Property(e => e.Age)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<Submitter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Submitte__3214EC07C811EAAD");

            entity.HasIndex(e => e.Email, "UQ__Submitte__A9D10534CEEEF8B7").IsUnique();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(1000)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TimeSlot__3214EC07D75A0B99");

            entity.Property(e => e.Start).HasColumnType("datetime");

            entity.HasOne(d => d.Location).WithMany(p => p.TimeSlot)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__TimeSlot__Locati__33D4B598");

            entity.HasMany(d => d.Event).WithMany(p => p.TimeSlot)
                .UsingEntity<Dictionary<string, object>>(
                    "During",
                    r => r.HasOne<Event>().WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__During__EventId__3E52440B"),
                    l => l.HasOne<TimeSlot>().WithMany()
                        .HasForeignKey("TimeSlotId")
                        .HasConstraintName("FK__During__TimeSlot__3D5E1FD2"),
                    j =>
                    {
                        j.HasKey("TimeSlotId", "EventId").HasName("PK__During__565853B32D229C78");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}