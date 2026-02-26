# NetScope - DHCP Management Tool 🔍

**NetScope** is a professional technical support tool designed to streamline DHCP lease queries across multiple Windows Servers. It integrates security policy verification and remote device wake-up via Wake-on-LAN (WOL) into a single, unified interface.

## 🚀 Key Features

- **Centralized Querying**: Search for DHCP leases across multiple servers simultaneously with asynchronous processing.
- **Configurable & Generic**: No environment-specific dependencies. All settings are dynamically configured via the UI.
- **Dynamic Policy Verification**: Validate if a MAC address is allowed within user-defined DHCP filter policies.
- **Dynamic Management**:
  - Unlimited DHCP servers.
  - Multiple MAC filter policies.
  - Custom names for known scopes.
- **Advanced Filtering**:
  - **MAC Address**: Locate IP and Hostname from a physical address.
  - **IP Address**: Identify the MAC and Hostname associated with an IP.
  - **Hostname**: Partial or exact search by machine name.
  - **Description**: Search within DHCP comment fields.
- **Wake-on-LAN (WOL)**: Send *Magic Packets* to power on computers remotely.

## 📊 Standardized Output

Results are displayed in a clear, technical format:

```text
DHCP QUERY RESULT
DHCP Server   : srv-dhcp-01.example.com
Scope         : 192.168.1.0
MAC Address   : 00-17-C8-A4-F8-4C
IP Address    : 192.168.1.100
Host Name     : WS-EXAMPLE-01
Description   : Finance Dept
Status        : Active
MAC Allowed   : Yes
Allowed Scopes: 192.168.1.0
Unit/Location : Central Office
```

## 🛠️ Technology Stack

- **Framework**: .NET 9.0 (WPF)
- **Language**: C# 13 (Async/Await, System.Text.Json)
- **Architecture**: Layered separation (Configuration, DHCP Service, and UI)
- **Integration**: Remote PowerShell (scripts with JSON return)
- **Persistence**: Structured JSON for portability
- **Installer**: Inno Setup (Professional x64 build)

## 📋 Prerequisites

To function correctly, the execution environment must have:
1. Connectivity to the configured DHCP servers.
2. Proper DHCP read permissions (DHCP Users group or higher).
3. Remote PowerShell (WinRM) enabled on target servers.
4. .NET 9 Desktop Runtime installed.

## 📥 Installation

1. Download the latest release from the [Releases](https://github.com/devopsmarcu/Netscope/releases) page.
2. Run the `NetScope_Setup.exe` installer.
3. After installation, go to `File > Settings` to register your servers and policies.

## 🏗️ Building from Source

To compile **NetScope** manually:

1. Clone the repository: `git clone https://github.com/devopsmarcu/Netscope.git`
2. Open the solution `NetScope.sln` in **Visual Studio 2022**.
3. Ensure you have the **.NET 9 SDK** installed.
4. Restore NuGet packages.
5. Build the solution in `Release` mode.
6. The executable will be available in `bin/Release/net9.0-windows/`.

## ⚙️ Technical Architecture

NetScope follows a decoupled architecture:
- **C# Core**: Manages the UI, models, and data persistence.
- **DhcpService**: Isolates query logic and script orchestration.
- **DatabaseService**: Provides dynamic persistence for servers, policies, and scope mapping.
- **PowerShell Integration**: Communication with Windows Server is handled via PowerShell for native compatibility with Microsoft infrastructure.

## �️ Security Disclaimer

This tool executes PowerShell scripts against remote servers. Users are responsible for ensuring proper security configurations (WinRM, RBAC) and that credentials used have minimal necessary permissions. No credentials or server names are hardcoded; all configuration remains local in the `database.json` file.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
1. Fork the project.
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`).
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4. Push to the branch (`git push origin feature/AmazingFeature`).
5. Open a Pull Request.

## 🔒 Security

NetScope prioritizes security when interacting with infrastructure:
- **Command Sanitization**: All user inputs (IPs, MACs, Hostnames) are validated against strict Regex patterns and sanitized before being processed.
- **Secure Execution**: PowerShell scripts are executed using `System.Management.Automation` with named parameters, preventing Command Injection vulnerabilities.
- **Least Privilege**: The application only requires the permissions necessary to query DHCP servers.

## 🤝 Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

Distributed under the **MIT License**. See `LICENSE` for more information.

## 👤 Credits

- **Author**: Marcus Santos
- **Version**: 2.1 (Open Source)

---
*NetScope v2.1 - 2026*
