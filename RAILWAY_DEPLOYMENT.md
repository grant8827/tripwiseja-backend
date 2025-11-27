# TripWise Jamaica Backend - Railway Deployment Guide

## 🚀 Quick Deploy to Railway

### Prerequisites
- Railway account (https://railway.app)
- PostgreSQL database (Railway provides this)

### Step 1: Create New Railway Project
1. Go to https://railway.app
2. Click "New Project"
3. Select "Deploy from GitHub repo"
4. Connect your GitHub repository
5. Select the backend folder: `tripwiseja-backend`

### Step 2: Add PostgreSQL Database
1. In your Railway project dashboard
2. Click "New Service"
3. Select "Database" → "PostgreSQL"
4. Railway will automatically create a PostgreSQL instance

### Step 3: Configure Environment Variables
In your Railway project settings, add these environment variables:

```bash
# Database (Railway will auto-populate DATABASE_URL)
DATABASE_URL=postgresql://postgres:password@host:port/database

# Frontend URL (update with your actual frontend URL)
FRONTEND_URL=https://your-frontend-domain.vercel.app

# Environment
ASPNETCORE_ENVIRONMENT=Production

# Port (Railway auto-sets this)
PORT=8080
```

### Step 4: Deploy
1. Railway will automatically detect the .NET project
2. It will use the `nixpacks.toml` configuration
3. The build process will:
   - Restore NuGet packages
   - Build the application
   - Publish for Linux deployment

### Step 5: Database Migration
The application will automatically:
- Create database tables on first run
- Apply any pending migrations
- Seed initial data

## 🔧 Configuration Files

### nixpacks.toml
Configures the build process for Railway's Nixpacks builder.

### Procfile
Defines how to start the application.

### appsettings.Production.json
Production configuration that uses environment variables.

## 🌐 API Endpoints

Once deployed, your API will be available at:
```
https://your-backend-domain.railway.app/api/
```

### Available Endpoints:
- `POST /api/users/register` - User registration
- `POST /api/users/login` - User login
- `GET /api/locations` - Get all locations
- `POST /api/reviews` - Submit review
- `POST /api/bookings` - Create booking
- And more...

## 🔍 Monitoring

### Logs
View application logs in Railway dashboard:
1. Go to your project
2. Click on your service
3. Navigate to "Logs" tab

### Health Check
Test if your API is running:
```bash
curl https://your-backend-domain.railway.app/api/locations
```

## 🛠️ Troubleshooting

### Common Issues:

1. **Database Connection Failed**
   - Check DATABASE_URL environment variable
   - Ensure PostgreSQL service is running
   - Verify connection string format

2. **CORS Errors**
   - Update FRONTEND_URL environment variable
   - Check allowed origins in Program.cs

3. **Build Failures**
   - Check .NET version compatibility
   - Verify all NuGet packages are restored
   - Review build logs in Railway dashboard

### Debug Commands:
```bash
# Test database connection
dotnet ef database update --startup-project src/TripWiseJa.API

# Run locally with production settings
ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/TripWiseJa.API
```

## 📝 Notes

- The application uses .NET 10
- PostgreSQL is the database provider
- Entity Framework Core handles migrations
- CORS is configured for cross-origin requests
- File uploads are stored in wwwroot/uploads

## 🔄 Updates

To update your deployment:
1. Push changes to your GitHub repository
2. Railway will automatically redeploy
3. Database migrations will run automatically if needed

## 🆘 Support

If you encounter issues:
1. Check Railway logs
2. Verify environment variables
3. Test database connectivity
4. Review CORS configuration