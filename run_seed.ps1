$sqlFile = "SeedPhase23And24SampleData.sql"
if (-not (Test-Path $sqlFile)) {
    Write-Host "SQL file not found!"
    exit 1
}

$sqlText = Get-Content $sqlFile -Raw
$servers = @('.', 'localhost', '(localdb)\mssqllocaldb', '.\SQLEXPRESS')
$executed = $false

# Split script by GO lines
$batches = [System.Text.RegularExpressions.Regex]::Split($sqlText, "^\s*GO\s*$", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -or [System.Text.RegularExpressions.RegexOptions]::Multiline)

foreach ($srv in $servers) {
    try {
        $cs = "Server=$srv;Database=ELearningManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;"
        $conn = New-Object System.Data.SqlClient.SqlConnection($cs)
        $conn.Open()

        foreach ($b in $batches) {
            $trimmed = $b.Trim()
            if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $trimmed
            $cmd.ExecuteNonQuery() | Out-Null
        }
        $conn.Close()
        Write-Host "Successfully executed SQL script via ADO.NET on server: $srv"
        $executed = $true
        break
    } catch {
        Write-Host "Failed execution on $srv : $_"
    }
}

if ($executed) {
    Write-Host "DATABASE SEEDING COMPLETE! ✅"
} else {
    Write-Host "Could not connect or execute on SQL Server."
}
