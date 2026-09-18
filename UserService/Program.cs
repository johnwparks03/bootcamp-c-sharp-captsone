using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using UserService.Clients;
using UserService.Configuration;
using UserService.Data;
using UserService.Middleware;
using UserService.Repositories;
using UserService.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}")
    .WriteTo.File("logs/api-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj} {NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.Configure<ServiceConfiguration>(
    builder.Configuration.GetSection("ServiceConfiguration"));

builder.Services.Configure<HttpClientSettings>(
    builder.Configuration.GetSection("HttpClientSettings"));

// builder.Services.Configure<ExternalServiceSettings>(
//     builder.Configuration.GetSection("ExternalServices:ReservationService"));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Digital Library Management System API", Version = "v1" });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        // Swagger UI adds the "Bearer " prefix for you, so you paste just the token
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter 'Bearer' [space] and then your JWT token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });
    
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

var databaseConnectionString = builder.Configuration["DatabaseSettings:ConnectionString"];
if (string.IsNullOrWhiteSpace(databaseConnectionString))
{
    throw new InvalidOperationException(
        "DatabaseSettings:ConnectionString is not configured.");
}
builder.Services.AddDbContext<UserDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseInMemoryDatabase(databaseConnectionString);
    }
    else
    {
        options.UseNpgsql(databaseConnectionString);
    }
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validate that the token was issued by our server (checks "iss" claim)
            ValidateIssuer = true,

            // Validate that the token is intended for our application (checks "aud" claim)
            ValidateAudience = true,

            // Validate that the token hasn't expired (checks "exp" claim)
            ValidateLifetime = true,

            // Validate that the token's signature is valid and hasn't been tampered with
            ValidateIssuerSigningKey = true,

            // The expected issuer value from appsettings.json (who created the token)
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            // The expected audience value from appsettings.json (who the token is for)
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // The secret key used to sign and verify tokens
            // This key must match between token generation and validation
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),

            // Remove default 5-minute clock skew for more precise expiration validation
            // By default, tokens are given a 5-minute grace period; this removes it
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();


var reservationServiceConfig = builder.Configuration.GetSection("ExternalServices:ReservationService");
var reservationServiceBaseUrl = reservationServiceConfig["BaseUrl"] ?? throw new InvalidOperationException("Reservation Service BaseUrl is not configured.");
var reservationServiceTimeout = reservationServiceConfig.GetValue<int>("TimeoutSeconds", 30);
builder.Services.AddHttpClient<IReservationClient, ReservationClient>(client =>
{
    client.BaseAddress = new Uri(reservationServiceBaseUrl ?? "http://localhost:5003");
    client.Timeout = TimeSpan.FromSeconds(reservationServiceTimeout);
    client.DefaultRequestHeaders.Add("User-Agent", "UserService/1.0");
});

builder.Services.AddHealthChecks().AddDbContextCheck<UserDbContext>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

var serviceConfig = builder.Configuration.GetSection("ServiceConfiguration").Get<ServiceConfiguration>();
if (serviceConfig?.Port > 0)
{
    app.Urls.Add($"http://localhost:{serviceConfig.Port}");
    app.Urls.Add($"https://localhost:{serviceConfig.Port + 1000}"); // HTTPS port
}
app.MapHealthChecks(serviceConfig?.HealthCheckPath ?? "/health");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<UserDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
}

// Add correlation ID middleware (before exception handling)
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-ID"))
    {
        context.Request.Headers["X-Correlation-ID"] = Guid.NewGuid().ToString();
    }

    using (Serilog.Context.LogContext.PushProperty("CorrelationId",
               context.Request.Headers["X-Correlation-ID"].ToString()))
    {
        await next();
    }
});

app.UseMiddleware<GlobalExceptionMiddleware>();

// app.UseHttpsRedirection();
app.MapControllers();

try
{
    Log.Information("Starting User API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
    
}