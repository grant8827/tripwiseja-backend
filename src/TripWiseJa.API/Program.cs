using TripWiseJa.Application.Interfaces;
using TripWiseJa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                      builder.Configuration.GetConnectionString("DefaultConnection");

// Convert Railway DATABASE_URL format to connection string if needed
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var host = uri.Host;
    var dbPort = uri.Port;
    var database = uri.AbsolutePath.Trim('/');
    var username = uri.UserInfo.Split(':')[0];
    var password = uri.UserInfo.Split(':')[1];
    
    connectionString = $"Host={host};Port={dbPort};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var allowedOrigins = new List<string>
        {
            "http://localhost:3000", 
            "https://localhost:3000"
        };
        
        // Add Railway frontend URL from environment variable or default
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? 
                         "https://tripwiseja-frontend-production.up.railway.app";
        allowedOrigins.Add(frontendUrl);
        
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("*")
              .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
    });
});

// Configure multipart form data limits for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

// Configure port for Railway (uses PORT env variable) or default to 5001 locally
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
var environment = builder.Environment.EnvironmentName;
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? 
                "https://tripwiseja-frontend-production.up.railway.app";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

Console.WriteLine("🚀 TripWise Jamaica Backend API starting...");
Console.WriteLine($"🌐 Environment: {environment}");
Console.WriteLine($"🌐 Listening on port: {port}");
Console.WriteLine($"🌍 CORS enabled for frontend: {frontendUrl}");
Console.WriteLine($"🗄️ Database: {(Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "Railway PostgreSQL" : "Local PostgreSQL")}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Disabled for development

// Serve static files from wwwroot/uploads
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Console.WriteLine($"📁 Created uploads directory: {uploadsPath}");
}

app.UseCors("AllowReactApp");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Initializing database...");
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Run migrations if needed
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations...");
            await context.Database.MigrateAsync();
        }
        
        // Seed initial data
        await DataSeeder.SeedAsync(context);
        
        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

app.Run();