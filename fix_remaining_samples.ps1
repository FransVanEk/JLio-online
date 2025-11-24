# Final comprehensive fix for remaining sample issues
$ErrorActionPreference = "Continue"

$samplesToCheck = @(
    # Known problematic patterns from auto-generation
    "Client\wwwroot\samples\commands\ifElse\Sample-156.json",
    "Client\wwwroot\samples\commands\ifElse\Sample-157.json",
    "Client\wwwroot\samples\commands\compare\Sample-158.json",
    "Client\wwwroot\samples\commands\compare\Sample-159.json", 
    "Client\wwwroot\samples\commands\compare\Sample-160.json",
    "Client\wwwroot\samples\commands\compare\Sample-161.json",
    "Client\wwwroot\samples\commands\compare\Sample-162.json",
    "Client\wwwroot\samples\functions\scriptPath\Sample-163.json",
    "Client\wwwroot\samples\functions\scriptPath\Sample-164.json",
    "Client\wwwroot\samples\functions\scriptPath\Sample-165.json",
    "Client\wwwroot\samples\functions\scriptPath\Sample-166.json",
    "Client\wwwroot\samples\functions\scriptPath\Sample-167.json"
)

$fixedCount = 0

foreach ($samplePath in $samplesToCheck) {
    if (-not (Test-Path $samplePath)) {
        Write-Host "File not found: $samplePath" -ForegroundColor Yellow
        continue
    }
    
    try {
        $content = Get-Content $samplePath -Raw | ConvertFrom-Json
        $modified = $false
        
        # Check if objects have single-letter names
        $hasShortNames = $false
        $objectNames = @()
        
        if ($content.model.inputObjects) {
            foreach ($obj in $content.model.inputObjects) {
                $objectNames += $obj.name
                if ($obj.name.Length -eq 1) {
                    $hasShortNames = $true
                }
            }
        }
        
        if ($content.model.outputObjects) {
            foreach ($obj in $content.model.outputObjects) {
                if ($obj.name -notin $objectNames) {
                    $objectNames += $obj.name
                }
                if ($obj.name.Length -eq 1) {
                    $hasShortNames = $true
                }
            }
        }
        
        if ($hasShortNames) {
            Write-Host "Fixing: $($content.model.name) - $($content.title)" -ForegroundColor Cyan
            Write-Host "  Current names: $($objectNames -join ', ')" -ForegroundColor Gray
            
            # Change all object names to 'data'
            if ($content.model.inputObjects) {
                foreach ($inputObj in $content.model.inputObjects) {
                    $inputObj.name = "data"
                    $modified = $true
                }
            }
            
            if ($content.model.outputObjects) {
                foreach ($outputObj in $content.model.outputObjects) {
                    $outputObj.name = "data" 
                    $modified = $true
                }
            }
        }
        
        if ($modified) {
            $content | ConvertTo-Json -Depth 20 | Set-Content $samplePath -Encoding UTF8
            Write-Host "  ? Fixed and renamed objects to 'data'" -ForegroundColor Green
            $fixedCount++
        } else {
            Write-Host "OK: $($content.model.name)" -ForegroundColor Green
        }
        
    } catch {
        Write-Host "Error processing $samplePath`: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Fixed $fixedCount additional samples" -ForegroundColor Green