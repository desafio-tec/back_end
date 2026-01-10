using System.Text;
using AuthApi.Data;
using AuthApi.Repositories;
using AuthApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE AMBIENTE (Crucial para o Render) ---
// Força o sistema a ler as variáveis que você cadastrou no painel do Render
builder.Configuration.AddEnvironmentVariables();

// --- 2. LOGS ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// --- 3. BANCO DE DADOS ---
// O GetConnectionString vai procurar pela chave 'ConnectionStrings__DefaultConnection' no Render
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("CRITICAL ERROR: Connection String não encontrada nas variáveis de ambiente!");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 4. INJEÇÃO DE DEPENDÊNCIA ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TokenService>();

// --- 5. CONFIGURAÇÃO DO JWT ---
// Busca 'Jwt__Key' do painel do Render. Se não achar, usa uma padrão.
var secretKey = builder.Configuration["Jwt:Key"] ?? "MinhaChaveSuperSecretaDeDesenvolvimento123!";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// --- 6. RATE LIMITING E CORS ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(10)
            }));
});

var allowedOrigins = "_allowedOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedOrigins,
        policy =>
        {
            policy.WithOrigins(
                "https://back.lhtecnologia.net.br", 
                "https://front.lhtecnologia.net.br",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- 7. SWAGGER ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LH Tecnologia Auth API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// --- 8. PIPELINE DE EXECUÇÃO ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(allowedOrigins);
app.UseRateLimiter();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// --- 9. MIGRAÇÃO AUTOMÁTICA COM TRATAMENTO DE ERROS ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        Console.WriteLine("Iniciando Migração do Banco de Dados...");
        dbContext.Database.Migrate();
        Console.WriteLine("Sucesso: Banco de Dados atualizado.");
    }
    catch (Exception ex)
    {
        // Se der erro 500, o motivo aparecerá aqui nos Logs do Render
        Console.WriteLine($"ERRO AO MIGRAR BANCO: {ex.Message}");
        if (ex.InnerException != null)
            Console.WriteLine($"DETALHE: {ex.InnerException.Message}");
    }
}

// --- 10. INICIALIZAÇÃO ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");