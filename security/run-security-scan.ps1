param(
    [switch]$Stop = $false,
    [switch]$Clean = $false
)

$ErrorActionPreference = 'Stop'

function Write-Status {
    param(
        [string]$Message,
        [ConsoleColor]$Color = [ConsoleColor]::White
    )

    Write-Host $Message -ForegroundColor $Color
}

function Stop-ScanContainers {
    $containers = @(
        'garagemanagement-zap-scan',
        'garagemanagement-http-api-host-scan',
        'garagemanagement-db-migrator-scan',
        'garagemanagement-postgres-scan'
    )

    foreach ($container in $containers) {
        if (docker ps -aq -f name=$container) {
            docker rm -f $container 2>$null | Out-Null
        }
    }
}

function Remove-ScanVolumes {
    $volumeName = docker volume ls -q -f name=garagemanagement-postgres-data-scan | Select-Object -First 1

    if ($volumeName) {
        docker volume rm $volumeName | Out-Null
    }
}

function Test-Prerequisites {
    try {
        docker --version | Out-Null
    }
    catch {
        throw 'Docker is not installed or is not in PATH.'
    }

    if (-not (Test-Path '.env')) {
        throw '.env not found in the repository root.'
    }

    if (-not (Test-Path 'reports/security')) {
        New-Item -ItemType Directory -Path 'reports/security' -Force | Out-Null
    }
}

function Wait-ForApiReady {
    param(
        [int]$MaxAttempts = 60,
        [int]$DelaySeconds = 2
    )

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        try {
            $response = Invoke-WebRequest `
                -Uri 'http://localhost:8080/swagger/v1/swagger.json' `
                -Method Get `
                -TimeoutSec 5 `
                -UseBasicParsing

            if ($response.StatusCode -eq 200) {
                Write-Host "API pronta!" -ForegroundColor Green
                return $true
            }
        }
        catch {
            Write-Host "Aguardando API... tentativa $attempt"
            Start-Sleep -Seconds $DelaySeconds
        }
    }

    return $false
}

if ($Stop) {
    Stop-ScanContainers

    if ($Clean) {
        Remove-ScanVolumes
    }

    exit 0
}

try {
    Test-Prerequisites

    Write-Status 'Starting Docker stack...' Cyan
    Stop-ScanContainers
    Remove-ScanVolumes

    docker-compose -f docker-compose.scan.yml up -d postgres db-migrator http-api-host

    Write-Status 'Waiting for the API to be ready...' Cyan
    if (-not (Wait-ForApiReady)) {
        Write-Status 'API did not start within the timeout.' Red
        docker-compose -f docker-compose.scan.yml logs http-api-host
        docker-compose -f docker-compose.scan.yml down
        exit 1
    }

    Write-Status 'Running OWASP ZAP...' Cyan
    docker-compose -f docker-compose.scan.yml run --rm zap-scan

    docker-compose -f docker-compose.scan.yml down

    $latestReport = Get-ChildItem 'reports/security' -Filter 'security_report_*.html' |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $latestReport) {
        throw 'No HTML report was generated.'
    }

    Write-Status ('Report generated: ' + $latestReport.FullName) Green
    Start-Process $latestReport.FullName
}
catch {
    Write-Status $_.Exception.Message Red
    exit 1
}
