using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using cmty_prjt.Areas.Identity.Data;
using cmty_prjt.Models;

namespace cmty_prjt.Data.Scaffold;

public partial class ClgeventmgmtContext : IdentityDbContext<cmty_prjtUser>
{
    public ClgeventmgmtContext(DbContextOptions<ClgeventmgmtContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.EventDate).IsRequired();
            entity.Property(e => e.Location).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.CreatedById).IsRequired(false);
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
