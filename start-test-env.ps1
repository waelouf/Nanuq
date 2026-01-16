# Nanuq Test Environment Startup Script
# This script starts all required services for testing Nanuq

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Nanuq Test Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
docker info > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker is running" -ForegroundColor Green
Write-Host ""

# Start services
Write-Host "Starting services..." -ForegroundColor Yellow
docker-compose -f docker-compose.test.yml up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Services started successfully!" -ForegroundColor Green
    Write-Host ""

    # Wait for services to be healthy
    Write-Host "Waiting for services to be healthy (this may take 30-60 seconds)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Service Information" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""

    Write-Host "Kafka:" -ForegroundColor Green
    Write-Host "  Bootstrap Server: localhost:9092" -ForegroundColor White
    Write-Host "  Authentication:   None (PLAINTEXT)" -ForegroundColor White
    Write-Host ""

    Write-Host "RabbitMQ:" -ForegroundColor Green
    Write-Host "  Server URL:     localhost:5672" -ForegroundColor White
    Write-Host "  Management UI:  http://localhost:15672" -ForegroundColor White
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Password: admin123" -ForegroundColor White
    Write-Host ""

    Write-Host "Redis:" -ForegroundColor Green
    Write-Host "  Server URL: localhost:6379" -ForegroundColor White
    Write-Host "  Password:   redis123" -ForegroundColor White
    Write-Host ""

    Write-Host "Kafka UI (Optional):" -ForegroundColor Green
    Write-Host "  URL: http://localhost:8090" -ForegroundColor White
    Write-Host ""

    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To check service status:" -ForegroundColor Yellow
    Write-Host "  docker-compose -f docker-compose.test.yml ps" -ForegroundColor White
    Write-Host ""
    Write-Host "To view logs:" -ForegroundColor Yellow
    Write-Host "  docker-compose -f docker-compose.test.yml logs -f" -ForegroundColor White
    Write-Host ""
    Write-Host "To stop services:" -ForegroundColor Yellow
    Write-Host "  docker-compose -f docker-compose.test.yml down" -ForegroundColor White
    Write-Host ""
    Write-Host "For detailed instructions, see TEST-CREDENTIALS.md" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "ERROR: Failed to start services" -ForegroundColor Red
    Write-Host "Check Docker logs for details" -ForegroundColor Yellow
    exit 1
}
