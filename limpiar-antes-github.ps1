# Script de limpieza antes de subir a GitHub
# Ejecutar en PowerShell desde la raíz del proyecto

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  LIMPIEZA PRE-GITHUB - Inmobiliaria App" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en la raíz del proyecto
if (-not (Test-Path ".\LandingBack") -or -not (Test-Path ".\frontend")) {
    Write-Host "ERROR: Ejecuta este script desde la raíz del proyecto" -ForegroundColor Red
    exit 1
}

Write-Host "Tamaños ANTES de limpiar:" -ForegroundColor Yellow
Write-Host ""

# Calcular tamaños actuales
$sizeLogs = if (Test-Path ".\LandingBack\logs") { (Get-ChildItem ".\LandingBack\logs" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB } else { 0 }
$sizeNodeModules = if (Test-Path ".\frontend\node_modules") { (Get-ChildItem ".\frontend\node_modules" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB } else { 0 }
$sizeBin = if (Test-Path ".\LandingBack\bin") { (Get-ChildItem ".\LandingBack\bin" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB } else { 0 }
$sizeObj = if (Test-Path ".\LandingBack\obj") { (Get-ChildItem ".\LandingBack\obj" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB } else { 0 }
$sizeNext = if (Test-Path ".\frontend\.next") { (Get-ChildItem ".\frontend\.next" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB } else { 0 }
$sizeUploads = if (Test-Path ".\LandingBack\wwwroot\uploads\properties") { (Get-ChildItem ".\LandingBack\wwwroot\uploads\properties" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB } else { 0 }

Write-Host "  - LandingBack/logs: $([math]::Round($sizeLogs, 2)) MB" -ForegroundColor White
Write-Host "  - frontend/node_modules: $([math]::Round($sizeNodeModules, 2)) MB" -ForegroundColor White
Write-Host "  - LandingBack/bin: $([math]::Round($sizeBin, 2)) MB" -ForegroundColor White
Write-Host "  - LandingBack/obj: $([math]::Round($sizeObj, 2)) MB" -ForegroundColor White
Write-Host "  - frontend/.next: $([math]::Round($sizeNext, 2)) MB" -ForegroundColor White
Write-Host "  - wwwroot/uploads/properties: $([math]::Round($sizeUploads, 2)) MB" -ForegroundColor White
Write-Host ""

$totalBefore = $sizeLogs + $sizeNodeModules + $sizeBin + $sizeObj + $sizeNext + $sizeUploads
Write-Host "TOTAL A ELIMINAR: $([math]::Round($totalBefore, 2)) MB" -ForegroundColor Yellow
Write-Host ""

# Pedir confirmación
$confirm = Read-Host "¿Deseas continuar? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "Operación cancelada." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Eliminando archivos..." -ForegroundColor Green
Write-Host ""

# Función para eliminar con verificación
function Remove-SafeDirectory {
    param($path, $name)
    if (Test-Path $path) {
        Write-Host "  [X] Eliminando $name..." -ForegroundColor Cyan
        Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "      ✓ $name eliminado" -ForegroundColor Green
    } else {
        Write-Host "  [√] $name no existe (ya limpio)" -ForegroundColor Gray
    }
}

# Eliminar carpetas de Backend
Remove-SafeDirectory ".\LandingBack\logs" "logs de desarrollo"
Remove-SafeDirectory ".\LandingBack\bin" "archivos compilados (bin)"
Remove-SafeDirectory ".\LandingBack\obj" "archivos temporales (obj)"
Remove-SafeDirectory ".\LandingBack\.vs" "configuración de Visual Studio"
Remove-SafeDirectory ".\LandingBack\wwwroot\uploads\properties" "imágenes de prueba"

# Eliminar carpetas de Frontend
Remove-SafeDirectory ".\frontend\node_modules" "dependencias de Node"
Remove-SafeDirectory ".\frontend\.next" "build de Next.js"

# Eliminar archivos específicos
if (Test-Path ".\frontend\tsconfig.tsbuildinfo") {
    Write-Host "  [X] Eliminando cache de TypeScript..." -ForegroundColor Cyan
    Remove-Item ".\frontend\tsconfig.tsbuildinfo" -Force
    Write-Host "      ✓ Cache eliminado" -ForegroundColor Green
}

# Recrear carpeta uploads vacía (para que esté en el repo)
Write-Host ""
Write-Host "  [+] Recreando estructura de uploads (vacía)..." -ForegroundColor Cyan
New-Item -Path ".\LandingBack\wwwroot\uploads\properties" -ItemType Directory -Force | Out-Null
Write-Host "      ✓ Carpeta recreada" -ForegroundColor Green

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  ✓ LIMPIEZA COMPLETADA" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Espacio liberado: $([math]::Round($totalBefore, 2)) MB" -ForegroundColor Yellow
Write-Host ""
Write-Host "PRÓXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "1. Verificar archivos: git status" -ForegroundColor White
Write-Host "2. Agregar cambios: git add ." -ForegroundColor White
Write-Host "3. Hacer commit: git commit -m 'Initial commit: Inmobiliaria Full Stack'" -ForegroundColor White
Write-Host "4. Crear repo en GitHub (web)" -ForegroundColor White
Write-Host "5. Conectar y push: git remote add origin URL && git push -u origin main" -ForegroundColor White
Write-Host ""
Write-Host "NOTA: Después del push, reinstalar dependencias:" -ForegroundColor Yellow
Write-Host "  - Backend: cd LandingBack && dotnet restore" -ForegroundColor Gray
Write-Host "  - Frontend: cd frontend && npm install" -ForegroundColor Gray
Write-Host ""
