$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "c:\Users\Amine\source\repos\WConsumsAPI\WConsumsAPI" -PassThru -NoNewWindow
Write-Host "Arrancant API... Esperant 10 segons..."
Start-Sleep -Seconds 10

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5293/api/Incidencia/actives/Totes" -Method Get
    Write-Host "✅ RESPOSTA API:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "❌ ERROR AL CONNECTAR: $_" -ForegroundColor Red
} finally {
    Stop-Process -Id $process.Id -Force
    Write-Host "Servidor aturat."
}
