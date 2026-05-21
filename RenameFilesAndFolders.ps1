<#
.SYNOPSIS
Renames parts of folder and file names recursively.

.DESCRIPTION
This script traverses a directory tree, replacing all occurrences of an old string
with a new string in the names of both files and folders.

.PARAMETER Path
The starting folder path for the recursive renaming operation.
This parameter is mandatory.

.PARAMETER OldString
The string to be replaced in the names of files and folders.
This parameter is mandatory.

.PARAMETER NewString
The string to replace the OldString.

.PARAMETER WhatIf
A standard PowerShell parameter that provides a safety feature.
If specified, the script only displays the renames that would occur,
but does not actually perform any changes. This fulfills the 'test parameter' requirement.

.EXAMPLE
# Dry-run (Test Mode): Displays what would be renamed without making changes.
.\Rename-Items-Recursive.ps1 -Path 'C:\Projects\Alpha' -OldString 'v1' -NewString 'final' -WhatIf

.EXAMPLE
# Live Run: Renames all files and folders under C:\Projects\Alpha
# replacing 'v1' with 'final'.
.\Rename-Items-Recursive.ps1 -Path 'C:\Projects\Alpha' -OldString 'v1' -NewString 'final'

.EXAMPLE
# Display Help
.\Rename-Items-Recursive.ps1 -Help

.NOTES
The script renames files first, then folders in descending path length order.
This prevents 'path not found' errors that occur if a parent folder is renamed
before its contents.
#>
[CmdletBinding(SupportsShouldProcess=$true, DefaultParameterSetName='Rename')]
param(
    [Parameter(Mandatory=$true, Position=0, ParameterSetName='Rename')]
    [ValidateScript({Test-Path -Path $_ -PathType Container})]
    [string]$Path,

    [Parameter(Mandatory=$true, Position=1, ParameterSetName='Rename')]
    [string]$OldString,

    [Parameter(Mandatory=$true, Position=2, ParameterSetName='Rename')]
    [string]$NewString,
    
    [Parameter(ParameterSetName='Help')]
    [switch]$Help
)

function Show-Help {
    Get-Help -Name $PSCommandPath -Full
    exit 0
}

if ($Help) {
    Show-Help
}

# --- Function to handle the actual rename logic ---
function Rename-Item-Core {
    param(
        [Parameter(Mandatory=$true)]
        [System.IO.FileSystemInfo]$Item
    )
    
    # Use -replace for non-case-sensitive string replacement
    $NewName = $Item.Name -replace [regex]::Escape($OldString), $NewString

    # Only attempt rename if the name has actually changed
    if ($Item.Name -ne $NewName) {
        if ($PSCmdlet.ShouldProcess("'$($Item.FullName)'", "Rename to '$($NewName)'")) {
            try {
                # Rename-Item implicitly uses LiteralPath when piped, which is safer.
                Rename-Item -Path $Item.FullName -NewName $NewName -ErrorAction Stop
                Write-Host "RENAMED: $($Item.FullName) -> $($Item.Parent.FullName)\$NewName" -ForegroundColor Green
            }
            catch {
                Write-Error "Failed to rename $($Item.FullName): $($_.Exception.Message)"
            }
        }
    }
}

# --- Main Script Logic ---
Write-Host "Starting recursive rename operation in: '$Path'"
Write-Host "Replacing '$OldString' with '$NewString'"
if ($PSCmdlet.ShouldProcess -eq $false) {
    Write-Host "--- DRY RUN MODE: No actual changes will be made. Use -WhatIf for a detailed dry run. ---" -ForegroundColor Yellow
}

try {
    # 1. Rename files first
    Write-Host "`n-- Processing Files --" -ForegroundColor Cyan
    Get-ChildItem -Path $Path -Recurse -File -ErrorAction Stop | ForEach-Object {
        Rename-Item-Core -Item $_
    }
    
    # 2. Rename folders, sorted by FullName length descending.
    # This ensures child folders are renamed before parent folders,
    # preventing path-not-found errors.
    Write-Host "`n-- Processing Folders --" -ForegroundColor Cyan
    Get-ChildItem -Path $Path -Recurse -Directory -ErrorAction Stop | 
        Sort-Object -Property FullName -Descending | 
        ForEach-Object {
            Rename-Item-Core -Item $_
        }

    Write-Host "`nOperation complete." -ForegroundColor Green

}
catch {
    Write-Error "A terminating error occurred: $($_.Exception.Message)"
}