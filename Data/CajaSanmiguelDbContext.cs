using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
namespace CajaSanmiguel;
public class CajaSanmiguelDbContext: DbContext
{
public CajaSanmiguelDbContext(DbContextOptions<CajaSanmiguelDbContext> options)
            : base(options)
        {
        }
        // Mapeo de Tablas
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }  
        public DbSet<Pago> Pagos {get; set;}
        public DbSet<Usuario> Usuarios {get; set;}
        //Configuración de la relación Pagos -> Cuota
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         //Relación Cliente -> Prestamos
        modelBuilder.Entity<Cliente>()
             .HasMany(c => c.Prestamos)// El Cliente tiene Muchos Préstamos
            .WithOne(p => p.Cliente)// Cada Préstamo tiene Un Cliente 
            .HasForeignKey(p => p.IdCliente)// La clave foránea está en la tabla Prestamo
            .OnDelete(DeleteBehavior.Restrict); // Evita borrar un Cliente si tiene préstamos activos.  
        }
}   
