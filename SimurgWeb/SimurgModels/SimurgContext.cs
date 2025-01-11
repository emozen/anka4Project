using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SimurgWeb.SimurgModels;

public partial class SimurgContext : DbContext
{
    public SimurgContext()
    {
    }

    public SimurgContext(DbContextOptions<SimurgContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblItem> TblItems { get; set; }

    public virtual DbSet<TblLog> TblLogs { get; set; }

    public virtual DbSet<TblPeriod> TblPeriods { get; set; }

    public virtual DbSet<TblPeriodItem> TblPeriodItems { get; set; }

    public virtual DbSet<TblProject> TblProjects { get; set; }

    public virtual DbSet<TblProjectAuthorize> TblProjectAuthorizes { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblItem>(entity =>
        {
            entity.ToTable("Tbl_Items");

            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Definition).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.TblItemCreatedUsers)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Items_Tbl_Users");

            entity.HasOne(d => d.DeleteUser).WithMany(p => p.TblItemDeleteUsers)
                .HasForeignKey(d => d.DeleteUserId)
                .HasConstraintName("FK_Tbl_Items_DeleteUser");

            entity.HasOne(d => d.Project).WithMany(p => p.TblItems)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Items_Tbl_Projects");
        });

        modelBuilder.Entity<TblLog>(entity =>
        {
            entity.ToTable("Tbl_Logs");

            entity.Property(e => e.Action).HasMaxLength(150);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Definition).HasMaxLength(250);
            entity.Property(e => e.PageName).HasMaxLength(50);

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.TblLogs)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Logs_Tbl_Logs");
        });

        modelBuilder.Entity<TblPeriod>(entity =>
        {
            entity.ToTable("Tbl_Periods");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TblPeriodItem>(entity =>
        {
            entity.ToTable("Tbl_PeriodItems");

            entity.Property(e => e.Description).HasMaxLength(150);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Period).WithMany(p => p.TblPeriodItems)
                .HasForeignKey(d => d.PeriodId)
                .HasConstraintName("FK_Tbl_PeriodItems_Tbl_Periods");
        });

        modelBuilder.Entity<TblProject>(entity =>
        {
            entity.ToTable("Tbl_Projects");

            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.ProjectName).HasMaxLength(250);

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.TblProjects)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Projects_Tbl_Users");
        });

        modelBuilder.Entity<TblProjectAuthorize>(entity =>
        {
            entity.ToTable("Tbl_Project_Authorize");

            entity.HasOne(d => d.Project).WithMany(p => p.TblProjectAuthorizes)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Project_Authorize_Tbl_Project_Authorize");

            entity.HasOne(d => d.User).WithMany(p => p.TblProjectAuthorizes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Project_Authorize_Tbl_Users");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_users");

            entity.ToTable("Tbl_Users");

            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(150);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
