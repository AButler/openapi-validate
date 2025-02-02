param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

function Write-GitHubOutput([string]$Name, [string]$Value)
{
    Write-Output "$Name=$Value" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding utf8 -Append
}

$versionRegex = '^v?(?<Major>\d+)\.(?<Minor>\d+)\.(?<Patch>\d+)(?<Suffix>-[A-Za-z0-9-]+)?$'

if ($Version -notmatch $versionRegex)
{
    Write-Error "'$Version' is not a valid semantic version"
}

$major = $Matches.Major
$minor = $Matches.Minor
$patch = $Matches.Patch
$suffix = $Matches.Suffix
$suffixStripped = ''
$isPrerelease = 'false'
if (![string]::IsNullOrWhiteSpace($suffix))
{
    $suffixStripped = $suffix.Substring(1)
    $isPrerelease = 'true'
}

Write-GitHubOutput -Name 'version_full' -Value "$major.$minor.$patch$suffix"
Write-GitHubOutput -Name 'version_major' -Value $major
Write-GitHubOutput -Name 'version_minor' -Value $minor
Write-GitHubOutput -Name 'version_patch' -Value $patch
Write-GitHubOutput -Name 'version_suffix' -Value $suffixStripped
Write-GitHubOutput -Name 'version_2' -Value "$major.$minor"
Write-GitHubOutput -Name 'version_3' -Value "$major.$minor.$patch"
Write-GitHubOutput -Name 'is_prerelease' -Value $isPrerelease