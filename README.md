# FileCopier

Desktop application for copying files with multilingual support and theme selection.

## Features

- **File Selection**: Browse and select files to copy
- **Destination Management**: Choose target folder via folder browser
- **Overwrite Protection**: Confirmation dialog if file already exists
- **Multilingual**: Support for English and Russian
- **Dark/Light Theme**: Toggle between light and dark UI themes
- **Status Feedback**: Real-time progress and success/error messages

## Technology

- **.NET**: .NET 9.0 (WPF + Windows Forms)
- **Framework**: WPF (Windows Presentation Foundation)
- **Target**: Windows x64
- **Build**: Self-contained executable

## Building

### Prerequisites
- .NET 9.0 SDK
- Windows 10/11 (x64)

### Build Commands

Debug build:
```bash
dotnet build
```

Release build (single executable):
```bash
dotnet publish -c Release
```

The published executable will be in `bin/Release/net9.0-windows/win-x64/publish/`.

## Usage

1. Run the application
2. Click "Browse..." next to "Select file to copy:" to choose a file
3. Click "Browse..." next to "Select destination folder:" to choose where to copy
4. Click "Copy" to copy the file
5. Use menu options to change language or theme