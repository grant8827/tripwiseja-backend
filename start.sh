#!/bin/bash

echo "🏝️ Starting TripWise Jamaica Backend..."

# Check if database exists
if [ ! -f "TripWiseJa.db" ]; then
    echo "📊 Database not found. Setting up..."
    ./setup-database.sh
fi

echo "🚀 Starting API server..."
dotnet run --project src/TripWiseJa.API