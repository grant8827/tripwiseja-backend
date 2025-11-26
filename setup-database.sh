#!/bin/bash

echo "🏝️ Setting up TripWise Jamaica Database..."

# Restore packages
echo "📦 Restoring packages..."
dotnet restore

# Remove existing migrations (if any)
echo "🗑️ Cleaning up old migrations..."
rm -rf src/TripWiseJa.Infrastructure/Migrations

# Create new migration
echo "📝 Creating database migration..."
dotnet ef migrations add InitialCreate --project src/TripWiseJa.Infrastructure --startup-project src/TripWiseJa.API

# Update database
echo "🗄️ Creating SQLite database..."
dotnet ef database update --project src/TripWiseJa.Infrastructure --startup-project src/TripWiseJa.API

echo "✅ Database setup complete!"
echo "🚀 Run 'dotnet run --project src/TripWiseJa.API' to start the server"