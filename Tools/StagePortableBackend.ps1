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
$configText = $configText.Replace("define('PLATFORM_TYPE', 'nati');", "define('PLATFORM_TYPE', 'nati');`r`ndefine('SONNET_DISABLE_AP_LIMITS', true);")
[System.IO.File]::WriteAllText($configPath, $configText, [System.Text.UTF8Encoding]::new($false))

# Keep AP records, recovery, UI values and ACTPT leads intact.  The portable
# build only bypasses the two gameplay effects: AP-short rejection and AP
# deduction.  These replacements target the copied webapp, never Docker's
# source checkout.
function Replace-Required([string] $path, [string] $before, [string] $after) {
    $text = [System.IO.File]::ReadAllText($path)
    if (-not $text.Contains($before)) {
        throw "Expected portable AP patch target was not found: $path"
    }
    [System.IO.File]::WriteAllText($path, $text.Replace($before, $after), [System.Text.UTF8Encoding]::new($false))
}

function Ensure-LegacySphereCommandValidation([string] $path) {
    $text = [System.IO.File]::ReadAllText($path)
    $statementPattern = '(?m)^[\t ]*\$errorCode = \$this->checkCommand\(\$command\);[\t ]*\r?$'
    if ([regex]::IsMatch($text, $statementPattern)) {
        return
    }

    # The Docker reference worktree can contain this executable statement
    # appended to a preceding // comment.  Restore it only in the staged copy.
    $corruptedPattern = '(?m)^(?<comment>[\t ]*//[^\r\n]*?)[\t ]+\$errorCode = \$this->checkCommand\(\$command\);[\t ]*\r?$'
    $matches = [regex]::Matches($text, $corruptedPattern)
    if ($matches.Count -ne 1) {
        throw "Expected exactly one Legacy SphereCommand validation statement or corrupted equivalent: $path"
    }

    $replacement = '${comment}' + "`r`n        " + '$errorCode = $this->checkCommand($command);'
    $text = [regex]::Replace($text, $corruptedPattern, $replacement, 1)
    if (-not [regex]::IsMatch($text, $statementPattern)) {
        throw "Legacy SphereCommand validation is not executable after staging: $path"
    }
    [System.IO.File]::WriteAllText($path, $text, [System.Text.UTF8Encoding]::new($false))
}

# MariaDB 10.11's default strict mode correctly rejects an INSERT that omits a
# NOT NULL column without a schema default.  Keep the original schema intact
# and provide the legacy registration value in the staged copy only.
$userInfoServicePath = Join-Path $backendRoot 'webapp\webapp\lib\service\User_InfoService.class.php'
Replace-Required $userInfoServicePath "'gold' => self::INITIAL_GOLD,`r`n            'place_id' => Place_MasterService::INITIAL_PLACE," "'gold' => self::INITIAL_GOLD,`r`n            'tutorial_step' => self::TUTORIAL_MORNING,`r`n            'place_id' => Place_MasterService::INITIAL_PLACE,"

$spherePath = Join-Path $backendRoot 'webapp\webapp\lib\sphere\SphereCommon.class.php'
Ensure-LegacySphereCommandValidation $spherePath
Replace-Required $spherePath "if(`$user['action_pt'] < Service::create('Quest_Master')->getConsumePt(`$this->info['quest_id']))" "if(!SONNET_DISABLE_AP_LIMITS && `$user['action_pt'] < Service::create('Quest_Master')->getConsumePt(`$this->info['quest_id']))"
Replace-Required $spherePath "if(`$user['action_pt'] < self::BATTLE_REMAKE_ACTPT) {" "if(!SONNET_DISABLE_AP_LIMITS && `$user['action_pt'] < self::BATTLE_REMAKE_ACTPT) {"
Replace-Required $spherePath "`$userSvc->plusValue(`$this->info['user_id'], array('action_pt'=> -1 * self::BATTLE_REMAKE_ACTPT));" "if(!SONNET_DISABLE_AP_LIMITS) `$userSvc->plusValue(`$this->info['user_id'], array('action_pt'=> -1 * self::BATTLE_REMAKE_ACTPT));"
Replace-Required $spherePath "`$userSvc->plusValue(`$this->info['user_id'], array(`r`n                                'action_pt'=> -1 * Service::create('Quest_Master')->getConsumePt(`$this->info['quest_id']))`r`n                            );" "if(!SONNET_DISABLE_AP_LIMITS) `$userSvc->plusValue(`$this->info['user_id'], array(`r`n                                'action_pt'=> -1 * Service::create('Quest_Master')->getConsumePt(`$this->info['quest_id']))`r`n                            );"

foreach ($relativePath in @('webapp\webapp\modules\Api\actions\QuestDramaAction.class.php', 'webapp\webapp\modules\Swf\actions\QuestDramaAction.class.php')) {
    $path = Join-Path $backendRoot $relativePath
    Replace-Required $path "if(`$this->userInfo['action_pt'] < Service::create('Quest_Master')->getConsumePt(`$_GET['questId'])) {" "if(!SONNET_DISABLE_AP_LIMITS && `$this->userInfo['action_pt'] < Service::create('Quest_Master')->getConsumePt(`$_GET['questId'])) {"
}

$dramaQuestPath = Join-Path $backendRoot 'webapp\webapp\lib\quest\DramaQuest.class.php'
Replace-Required $dramaQuestPath "Service::create('User_Info')->plusValue(`$this->userId, array(`r`n            'action_pt' => -1 * `$this->quest['consume_pt'],`r`n        ));" "if(!SONNET_DISABLE_AP_LIMITS) Service::create('User_Info')->plusValue(`$this->userId, array(`r`n            'action_pt' => -1 * `$this->quest['consume_pt'],`r`n        ));"

Write-Host "Staged local backend: $backendRoot"
