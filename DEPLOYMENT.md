# Global Disaster Management System - Deployment Guide

## Prerequisites
- Docker Desktop installed
- Docker Compose installed
- .NET 8.0 SDK (for local development)
- SQL Server (for production)
- Twilio Account (optional - for SMS/WhatsApp/Voice)
- Firebase Account (optional - for push notifications)

## Quick Start with Docker Compose

### 1. Clone the repository
```bash
git clone https://github.com/Karinateii/FloodManagementSystem.git
cd FloodManagementSystem
```

### 2. Configure Environment Variables
Edit `docker-compose.yml` and update the following:
- SQL Server password
- Email settings (SMTP credentials)
- Twilio credentials (for SMS/WhatsApp/Voice)
- Firebase credentials (for push notifications)

**Important:** For production, use environment variables or secrets management instead of hardcoding credentials.

### 3. Build and Run
```bash
docker-compose up -d
```

### 4. Access the Application
- Web Application: http://localhost:8080
- Swagger API Docs: http://localhost:8080/api/docs
- SQL Server: localhost:1433
- Redis: localhost:6379

### 5. Initialize Database
The application will automatically run migrations on first startup.

## Docker Commands

### Build the application
```bash
docker-compose build
```

### Start services
```bash
docker-compose up -d
```

### Stop services
```bash
docker-compose down
```

### View logs
```bash
docker-compose logs -f webapp
```

### Restart a service
```bash
docker-compose restart webapp
```

### Remove all containers and volumes
```bash
docker-compose down -v
```

## Production Deployment

### 1. Environment Variables
Create a `.env` file with production values:
```env
DB_PASSWORD=YourProductionPassword
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_EMAIL=your-email@gmail.com
SMTP_PASSWORD=your-app-password
```

### 2. Update docker-compose for production
Use `docker-compose.prod.yml` for production settings:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### 3. SSL/TLS Configuration
For HTTPS support, mount SSL certificates:
```yaml
volumes:
  - ./certs:/https:ro
environment:
  - ASPNETCORE_URLS=https://+:443;http://+:80
  - ASPNETCORE_Kestrel__Certificates__Default__Password=cert-password
  - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pfx
```

## Health Checks

### Application Health Endpoint
```bash
curl http://localhost:8080/health
```

### Database Health
```bash
docker exec floodmanagement-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"
```

### Redis Health
```bash
docker exec floodmanagement-redis redis-cli ping
```

## Backup and Restore

### Backup Database
```bash
docker exec floodmanagement-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -Q "BACKUP DATABASE FloodManagementDB TO DISK='/var/opt/mssql/backup/flood_db.bak'"

docker cp floodmanagement-sqlserver:/var/opt/mssql/backup/flood_db.bak ./backups/
```

### Restore Database
```bash
docker cp ./backups/flood_db.bak floodmanagement-sqlserver:/var/opt/mssql/backup/

docker exec floodmanagement-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -Q "RESTORE DATABASE FloodManagementDB FROM DISK='/var/opt/mssql/backup/flood_db.bak' WITH REPLACE"
```

## Monitoring

### View Container Stats
```bash
docker stats floodmanagement-webapp floodmanagement-sqlserver floodmanagement-redis
```

### View Application Logs
```bash
docker logs -f --tail 100 floodmanagement-webapp
```

## Troubleshooting

### Container won't start
```bash
docker-compose logs webapp
docker-compose ps
```

### Database connection issues
1. Check SQL Server is healthy: `docker ps`
2. Verify connection string in `docker-compose.yml`
3. Ensure database migrations ran: Check webapp logs

### Port conflicts
If ports 8080, 1433, or 6379 are in use, modify them in `docker-compose.yml`

## CI/CD Pipeline

### GitHub Actions Workflow
The repository includes a complete CI/CD pipeline that:
1. Builds and tests the application
2. Runs code quality checks
3. Builds and pushes Docker images
4. Performs security scanning
5. Deploys to production

### Required GitHub Secrets
- `DOCKER_USERNAME`: Docker Hub username
- `DOCKER_PASSWORD`: Docker Hub password
- `PROD_SERVER_HOST`: Production server IP/hostname
- `PROD_SERVER_USER`: SSH username
- `PROD_SERVER_SSH_KEY`: SSH private key
- `PROD_URL`: Production URL for health checks

## Scaling

### Scale webapp instances
```bash
docker-compose up -d --scale webapp=3
```

### Add load balancer (nginx)
See `docker-compose.prod.yml` for nginx configuration

## Security Recommendations

1. Change default passwords in production
2. Use Docker secrets for sensitive data
3. Enable firewall rules
4. Use HTTPS in production
5. Regular security updates: `docker-compose pull && docker-compose up -d`
6. Implement rate limiting
7. Enable SQL Server encryption

## Support

For issues and questions:
- GitHub Issues: https://github.com/Karinateii/FloodManagementSystem/issues
- Email: support@lagosflood.com
