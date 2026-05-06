Write-Host "Starting Snapflow deployment..." -ForegroundColor Cyan

# Navigate to the project root directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location -Path "$ScriptDir\.."

# Check if Docker is running
docker info > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker is not running or not recognized. Please start Docker Desktop." -ForegroundColor Red
    Write-Host "Note: If you just installed Docker, you may need to restart VS Code so the terminal can see it." -ForegroundColor Yellow
    exit 1
}

Write-Host "Building and starting containers (Docker Compose)..." -ForegroundColor Yellow
docker-compose -f deployment/docker-compose.yml up -d --build

Write-Host "Deployment successful!" -ForegroundColor Green
Write-Host "Web client and API are now running."
Write-Host "To view logs, run: docker-compose -f deployment/docker-compose.yml logs -f"