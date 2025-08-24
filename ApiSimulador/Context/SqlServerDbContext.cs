using ApiSimulador.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSimulador.Context;

public class SqlServerDbContext : DbContext
{
    public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options) { }

    public DbSet<Produto> PRODUTO { get; set; }
}
