using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace AIForRentersAPI.Models
{
    public partial class AIForRentersDbContext : DbContext
    {
        public AIForRentersDbContext()
        {

        }

        public AIForRentersDbContext(DbContextOptions<AIForRentersDbContext> options)
            : base(options)
        {

        }

        public virtual DbSet<Availability> Availability { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<EmailTemplate> EmailTemplate { get; set; }
        public virtual DbSet<Property> Property { get; set; }
        public virtual DbSet<Request> Request { get; set; }
        public virtual DbSet<Unit> Unit { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=air2020.database.windows.net;Initial Catalog=SE20E01_DB;User ID=air2020;Password=QWERTZqwertz1234;Connect Timeout=60;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"); 
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Availability>(entity =>
            {
                entity.HasOne(d => d.Unit)
                    .WithMany(p => p.Availability)
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Availability_Unit");
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(e => e.Email).IsUnicode(false);

                entity.Property(e => e.Name).IsUnicode(false);

                entity.Property(e => e.Surname).IsUnicode(false);
            });

            modelBuilder.Entity<EmailTemplate>(entity =>
            {
                entity.Property(e => e.Name).IsUnicode(false);

                entity.Property(e => e.TemplateContent).IsUnicode(false);
            });

            modelBuilder.Entity<Property>(entity =>
            {
                entity.Property(e => e.Location).IsUnicode(false);

                entity.Property(e => e.Name).IsUnicode(false);
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.Property(e => e.Property).IsUnicode(false);

                entity.Property(e => e.ResponseBody).IsUnicode(false);

                entity.Property(e => e.ResponseSubject).IsUnicode(false);

                entity.Property(e => e.Unit).IsUnicode(false);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Request)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Request_Client");
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.Property(e => e.Name).IsUnicode(false);

                entity.HasOne(d => d.Property)
                    .WithMany(p => p.Unit)
                    .HasForeignKey(d => d.PropertyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Unit_Property");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
