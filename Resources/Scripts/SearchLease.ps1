# NetScope Search Script
# Safe parameters
param(
    [Parameter(Mandatory=$true)]
    [string]$SearchValue,

    [Parameter(Mandatory=$true)]
    [ValidateSet("MAC", "IP", "Hostname", "Description")]
    [string]$SearchType,

    [Parameter(Mandatory=$true)]
    [string[]]$DhcpServers
)

$ErrorActionPreference = "SilentlyContinue"
$found = $false

foreach ($DhcpServer in $DhcpServers) {
    try {
        $scopes = Get-DhcpServerv4Scope -ComputerName $DhcpServer -ErrorAction Stop
        foreach ($scope in $scopes) {
            $leases = @()
            
            switch ($SearchType) {
                "MAC" {
                    $leases = Get-DhcpServerv4Lease -ComputerName $DhcpServer -ScopeId $scope.ScopeId | Where-Object { ($_.ClientId -replace '[:-]', '') -eq ($SearchValue -replace '[:-]', '') }
                }
                "IP" {
                    $leases = Get-DhcpServerv4Lease -ComputerName $DhcpServer -ScopeId $scope.ScopeId | Where-Object { $_.IPAddress.IPAddressToString -eq $SearchValue }
                }
                "Hostname" {
                    $leases = Get-DhcpServerv4Lease -ComputerName $DhcpServer -ScopeId $scope.ScopeId | Where-Object { 
                        $_.HostName -and (
                            $_.HostName -like "*$SearchValue*" -or 
                            $_.HostName -eq $SearchValue -or
                            $_.HostName -like "$SearchValue.*" -or
                            $_.HostName -like "*.$SearchValue"
                        )
                    }
                }
                "Description" {
                    $leases = Get-DhcpServerv4Lease -ComputerName $DhcpServer -ScopeId $scope.ScopeId | Where-Object { $_.Description -like "*$SearchValue*" }
                }
            }

            if ($leases) {
                foreach ($lease in $leases) {
                    $macFormatted = (($lease.ClientId -replace '[^A-Fa-f0-9]', '').ToUpper() -replace '(.{2})(?!$)', '$1-')
                    $scopeIP = $scope.ScopeId.IPAddressToString
                    $leaseIP = $lease.IPAddress.IPAddressToString
                    $hostName = if ($lease.HostName) { $lease.HostName } else { '(vazio)' }
                    $status = $lease.AddressState
                    $description = if ($lease.Description) { $lease.Description } else { '(vazio)' }
                    
                    Write-Host "Servidor DHCP : $DhcpServer"
                    Write-Host "Escopo        : $scopeIP"
                    Write-Host "MAC Address   : $macFormatted"
                    Write-Host "IP Address    : $leaseIP"
                    Write-Host "Host Name     : $hostName"
                    Write-Host "Descrição     : $description"
                    Write-Host "Status        : $status"
                    $found = $true
                }
                if ($SearchType -eq "MAC" -or $SearchType -eq "IP") { break }
            }
        }
        if ($found -and ($SearchType -eq "MAC" -or $SearchType -eq "IP")) { break }
    }
    catch {
        # Log error in PS if needed
    }
}

if (-not $found) {
    Write-Host "Nenhum lease encontrado para $SearchType: $SearchValue"
}
