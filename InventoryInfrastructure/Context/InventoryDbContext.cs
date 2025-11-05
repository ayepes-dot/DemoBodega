using InventoryDomain.InventoryEntities;
using Microsoft.EntityFrameworkCore;

namespace InventoryDatabase.Context
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

        public DbSet<DocumentoTransporte> DocumentosTransporte { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<Referencia> Referencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentoTransporte>()
                .HasKey(d => d.TrasnporDocumentId);

            modelBuilder.Entity<DocumentoTransporte>()
                .Property(d => d.TrasnporDocumentId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Caja>()
                .HasKey(c => c.CajaId);

            modelBuilder.Entity<Caja>()
                .Property(c => c.CajaId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Referencia>()
                .HasKey(r => r.ReferenciaId);

            modelBuilder.Entity<Referencia>()
                .Property(r => r.ReferenciaId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<DocumentoTransporte>()
                .HasMany(d => d.Cajas)
                .WithOne(c => c.DocumentoTransporte)
                .HasForeignKey(c => c.DocumentoTransporteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Caja>()
                .HasMany(c => c.Referencias)
                .WithOne(r => r.Caja)
                .HasForeignKey(r => r.CajaId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
