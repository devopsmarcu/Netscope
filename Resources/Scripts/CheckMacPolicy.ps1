#Requires -Modules DhcpServer

param(
  [Parameter(Mandatory=$true)]
  [string]$MacAddress,

  [Parameter(Mandatory=$true)]
  [string]$DhcpServer,
  
  [Parameter(Mandatory=$true)]
  [string]$PolicyName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Normalize-Mac {
  param([Parameter(Mandatory=$true)][string]$Mac)
  $m = ($Mac -replace '[^0-9A-Fa-f]', '').ToUpper()
  if ($m.Length -ne 12) { throw "MAC inválido." }
  return ($m -split '(.{2})' | Where-Object { $_ -and $_.Length -eq 2 }) -join '-'
}

function Extract-MacsFromPolicyObject {
  param([Parameter(Mandatory=$true)]$PolicyObject)
  
  # A propriedade que contém os valores da condição (MACs) chama-se 'Condition'
  # e dentro dela existe um array de 'Value' se o tipo for MAC.
  # No entanto, a forma mais segura de extrair de um objeto de política é via strings 
  # ou navegando na estrutura se soubermos exatamente onde está.
  # O script original usava regex no JSON, vamos manter essa lógica que é resiliente.
  $text = ($PolicyObject | ConvertTo-Json -Depth 10 -Compress)
  $raw = [regex]::Matches($text, '(?i)(?:[0-9A-F]{2}[:-]?){5}[0-9A-F]{2}')
  $macs = New-Object System.Collections.Generic.HashSet[string]

  foreach ($m in $raw) {
    try {
        $n = Normalize-Mac $m.Value
        [void]$macs.Add($n)
    } catch {}
  }

  return $macs
}

try {
    $targetMac = Normalize-Mac $MacAddress
    $found = $false
    $foundScopes = @()

    $scopes = Get-DhcpServerv4Scope -ComputerName $DhcpServer -ErrorAction SilentlyContinue

    if ($scopes) {
        foreach ($s in $scopes) {
          try {
            $policies = Get-DhcpServerv4Policy -ComputerName $DhcpServer -ScopeId $s.ScopeId -ErrorAction SilentlyContinue | Where-Object { $_.Name -eq $PolicyName }
            foreach ($policy in $policies) {
              $macs = Extract-MacsFromPolicyObject $policy
              if ($macs.Contains($targetMac)) {
                $found = $true
                $foundScopes += $s.ScopeId.IPAddressToString
              }
            }
          } catch {}
        }
    }

    $result = @{
        mac = $targetMac
        isAllowed = $found
        scopes = $foundScopes
    }

    $result | ConvertTo-Json -Compress
} catch {
    $result = @{
        mac = $MacAddress
        isAllowed = $false
        scopes = @()
        error = $_.Exception.Message
    }
    $result | ConvertTo-Json -Compress
}
