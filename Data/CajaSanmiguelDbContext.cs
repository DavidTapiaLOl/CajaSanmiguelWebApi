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
        public DbSet<CalendarioPago> CalendarioPagos { get; set; }
        public DbSet<Pago> Pagos {get; set;}
        public DbSet<Usuario> Usuarios {get; set;}

        // Opcional: Configuración de la relación Pagos -> Cuota
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Definir la relación explícitamente si EF Core no la detecta por convención
            modelBuilder.Entity<Pago>()
            .HasOne(p => p.Cuota) // Un Pago tiene una Cuota
            .WithMany(cp => cp.Pagos) // Una Cuota puede tener muchos Pagos (aunque en tu lógica solo debería tener uno completado)

            .OnDelete(DeleteBehavior.Cascade); 


            // --- SOLUCIÓN AL ERROR DE CICLOS DE ELIMINACIÓN ---

        // 2. Relación Prestamo -> CalendarioPagos
        // Si borras el préstamo, se borran todas las cuotas (CASCADE).
        modelBuilder.Entity<Prestamo>()
            .HasMany(p => p.CalendarioPagos)
            .WithOne(cp => cp.Prestamo)
            .HasForeignKey(cp => cp.IdPrestamo)
            .OnDelete(DeleteBehavior.Cascade); // Mantenemos CASCADE

            
        // 4. (Opcional) Relación Cliente -> Prestamos (Buena práctica)
        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Prestamos)
            .WithOne(p => p.Cliente)
            .HasForeignKey(p => p.IdCliente)
            .OnDelete(DeleteBehavior.Restrict); // Evita borrar un Cliente si tiene préstamos activos.
        }
}   
