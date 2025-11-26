# TripWise Jamaica - Clean Architecture Web API

A modern web application for Jamaica tourism featuring hotels, restaurants, and attractions with user reviews.

## Architecture

- **Domain Layer**: Core entities and business logic
- **Application Layer**: Interfaces and application services
- **Infrastructure Layer**: Data access with Entity Framework Core
- **Presentation Layer**: ASP.NET Core Web API

## Getting Started

### Prerequisites
- .NET 8.0 SDK

### Quick Setup
1. Navigate to backend directory: `cd backend`
2. Run setup script: `./start.sh`

### Manual Setup
1. Restore packages: `dotnet restore`
2. Setup database: `./setup-database.sh`
3. Run the API: `dotnet run --project src/TripWiseJa.API`

### Database
- Uses SQLite (TripWiseJa.db)
- Includes sample data (vendors, locations, reviews)
- No external database server required

### API Endpoints
- GET /api/locations - Get all locations
- GET /api/locations/{id} - Get location by ID
- POST /api/locations - Create new location

## Database Models

### User
- Multi-account user system
- Email-based authentication
- User reviews tracking

### Location
- Hotels, restaurants, attractions
- GPS coordinates
- Average ratings
- Contact information

### Review
- 1-5 star rating system
- User comments
- One review per user per location