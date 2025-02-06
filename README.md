# AcornUnObfuscate

# Acorn BBC BASIC Detokenizer and Deobfuscator

A Windows application for detokenizing and deobfuscating BBC BASIC source code files from Acorn/RISC OS systems. Features syntax highlighting similar to Visual Studio's style.

![alt text](https://github.com/aidanhutch/AcornUnObfuscate/blob/master/Docs/Screenshot%202025-02-06%20100249.png?raw=true)
<img src="https://github.com/aidanhutch/AcornUnObfuscate/blob/master/Docs/Screenshot%202025-02-06%20100249.png" width="128"/>

## Features

- **Detokenization**: Converts tokenized BBC BASIC files (`.bas`) back into readable text
- **Deobfuscation**: Intelligently renames variables and procedures to make code more readable
- **Syntax Highlighting**: Visual Studio-style syntax highlighting for:
  - BBC BASIC keywords
  - String literals
  - Numbers (including hexadecimal)
  - Comments
  - Procedures and Functions
  - SYS calls
  - Operators

## Installation

1. Download the latest release from the [releases page](https://github.com/aidanhutch/AcornUnObfuscate.git)
2. Extract the ZIP file to your desired location
3. Run `AcornBasicDetokenizer.exe`

## Usage

### Opening Files

1. Click `File -> Open` or press `Ctrl+O`
2. Select a BBC BASIC file (`.bas` extension or All Files to load ,bas file)
3. The detokenized source code will be displayed with syntax highlighting

### Deobfuscating Code

1. Open a BBC BASIC file
2. Click `Deobfuscate -> Deobfuscate` to process the code
3. The deobfuscated version will appear with:
   - Meaningful variable names
   - Restructured procedure names
   - Preserved functionality
4. Use `Deobfuscate -> Revert Changes` to return to the original code

## Supported File Formats

- BBC BASIC V tokenized files (`.bas`)
- Files from RISC OS and earlier Acorn systems

## Technical Details

### BBC BASIC File Format

The application handles the BBC BASIC V file format which includes:
- Line-based structure with line numbers
- Tokenized keywords
- Special handling for PROC/FN definitions
- SYS call formatting
- REM statement preservation

### Deobfuscation Rules

Variables are renamed based on their:
- Usage context
- Data type (%, $, or no suffix)
- Scope (LOCAL or global)
- Relationship to procedures

Procedures are renamed based on their:
- Functionality
- System call patterns
- Usage context

## Building from Source

### Prerequisites

- Visual Studio 2022
- .NET 8

### Build Steps

1. Clone the repository:
```bash
git clone https://github.com/aidanhutch/AcornUnObfuscate.git
```

2. Open `AcornBasicDetokenizer.sln` in Visual Studio

3. Build the solution:
```bash
dotnet build
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Thanks to the Acorn/RISC OS community for documenting the BBC BASIC file format
- Built on research and documentation about the BBC BASIC tokenization system
- Inspired by the need for modern tools to work with classic Acorn software

## Contact

Your Name - [@aidanhutch](https://twitter.com/aidanhutch)

## Release History

* 1.0.0
    * Initial release
    * Basic detokenization and syntax highlighting
* 1.1.0
    * Added deobfuscation feature
    * Improved syntax highlighting
* 1.2.0
    * Enhanced Visual Studio-style theme
    * Bug fixes for keyword highlighting
