# Define the output file path
$outFile = "C:\code\TSP\scripts\PowerShell.txt"

# Wrap all commands in a script block and pipe the collected output to Out-File
& {
    # Set location for the following commands
    Set-Location "C:\code\tsp"

    Get-Location

    # --- Commands ---
    # The output of each command is sent to the pipeline automatically
    git status
    git remote show origin
    git clean -dfx
    tree /F

    # --- File Content Sections ---
    # Replace Write-Host with simple strings. They will be written to the output stream.
    # Note: Color formatting is lost when writing to a plain text file.
    
    "`n=== SLN FILE ==="
    Get-Content *.sln

    "`n=== DOCKERFILE ==="
    Get-Content Dockerfile

    "`n=== Properties FILE ==="
    Get-Content *.props

    "`n=== GITHUB ACTIONS ==="
    Get-Content .github\workflows\*.yml

    "`n=== CSPROJ FILES ==="
    Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object {
        "`n--- $($_.FullName) ---"
        Get-Content $_.FullName
    }

    "`n=== CS FILES ==="
    Get-ChildItem -Recurse -Filter *.cs | ForEach-Object {
        "`n--- $($_.FullName) ---"
        Get-Content $_.FullName
    }

    "`n=== feature FILES ==="
    Get-ChildItem -Recurse -Filter *.feature | ForEach-Object {
        "`n--- $($_.FullName) ---"
        Get-Content $_.FullName
    }

} | Out-File -FilePath $outFile -Encoding utf8

Write-Host "Script output has been saved to $outFile"

git add .
git commit --message "add all files"
git pull --rebase --strategy-option=theirs 
git push origin master