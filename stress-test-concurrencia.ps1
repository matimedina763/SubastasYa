# Stress test de concurrencia: envía 2 pujas IDÉNTICAS al mismo tiempo
# sobre la misma subasta, para verificar que el optimistic locking
# (campo Version) rechaza una de las dos con 409 Conflict.

$subastaId = 6          # <-- reemplazar por el id de una subasta ACTIVA real
$compradorId = 2        # <-- comprador con fondos suficientes
$monto = 60000          # <-- un monto válido para esa subasta

$url = "https://localhost:7006/api/Subastas/$subastaId/pujas"
$body = @{ compradorId = $compradorId; monto = $monto } | ConvertTo-Json

Write-Host "Enviando 2 pujas simultáneas a $url ..." -ForegroundColor Cyan

$job1 = Start-Job -ScriptBlock {
    param($url, $body)
    try {
        $r = Invoke-WebRequest -Uri $url -Method POST -Body $body -ContentType "application/json" -SkipCertificateCheck
        return "Puja 1 -> $($r.StatusCode)"
    } catch {
        return "Puja 1 -> $($_.Exception.Response.StatusCode.value__)"
    }
} -ArgumentList $url, $body

$job2 = Start-Job -ScriptBlock {
    param($url, $body)
    try {
        $r = Invoke-WebRequest -Uri $url -Method POST -Body $body -ContentType "application/json" -SkipCertificateCheck
        return "Puja 2 -> $($r.StatusCode)"
    } catch {
        return "Puja 2 -> $($_.Exception.Response.StatusCode.value__)"
    }
} -ArgumentList $url, $body

Wait-Job $job1, $job2 | Out-Null
Receive-Job $job1
Receive-Job $job2
Remove-Job $job1, $job2

Write-Host "`nResultado esperado: una puja con 201 (o 200) y la otra con 409 Conflict." -ForegroundColor Yellow