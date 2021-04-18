# dotnet-trx2sonar

[![Build](https://github.com/gmarokov/dotnet-trx2sonar/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/gmarokov/dotnet-trx2sonar/actions/workflows/dotnet-core.yml)
[![.NET](https://img.shields.io/badge/.NET%20-%3E%3D%205.0-512bd4)](https://dotnet.microsoft.com/download) 
[![.NET Core](https://img.shields.io/badge/.NET%20Core-%3E%3D%202.1-512bd4)](https://dotnet.microsoft.com/download)

Dotnet tool to convert Visual Studio TRX files to SonarCloud Generic Test Data - Generic Execution. 
More info about Generic Test Data [here](https://docs.sonarqube.org/latest/analysis/generic-test/)

## Installation 
Download and install the .NET Core 2.1, 3.1 or 5 SDK. Once installed, run the following command:

## Usage

- `--help` `-h` `-?`
  Show the current help.
- `-d` [string]
  Solution directory to parse (recursive).
- `-o` [string]  
  Output filename.
- `-a` `--absolute`  
  Use absolute path for Sonarqube file path.

## Development
Use .vscode launch settings to provide parameters.

## Contribution
- If you want to contribute to codes, create pull request
- If you find any bugs or error, create an issue
## License
This project is licensed under the MIT License
