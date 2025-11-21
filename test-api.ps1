# Test de l'API Loutaupia V2

Write-Host "`n════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  🧪 TEST DE L'API LOUTAUPIA V2" -ForegroundColor Cyan  
Write-Host "════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "1️⃣  Compilation du projet..." -ForegroundColor Yellow
$buildResult = dotnet build --nologo --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Compilation réussie!`n" -ForegroundColor Green
} else {
    Write-Host "❌ Échec de la compilation`n" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}

Write-Host "2️⃣  Démarrage de l'API..." -ForegroundColor Yellow
Write-Host "    (Cela peut prendre quelques secondes)`n" -ForegroundColor Gray

# Démarrer l'API en arrière-plan
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build" -WorkingDirectory $PSScriptRoot -PassThru -WindowStyle Hidden

Start-Sleep -Seconds 10

Write-Host "3️⃣  Test de l'endpoint..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000" -Method Get -TimeoutSec 5
    
    Write-Host "✅ L'API FONCTIONNE!`n" -ForegroundColor Green
    
    Write-Host "📋 Réponse de l'API:" -ForegroundColor Cyan
    Write-Host "   Service: $($response.service)" -ForegroundColor White
    Write-Host "   Version: $($response.version)" -ForegroundColor White  
    Write-Host "   Status: $($response.status)" -ForegroundColor White
    
    Write-Host "`n🌐 Accès à l'API:" -ForegroundColor Cyan
    Write-Host "   API: http://localhost:5000" -ForegroundColor White
    Write-Host "   Swagger: http://localhost:5000/swagger`n" -ForegroundColor White
    
    Write-Host "💡 Pour arrêter l'API:" -ForegroundColor Yellow
    Write-Host "   Stop-Process -Id $($apiProcess.Id)`n" -ForegroundColor Gray
    
} catch {
    Write-Host "❌ L'API ne répond pas`n" -ForegroundColor Red
    Write-Host "Erreur: $($_.Exception.Message)" -ForegroundColor Gray
    
    # Arrêter le processus en cas d'échec
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host "════════════════════════════════════════`n" -ForegroundColor Cyan

