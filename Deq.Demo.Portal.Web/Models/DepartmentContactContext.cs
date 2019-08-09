using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Deq.Demo.Portal.Web.Models
{
    public partial class DepartmentContactContext : DbContext
    {
        public DepartmentContactContext()
        {
        }

        public DepartmentContactContext(DbContextOptions<DepartmentContactContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DepartmentContact> DepartmentContact { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=deqdemodepartmentcontact;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.2-servicing-10034");

            modelBuilder.Entity<DepartmentContact>(entity =>
            {
                entity.ToTable("department_contact");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ContactName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentName)
                    .IsRequired()
                    .IsUnicode(false);
            });
        }
    }
}
