<#
.SYNOPSIS
Roda o worker do Brasil Compete com o SDK do .NET 10.

.DESCRIPTION
Procura um dotnet com o SDK 10: primeiro o do PATH, depois a instalação por usuário
(%LOCALAPPDATA%\Microsoft\dotnet) e a instalação padrão. Repassa os argumentos ao worker.

.EXAMPLE
.\backend\scripts\worker.ps1 collect --from 2026-09-01 --to 2026-09-30

.EXAMPLE
.\backend\scripts\worker.ps1 test
Roda os testes do backend.
#>
$ErrorActionPreference = 'Stop'

$backend = Split-Path -Parent $PSScriptRoot
$project = Join-Path $backend 'src\BrasilCompete.Worker'
$solution = Join-Path $backend 'BrasilCompete.slnx'

function Find-Dotnet10 {
    $candidates = @(
        (Get-Command dotnet -ErrorAction SilentlyContinue).Source,
        (Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet\dotnet.exe'),
        (Join-Path $env:ProgramFiles 'dotnet\dotnet.exe')
    ) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

    foreach ($candidate in $candidates) {
        if (& $candidate --list-sdks 2>$null | Where-Object { $_ -match '^10\.' }) {
            return $candidate
        }
    }

    throw 'Não encontrei o SDK do .NET 10. Veja a seção "Requisitos" de backend/README.md.'
}

$dotnet = Find-Dotnet10
$env:DOTNET_NOLOGO = '1'

Push-Location $backend
try {
    if ($args.Count -gt 0 -and $args[0] -eq 'test') {
        & $dotnet test $solution
    }
    else {
        & $dotnet run --project $project -- @args
    }

    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
