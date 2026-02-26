# Changelog

All notable changes to this project will be documented in this file.

## [2.2.0] - 2026-02-26

### Added
- **Multi-language support**: Portuguese, English, and Spanish.
- **PowerShell Security**: Switched to `System.Management.Automation` with parameterized calls to mitigate Command Injection.
- **Improved Architecture**: Implemented MVVM (Model-View-ViewModel) pattern.
- **Logging**: Added local file logging in `%AppData%\NetScope\app.log`.
- **Atomic Storage**: Implemented atomic writes for reliability.

### Changed
- **Folder Structure**: Cleaned up the project into `Views`, `ViewModels`, `Services`, and `Resources`.
- **Database Location**: Migrated `database.json` to `%AppData%\NetScope` for better compatibility with restricted environments.

### Fixed
- Various UI binding issues.
- Potential race conditions in file access.

## [2.1.0] - 2026-02-25
- Generic DHCP server management.
- Initial PowerShell integration.
