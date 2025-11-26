using TripWiseJa.Application.Interfaces;
using TripWiseJa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", 
                "https://localhost:3000",
                "https://tripwiseja-frontend-production.up.railway.app")
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
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

Console.WriteLine("🚀 Backend API starting...");
Console.WriteLine($"🌐 Listening on port: {port}");
Console.WriteLine($"🌍 CORS enabled for frontend: https://tripwiseja-frontend-production.up.railway.app");

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

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(context);
}

app.Run();