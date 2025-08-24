using ApiSimulador.Models;
using Microsoft.EntityFrameworkCore;
//dotnet ef migrations add NoMigration -c MySqlDbContext
//dotnet ef database update -c MySqlDbContext

namespace ApiSimulador.Context
{
    public class MySqlDbContext: DbContext
    {
        public MySqlDbContext(DbContextOptions<MySqlDbContext> options) : base(options) { }

        public DbSet<Simulacao> SIMULACAO { get; set; }
        public DbSet<Parcela> PARCELA { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
    }
}
