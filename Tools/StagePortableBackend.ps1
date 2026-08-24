param(
    [Parameter(Mandatory = $true)] [string] $PhpRoot,
    [Parameter(Mandatory = $true)] [string] $MariaDbRoot,
    [Parameter(Mandatory = $true)] [string] $ServerRoot,
    [Parameter(Mandatory = $true)] [string] $Sonnet1Dump,
    [Parameter(Mandatory = $true)] [string] $SonnetMDump
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$backendRoot = Join-Path $repositoryRoot 'Assets\StreamingAssets\Backend'

foreach ($path in @($PhpRoot, $MariaDbRoot, $ServerRoot, $Sonnet1Dump, $SonnetMDump)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing staging input: $path"
    }
}

if (Test-Path -LiteralPath $backendRoot) {
    throw "Refusing to overwrite an existing staged backend: $backendRoot"
}

New-Item -ItemType Directory -Path $backendRoot | Out-Null
Copy-Item -LiteralPath $PhpRoot -Destination (Join-Path $backendRoot 'php') -Recurse
Copy-Item -LiteralPath $MariaDbRoot -Destination (Join-Path $backendRoot 'mariadb') -Recurse
Copy-Item -LiteralPath $ServerRoot -Destination (Join-Path $backendRoot 'webapp') -Recurse
New-Item -ItemType Directory -Path (Join-Path $backendRoot 'initial_db') | Out-Null
Copy-Item -LiteralPath $Sonnet1Dump -Destination (Join-Path $backendRoot 'initial_db\sonnet_1.sql')
Copy-Item -LiteralPath $SonnetMDump -Destination (Join-Path $backendRoot 'initial_db\sonnet_m.sql')

# Keep Docker's source checkout untouched.  The staged copy accepts the manager's
# loopback DB endpoint and derives its root from the copied backend layout.
$configPath = Join-Path $backendRoot 'webapp\webapp\config.php'
$configText = [System.IO.File]::ReadAllText($configPath)
$configText = $configText.Replace("'hostspec' => 'mariadb',", "'hostspec' => getenv('SONNET_DB_HOST') ?: 'mariadb',`r`n        'port' => getenv('SONNET_DB_PORT') ?: '3306',`r`n        'socket' => '',")
$configText = $configText.Replace("define('MO_BASE_DIR', '/var/www');", "define('MO_BASE_DIR', dirname(__DIR__));")
[System.IO.File]::WriteAllText($configPath, $configText, [System.Text.UTF8Encoding]::new($false))

Write-Host "Staged local backend: $backendRoot"
