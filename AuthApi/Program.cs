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

// --- 1. Configuração de Logs ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// --- 2. Banco de Dados ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 3. Injeção de Dependência ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TokenService>();

// --- 4. Configuração do JWT ---
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

// --- 5. Rate Limiting e CORS (AQUI ESTÁ O AJUSTE) ---
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
            // Adicionei a URL que aparece na sua imagem do Vercel
            policy.WithOrigins(
                "https://back.lhtecnologia.net.br", 
                "https://front.lhtecnologia.net.br",
                "https://frontend-teste-nu.vercel.app", // URL do seu print
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Importante para o navegador aceitar a resposta
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- 6. Swagger ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LH Tecnologia Auth API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {seu_token}",
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

var app = builder.Build();

// --- 7. Pipeline (ORDEM É TUDO) ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// O CORS precisa vir antes de quase tudo
app.UseCors(allowedOrigins); 

app.UseRateLimiter();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// Migração Automática
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try 
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Banco de dados migrado com sucesso.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao migrar banco: {ex.Message}");
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");