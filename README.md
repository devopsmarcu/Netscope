# NetScope - DHCP Management Tool 🔍

**NetScope** é uma ferramenta de suporte técnico avançada desenvolvida para facilitar a consulta de leases DHCP em múltiplos servidores Windows Server, integrar verificação de políticas de segurança e permitir o acionamento remoto de dispositivos via Wake-on-LAN.

![NetScope Icon](ico-f.ico)

## 🚀 Funcionalidades Principais

- **Consulta Centralizada**: Busca leases em diversos servidores DHCP simultaneamente com processamento assíncrono.
- **Configurável e Genérico**: Sem dependências de ambiente específicas. Tudo é configurado dinamicamente via interface.
- **Verificação de Política Dinâmica**: Valida se o MAC Address está liberado em políticas de filtro definidas pelo usuário.
- **Cadastro Dinâmico**:
  - Servidores DHCP ilimitados.
  - Múltiplas políticas de filtro de MAC.
  - Cadastro de nomes para escopos conhecidos.
- **Múltiplos Filtros**:
  - **MAC Address**: Localize o IP e Hostname a partir do endereço físico.
  - **IP Address**: Identifique o MAC e Hostname associados ao IP.
  - **Hostname**: Busca parcial ou exata por nome de máquina.
  - **Descrição**: Pesquisa em campos de comentários do DHCP.
- **Wake-on-LAN (WOL)**: Envio de *Magic Packets* para ligar computadores remotamente.

## 📊 Formato de Saída Padronizado

Os resultados são exibidos de forma clara e alinhada para facilitar a leitura técnica:

```text
RESULTADO DA CONSULTA DHCP
Servidor DHCP : srv-dhcp-01.empresa.local
Escopo        : 10.0.10.0
MAC Address   : 00-C0-EE-D8-35-E9
IP Address    : 10.0.10.104
Host Name     : workstation-01
Descrição     : Setor Financeiro
Status        : Active
MAC Liberado  : Sim
Escopos Lib.  : 10.0.10.0
Unidade/Local : Escritório Central
```

## 🛠️ Tecnologias Utilizadas

- **Framework**: .NET 9.0 (WPF)
- **Linguagem**: C# 13 (Async/Await, System.Text.Json)
- **Arquitetura**: Separação em camadas (Configuração, Serviço DHCP e Interface)
- **Integração**: PowerShell Remoto (scripts `.ps1` com retorno JSON)
- **Banco de Dados**: Persistência estruturada em JSON para portabilidade
- **Instalador**: Inno Setup (Compilação profissional x64)

## 📋 Pré-requisitos

Para o pleno funcionamento, a máquina de execução deve:
1. Ter conectividade com os servidores DHCP configurados.
2. Possuir permissões de leitura no DHCP (Grupo "DHCP Users" ou superior).
3. Ter o PowerShell Remoto (WinRM) habilitado nos servidores.
4. Possuir o .NET 9 Desktop Runtime instalado.

## 📥 Instalação

1. Execute o instalador `Setup_NetScope_v2.1.exe`.
2. Após a instalação, acesse `Arquivo > Configurações` para cadastrar seus servidores e políticas.

## ⚙️ Arquitetura Técnica

O NetScope utiliza uma arquitetura desacoplada:
- **C#** gerencia a interface, os modelos e a persistência de dados.
- **DhcpService** isola a lógica de consulta e orquestração de scripts.
- **DatabaseService** provê persistência dinâmica para servidores, políticas e nomes de escopos.
- A comunicação com o Windows Server é feita via **PowerShell**, garantindo compatibilidade nativa com a infraestrutura Microsoft.

## 👤 Créditos

<<<<<<< HEAD
- **Desenvolvido por**: Marcus Santos 💻
- **Versão**: 2.1 (Open Source)
=======
- **Marcus Santos** 💻
- Setor de Operaçôes de TI - Santa Casa da Bahia
>>>>>>> b8df3cc9f411e52bb6ba91b80ee028f3ce3bc92c

---
*NetScope v2.1 - 2026*
