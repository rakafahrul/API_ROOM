using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Room_App.Data;
using Room_App.Services;
using System;
using System.Text;
using System.Threading.Tasks;
using Room_App.Utility;

var builder = WebApplication.CreateBuilder(args);

// RAILWAY CONFIGURATION: Simplified untuk Railway
if (builder.Environment.IsProduction())
{
    // Railway: Gunakan UseUrls saja untuk production
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://*:{port}");
    Console.WriteLine($"Railway: Configured to listen on port: {port}");
}
else
{
    // Development: Configure Kestrel
    builder.WebHost.ConfigureKestrel(serverOptions => 
    {
        serverOptions.ListenLocalhost(5228); // HTTP
        serverOptions.ListenLocalhost(7143, listenOptions => {
            listenOptions.UseHttps();
        });
    });
}

// RAILWAY: Configure forwarded headers untuk proxy
if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Room App API",
        Version = "v1",
        Description = "API untuk aplikasi manajemen ruangan"
    });

    // Tambahkan konfigurasi untuk autentikasi JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header menggunakan skema Bearer. Masukkan token JWT Anda."
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

// Configure Npgsql to use snake_case naming convention
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add database context with snake_case naming convention
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseSnakeCaseNamingConvention();
});

// Register services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPhotoUsageService, PhotoUsageService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IUserService, UserService>();

// Konfigurasi JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key tidak ditemukan dalam konfigurasi");
}

var key = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Penting untuk Railway
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});

// Enhanced CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("*");
    });
});

var app = builder.Build();

// RAILWAY: Use forwarded headers in production
if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}

// Database migration dan seeding
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();
        await DataHelper.ManageDataAsync(scope.ServiceProvider);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database migration error: {ex.Message}");
    // Jangan stop aplikasi karena database error
}

// Configure the HTTP request pipeline
// RAILWAY: Always enable Swagger untuk testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Room App API v1");
    c.RoutePrefix = "swagger";
    c.ConfigObject.AdditionalItems["ignoreSslErrors"] = true;
});

// Global exception handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        
        var response = new
        {
            error = "Internal Server Error",
            message = exception?.Message,
            timestamp = DateTime.UtcNow
        };
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        
        Console.WriteLine($"Error: {exception?.Message}");
        Console.WriteLine($"Stack trace: {exception?.StackTrace}");
    });
});

// RAILWAY: Hanya gunakan HTTPS redirect di development
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// IMPORTANT: UseCors must be before UseAuthentication and UseAuthorization
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint dan root endpoint untuk Railway
app.MapGet("/", () => new { 
    message = "Room App API is running", 
    status = "OK", 
    timestamp = DateTime.UtcNow,
    endpoints = new {
        swagger = "/swagger",
        health = "/health",
        api = "/api"
    }
});

app.MapGet("/health", () => new { status = "OK", timestamp = DateTime.UtcNow });

try
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    var environment = app.Environment.EnvironmentName;
    
    Console.WriteLine($"=== Room App API Starting ===");
    Console.WriteLine($"Environment: {environment}");
    Console.WriteLine($"Port: {port}");
    
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("HTTP: http://localhost:5228");
        Console.WriteLine("HTTPS: https://localhost:7143");
        Console.WriteLine("Swagger: http://localhost:5228/swagger");
    }
    else
    {
        Console.WriteLine("Swagger: /swagger");
        Console.WriteLine("Health: /health");
    }
    
    Console.WriteLine("=== API Ready ===");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw; // Re-throw untuk Railway dapat mendeteksi failure
}
