#!/bin/bash

echo "🚀 Deploying TripWise Jamaica Backend to Railway..."

# Build and publish the application
echo "📦 Building application..."
dotnet restore TripWiseJa.sln
dotnet publish src/TripWiseJa.API/TripWiseJa.API.csproj -c Release -o publish --no-self-contained -r linux-x64

echo "✅ Build completed successfully!"
echo "📁 Published files are in the 'publish' directory"
echo ""
echo "🔧 Make sure to set these environment variables in Railway:"
echo "   DATABASE_URL=<your_postgresql_connection_string>"
echo "   FRONTEND_URL=<your_frontend_url>"
echo "   ASPNETCORE_ENVIRONMENT=Production"
echo ""
echo "🌐 Your backend will be available at: https://<your-railway-domain>.railway.app"