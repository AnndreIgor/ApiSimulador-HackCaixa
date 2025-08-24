using System.Reflection;
using Azure.Messaging.EventHubs.Producer;
using Asp.Versioning;
using DotNetEnv;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using ApiSimulador.Context;
using ApiSimulador.Middlewares;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;



var builder = WebApplication.CreateBuilder(args);

// .env
Env.Load();


// Service
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors?.Count > 0)
            .SelectMany(kvp =>
            {
                // Ex.: "$.prazo" vira "prazo"
                var field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key;

                return kvp.Value!.Errors.Select(err =>
                {
                    // Mapeia mensagens técnicas → mensagens amigáveis
                    var msg = err.ErrorMessage;

                    // Regras simples (ajuste como preferir)
                    if (string.IsNullOrWhiteSpace(msg) && err.Exception != null)
                        msg = "Valor inválido.";

                    if (msg.Contains("required", StringComparison.OrdinalIgnoreCase))
                        msg = "Campo obrigatório.";

                    if (msg.Contains("could not be converted", StringComparison.OrdinalIgnoreCase))
                        msg = "Valor inválido.";

                    return new { field, message = msg };
                });
            })
            // se vier duplicado, agrupamos
            .GroupBy(e => new { e.field, e.message })
            .Select(g => new { g.Key.field, g.Key.message })
            .ToArray();

        var problem = new
        {
            title = "Erro de validação",
            status = StatusCodes.Status400BadRequest,
            errors,
            traceId = context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(problem);
    };
});

builder.Services.AddHealthChecks()
    // Liveness: checagem trivial (se o processo está vivo)
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })

    // Readiness: depende dos bancos
    .AddDbContextCheck<MySqlDbContext>(
        name: "mysql",
        tags: new[] { "ready" })

    .AddDbContextCheck<SqlServerDbContext>(
        name: "sqlserver",
        tags: new[] { "ready" });

// Swagger XML
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Simulação de Crédito para o Hackaton 2025",
        Version = "v1",
        Description = "API de simulações de crédito",
        Contact = new OpenApiContact
        {
            Name = "André Igor Pereira (C149941)",
            Email = "andre.i.pereira@caixa.gov.br",
        },
        License = new OpenApiLicense
        {
            Name = "Licença MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// Conexões
string? mySqlConn =
    Environment.GetEnvironmentVariable("MySqlCon")
    ?? builder.Configuration.GetConnectionString("MySql");

string? sqlServerConn =
    Environment.GetEnvironmentVariable("SqlServerCon")
    ?? builder.Configuration.GetConnectionString("SqlServer");

// Registra DbContexts somente se houver conexão
if (!string.IsNullOrWhiteSpace(mySqlConn))
{
    builder.Services.AddDbContext<MySqlDbContext>(opt =>
        opt.UseMySql(mySqlConn, new MySqlServerVersion(new Version(8, 0, 36))));
}

if (!string.IsNullOrWhiteSpace(sqlServerConn))
{
    builder.Services.AddDbContext<SqlServerDbContext>(opt =>
        opt.UseSqlServer(sqlServerConn));
}

// Event Hubs
builder.Services.AddSingleton<EventHubProducerClient?>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("EventHubs");
    var conn = Environment.GetEnvironmentVariable("EventHubCon")
              ?? cfg["ConnectionString"];
    var hub = Environment.GetEnvironmentVariable("EventHubName")
              ?? cfg["EventHubName"];

    if (string.IsNullOrWhiteSpace(conn))
    {
        return null;
    }

    return string.IsNullOrWhiteSpace(hub)
        ? new EventHubProducerClient(conn)
        : new EventHubProducerClient(conn, hub);
});

// URLs minúsculas
builder.Services.AddRouting(o =>
{
    o.LowercaseUrls = true;
    o.LowercaseQueryStrings = true;
});

// Versionamento
builder.Services
    .AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
        o.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("x-api-version"));
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var mensagens = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => new {mensagem = e.ErrorMessage })
                .ToList();

            var payload = new
            {
                erro = StatusCodes.Status400BadRequest,
                mensagens
            };

            return new BadRequestObjectResult(payload);
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MySqlDbContext>();
    db.Database.Migrate();
}


// Swagger
app.UseStaticFiles();
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Simulação v1");
    c.InjectStylesheet("/swagger/custom.css");   // adiciona CSS
});

app.UseHttpsRedirection();

app.UseAuthorization();

// Middleware de captura para EventHub somente se o producer existir
var ehProducer = app.Services.GetService<EventHubProducerClient?>();
if (ehProducer is not null)
{
    app.UseEventHubResponseCapture();
}

// logging
app.UseRequestDbLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async ctx =>
        {
            var feature = ctx.Features.Get<IExceptionHandlerFeature>();
            var ex = feature?.Error;

            ctx.Response.ContentType = "application/json; charset=utf-8";
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // Logue ex aqui (Serilog, ILogger, etc.)
            // _logger.LogError(ex, "Erro não tratado");

            var payload = new
            {
                title = "Erro interno",
                status = 500,
                message = "Ocorreu um erro ao processar sua solicitação.",
                traceId = ctx.TraceIdentifier
            };

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
        });
    });
}

app.MapControllers();

// https://localhost:7287/health
app.MapHealthChecks("/");

app.Run();
