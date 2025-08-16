Set-Location "C:\code\tsp"; `
git status; `
git remote show origin; `
git clean -dfx; `
tree /F; `
Write-Host "`n=== SLN FILE ===" -ForegroundColor Cyan; `
Get-Content *.sln; `
Write-Host "`n=== DOCKERFILE ===" -ForegroundColor Cyan; `
Get-Content Dockerfile; `
Write-Host "`n=== GITHUB ACTIONS ===" -ForegroundColor Cyan; `
Get-Content .github\workflows\*.yml; `
Write-Host "`n=== CSPROJ FILES ===" -ForegroundColor Cyan; `
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { `
    Write-Host "`n--- $($_.FullName) ---" -ForegroundColor Yellow; `
    Get-Content $_.FullName `
}; `
Write-Host "`n=== CS FILES ===" -ForegroundColor Cyan; `
Get-ChildItem -Recurse -Filter *.cs | ForEach-Object { `
    Write-Host "`n--- $($_.FullName) ---" -ForegroundColor Yellow; `
    Get-Content $_.FullName `
}