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

// ============================================================
// 1. CONFIGURAÇÕES DE SERVIÇOS (Injeção de Dependência)
// ============================================================

// Logs para monitorar o que acontece no servidor via console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configuração do Banco de Dados PostgreSQL usando a string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrando Repositórios e Serviços de Negócio
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TokenService>();

// ============================================================
// 2. SEGURANÇA: AUTENTICAÇÃO JWT
// ============================================================
var secretKey = builder.Configuration["Jwt:Key"] ?? "MinhaChaveSuperSecretaDeDesenvolvimento123!";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // Define como true em produção real com SSL
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// ============================================================
// 3. SEGURANÇA: RATE LIMITING E CORS (Crucial para o Vercel)
// ============================================================

// Rate Limiting: Evita ataques de força bruta limitando requisições por IP
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
                "https://frontend-teste-nu.vercel.app", // <--- ADICIONE ESTA LINHA
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});
// CORS: Define quem pode "conversar" com esta API. 
// Sem isso, o navegador do usuário bloqueia as chamadas vindo do Vercel.
var allowedOrigins = "_allowedOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedOrigins,
        policy =>
        {
            policy.WithOrigins(
                "https://back.lhtecnologia.net.br", 
                "https://front.lhtecnologia.net.br",
                "https://frontend-teste-nu.vercel.app", // URL capturada do seu erro de CORS
                "http://localhost:3000") // Para testes locais
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Permite que o front envie o Token JWT nos headers
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ============================================================
// 4. DOCUMENTAÇÃO: SWAGGER COM JWT
// ============================================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LH Tecnologia Auth API", Version = "v1" });
    
    // Adiciona o botão "Authorize" para testar rotas protegidas
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// Finaliza a configuração e constrói o app
var app = builder.Build();

// ============================================================
// 5. PIPELINE DE EXECUÇÃO (A ORDEM DAS