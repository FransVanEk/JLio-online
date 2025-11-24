# Comprehensive sample validation script
param(
    [string]$SamplesPath = "Client\wwwroot\samples"
)

$ErrorActionPreference = "Continue"

function Test-SampleStructure {
    param($content, $filePath)
    
    $issues = @()
    
    # Check basic structure
    if (-not $content.model) {
        $issues += "Missing 'model' property"
        return $issues
    }
    
    if (-not $content.model.scriptText) {
        $issues += "Missing 'scriptText' property"
        return $issues
    }
    
    # Parse script text
    try {
        $script = $content.model.scriptText | ConvertFrom-Json
    } catch {
        $issues += "Invalid script JSON: $($_.Exception.Message)"
        return $issues
    }
    
    # Extract path references from script
    $pathRefs = @()
    $targetRefs = @()
    
    foreach ($cmd in $script) {
        # Main path references
        if ($cmd.path -and $cmd.path -ne '$' -and $cmd.path -match '\$\.([^.\[\]\s\)]+)') {
            $pathRefs += $matches[1]
        }
        
        # Target path references
        if ($cmd.targetPath -and $cmd.targetPath -ne '$' -and $cmd.targetPath -match '\$\.([^.\[\]\s\)]+)') {
            $targetRefs += $matches[1]
        }
        
        # Compare command specific paths
        if ($cmd.leftPath -and $cmd.leftPath -ne '$' -and $cmd.leftPath -match '\$\.([^.\[\]\s\)]+)') {
            $pathRefs += $matches[1]
        }
        if ($cmd.rightPath -and $cmd.rightPath -ne '$' -and $cmd.rightPath -match '\$\.([^.\[\]\s\)]+)') {
            $pathRefs += $matches[1]
        }
        
        # Nested commands (ifTrue/ifFalse)
        if ($cmd.ifTrue) {
            foreach ($nestedCmd in $cmd.ifTrue) {
                if ($nestedCmd.path -and $nestedCmd.path -ne '$' -and $nestedCmd.path -match '\$\.([^.\[\]\s\)]+)') {
                    $pathRefs += $matches[1]
                }
            }
        }
        if ($cmd.ifFalse) {
            foreach ($nestedCmd in $cmd.ifFalse) {
                if ($nestedCmd.path -and $nestedCmd.path -ne '$' -and $nestedCmd.path -match '\$\.([^.\[\]\s\)]+)') {
                    $pathRefs += $matches[1]
                }
            }
        }
    }
    
    $pathRefs = $pathRefs | Sort-Object -Unique
    $targetRefs = $targetRefs | Sort-Object -Unique
    
    # Get actual properties from input JSON
    $inputProperties = @()
    if ($content.model.inputObjects) {
        foreach ($inputObj in $content.model.inputObjects) {
            try {
                $jsonData = $inputObj.jsonString | ConvertFrom-Json
                if ($jsonData -is [PSCustomObject]) {
                    $inputProperties += $jsonData.PSObject.Properties.Name
                }
            } catch {
                $issues += "Invalid input JSON for object '$($inputObj.name)': $($_.Exception.Message)"
            }
        }
    }
    $inputProperties = $inputProperties | Sort-Object -Unique
    
    # Get actual properties from output JSON  
    $outputProperties = @()
    if ($content.model.outputObjects) {
        foreach ($outputObj in $content.model.outputObjects) {
            try {
                $jsonData = $outputObj.jsonString | ConvertFrom-Json
                if ($jsonData -is [PSCustomObject]) {
                    $outputProperties += $jsonData.PSObject.Properties.Name
                }
            } catch {
                $issues += "Invalid output JSON for object '$($outputObj.name)': $($_.Exception.Message)"
            }
        }
    }
    $outputProperties = $outputProperties | Sort-Object -Unique
    
    # Check if path references exist in input
    $missingInputRefs = @()
    foreach ($ref in $pathRefs) {
        if ($ref -notin $inputProperties) {
            $missingInputRefs += $ref
        }
    }
    
    # Check if target references exist in output
    $missingOutputRefs = @()
    foreach ($ref in $targetRefs) {
        if ($ref -notin $outputProperties) {
            $missingOutputRefs += $ref
        }
    }
    
    if ($missingInputRefs.Count -gt 0) {
        $issues += "Missing input properties: $($missingInputRefs -join ', ')"
    }
    
    if ($missingOutputRefs.Count -gt 0) {
        $issues += "Missing output properties: $($missingOutputRefs -join ', ')"
    }
    
    # Additional structure checks
    if ($content.model.inputObjects -and $content.model.inputObjects.Count -gt 0) {
        foreach ($inputObj in $content.model.inputObjects) {
            if (-not $inputObj.name) {
                $issues += "Input object missing 'name' property"
            }
            if (-not $inputObj.jsonString) {
                $issues += "Input object missing 'jsonString' property"
            }
        }
    }
    
    if ($content.model.outputObjects -and $content.model.outputObjects.Count -gt 0) {
        foreach ($outputObj in $content.model.outputObjects) {
            if (-not $outputObj.name) {
                $issues += "Output object missing 'name' property"
            }
            if (-not $outputObj.jsonString) {
                $issues += "Output object missing 'jsonString' property"
            }
        }
    }
    
    return $issues
}

