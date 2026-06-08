# ============================================================
#  dump_project.ps1
#  Собирает исходники VS-проекта в один текстовый файл
#  Использование: запустить в папке с .sln или корне проекта
# ============================================================

param(
    [string]$Root      = (Get-Location).Path,
    [string]$Output    = "project_context.txt",
    [int]   $MaxFileSizeKB = 500
)

# --- Что включать ---
$includeExt = @(
    "*.cs", "*.vb",                          # C# / VB.NET
    "*.py",                                  # Python
    "*.js", "*.ts", "*.jsx", "*.tsx",        # JS / TS
    "*.cpp", "*.c", "*.h", "*.hpp",          # C/C++
    "*.java",                                # Java
    "*.go", "*.rs",                          # Go / Rust
    "*.sql",                                 # SQL
    "*.json", "*.yaml", "*.yml",             # Конфиги
    "*.xml", "*.csproj", "*.sln",            # Проектные файлы
    "*.md", "*.txt"                          # Документация
)

# --- Что исключать (папки и паттерны) ---
$excludeDirs = @(
    "bin", "obj", "node_modules", ".git", ".vs",
    "packages", "publish", "dist", "out",
    "TestResults", ".idea", "__pycache__"
)
$excludeFilePatterns = @(
    "*.Designer.cs", "*.g.cs", "*.g.i.cs",  # Автогенерация
    "*.min.js", "*.min.css",                 # Минифицированное
    "*.lock", "package-lock.json",
    $Output                                  # Сам выходной файл
)

# --- Сбор файлов ---
Write-Host "Сканирую проект: $Root" -ForegroundColor Cyan

$allFiles = Get-ChildItem -Path $Root -Recurse -Include $includeExt -File |
    Where-Object {
        $file = $_
        $inExcludedDir = $excludeDirs | Where-Object { $file.FullName -match "[\\/]$_[\\/]" }
        $isExcluded    = $excludeFilePatterns | Where-Object { $file.Name -like $_ }
        $tooBig        = ($file.Length / 1KB) -gt $MaxFileSizeKB
        -not $inExcludedDir -and -not $isExcluded -and -not $tooBig
    } |
    Sort-Object FullName

if ($allFiles.Count -eq 0) {
    Write-Host "Файлы не найдены. Проверь путь: $Root" -ForegroundColor Red
    exit 1
}

# --- Запись в файл ---
$outputPath = Join-Path $Root $Output
$sb = [System.Text.StringBuilder]::new()
$totalLines = 0
$skipped    = 0
$included   = 0

# Заголовок
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
$null = $sb.AppendLine("=" * 60)
$null = $sb.AppendLine("  PROJECT CONTEXT DUMP")
$null = $sb.AppendLine("  Создан: $timestamp")
$null = $sb.AppendLine("  Корень: $Root")
$null = $sb.AppendLine("=" * 60)
$null = $sb.AppendLine()

# Оглавление
$null = $sb.AppendLine("### ОГЛАВЛЕНИЕ ($($allFiles.Count) файлов) ###")
$null = $sb.AppendLine()
foreach ($f in $allFiles) {
    $rel = $f.FullName.Replace($Root, "").TrimStart("\", "/")
    $null = $sb.AppendLine("  $rel")
}
$null = $sb.AppendLine()
$null = $sb.AppendLine("=" * 60)
$null = $sb.AppendLine()

# Содержимое файлов
foreach ($f in $allFiles) {
    $rel = $f.FullName.Replace($Root, "").TrimStart("\", "/")

    try {
        $content = Get-Content $f.FullName -Raw -Encoding UTF8 -ErrorAction Stop

        if ([string]::IsNullOrWhiteSpace($content)) {
            $skipped++
            continue
        }

        $lines = ($content -split "`n").Count
        $totalLines += $lines

        $null = $sb.AppendLine("### FILE: $rel ###")
        $null = $sb.AppendLine("### LINES: $lines | SIZE: $([math]::Round($f.Length/1KB, 1)) KB ###")
        $null = $sb.AppendLine()
        $null = $sb.AppendLine($content)
        $null = $sb.AppendLine()
        $null = $sb.AppendLine("-" * 60)
        $null = $sb.AppendLine()

        $included++
        Write-Host "  + $rel ($lines строк)" -ForegroundColor Green

    } catch {
        Write-Host "  ! Пропущен (ошибка чтения): $rel" -ForegroundColor Yellow
        $skipped++
    }
}

# Итоговая статистика в конце файла
$null = $sb.AppendLine("=" * 60)
$null = $sb.AppendLine("  ИТОГО: $included файлов | $totalLines строк")
if ($skipped -gt 0) {
    $null = $sb.AppendLine("  Пропущено: $skipped файлов (пустые или ошибки чтения)")
}
$null = $sb.AppendLine("=" * 60)

# Сохранение
$sb.ToString() | Set-Content -Path $outputPath -Encoding UTF8

$sizeKB = [math]::Round((Get-Item $outputPath).Length / 1KB)
$sizeMB = [math]::Round((Get-Item $outputPath).Length / 1MB, 2)

Write-Host ""
Write-Host "Готово!" -ForegroundColor Cyan
Write-Host "  Файл:    $outputPath"
Write-Host "  Файлов:  $included"
Write-Host "  Строк:   $totalLines"
Write-Host "  Размер:  $sizeKB KB ($sizeMB MB)"

if ($sizeKB -gt 150000) {
    Write-Host ""
    Write-Host "  Предупреждение: файл большой (>150 MB)." -ForegroundColor Yellow
    Write-Host "  Рекомендуется использовать Claude Code вместо вставки." -ForegroundColor Yellow
}
