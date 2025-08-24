using ApiSimulador.Context;
using ApiSimulador.Models;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiSimulador.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Desliga EventHub pra não depender de nada externo
            services.RemoveAll<EventHubProducerClient>();

            // Remove DbContexts reais se estiverem cadastrados
            var mysqlDesc = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<MySqlDbContext>));
            if (mysqlDesc != null) services.Remove(mysqlDesc);
            var sqlDesc = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<SqlServerDbContext>));
            if (sqlDesc != null) services.Remove(sqlDesc);

            // Registra SQLite in-memory para ambos os DbContexts
            var mysqlConn = new SqliteConnection("DataSource=:memory:");
            mysqlConn.Open();
            services.AddDbContext<MySqlDbContext>(o => o.UseSqlite(mysqlConn));

            var sqlConn = new SqliteConnection("DataSource=:memory:");
            sqlConn.Open();
            services.AddDbContext<SqlServerDbContext>(o => o.UseSqlite(sqlConn));

            // Cria DBs e faz seed
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var mysql = scope.ServiceProvider.GetRequiredService<MySqlDbContext>();
            var sql = scope.ServiceProvider.GetRequiredService<SqlServerDbContext>();

            mysql.Database.EnsureCreated();
            sql.Database.EnsureCreated();

            // Produto para habilitar /simular
            if (!sql.PRODUTO.Any())
            {
                sql.PRODUTO.Add(new Produto
                {
                    CO_PRODUTO = 1,
                    NO_PRODUTO = "Produto Teste",
                    PC_TAXA_JUROS = 0.02m,   // 2% ao mês
                    NU_MINIMO_MESES = 1,
                    NU_MAXIMO_MESES = 60,
                    VR_MINIMO = 100m,
                    VR_MAXIMO = 100000m
                });
                sql.SaveChanges();
            }
        });
    }
}