# Get all sample files
$sampleFiles = Get-ChildItem -Path $SamplesPath -Filter "Sample-*.json" -Recurse | Sort-Object Name

$allResults = @()
$successCount = 0
$failureCount = 0

Write-Host "Validating $($sampleFiles.Count) sample files..." -ForegroundColor Cyan
Write-Host ""

foreach ($file in $sampleFiles) {
    try {
        $content = Get-Content $file.FullName -Raw | ConvertFrom-Json
        $issues = Test-SampleStructure $content $file.FullName
        
        $result = [PSCustomObject]@{
            SampleName = $content.model.name
            File = $file.Name
            Directory = Split-Path $file.DirectoryName -Leaf
            FilePath = $file.FullName
            Success = $issues.Count -eq 0
            Issues = $issues
            Title = $content.title
        }
        
        $allResults += $result
        
        if ($result.Success) {
            Write-Host "? $($result.SampleName)" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "? $($result.SampleName)" -ForegroundColor Red
            foreach ($issue in $issues) {
                Write-Host "  - $issue" -ForegroundColor Yellow
            }
            $failureCount++
        }
        
    } catch {
        Write-Host "? ERROR: $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
        $failureCount++
        
        $allResults += [PSCustomObject]@{
            SampleName = $file.Name
            File = $file.Name
            Directory = Split-Path $file.DirectoryName -Leaf
            FilePath = $file.FullName
            Success = $false
            Issues = @("Parse error: $($_.Exception.Message)")
            Title = "Unknown"
        }
    }
}

Write-Host ""
Write-Host "=== VALIDATION SUMMARY ===" -ForegroundColor Cyan
Write-Host "Total samples: $($sampleFiles.Count)"
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Failed: $failureCount" -ForegroundColor Red

if ($failureCount -gt 0) {
    Write-Host ""
    Write-Host "=== FAILED SAMPLES BY DIRECTORY ===" -ForegroundColor Red
    
    $failedSamples = $allResults | Where-Object { -not $_.Success }
    $groupedFailures = $failedSamples | Group-Object Directory | Sort-Object Name
    
    foreach ($group in $groupedFailures) {
        Write-Host ""
        Write-Host "$($group.Name): $($group.Count) failures" -ForegroundColor Yellow
        foreach ($failure in $group.Group) {
            Write-Host "  ? $($failure.SampleName)" -ForegroundColor White
            foreach ($issue in $failure.Issues) {
                Write-Host "     - $issue" -ForegroundColor Gray
            }
        }
    }
    
    # Export failed samples for analysis
    $failedSamples | Export-Csv "failed_samples_analysis.csv" -NoTypeInformation
    Write-Host ""
    Write-Host "Failed samples exported to: failed_samples_analysis.csv" -ForegroundColor Cyan
}

Write-Host ""