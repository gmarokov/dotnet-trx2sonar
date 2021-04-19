# dotnet-trx2sonar

[![Build](https://github.com/gmarokov/dotnet-trx2sonar/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/gmarokov/dotnet-trx2sonar/actions/workflows/dotnet-core.yml)
[![.NET Core](https://img.shields.io/badge/.NET%20Core-%3E%3D%202.1-512bd4)](https://dotnet.microsoft.com/download)
[![NuGet](https://img.shields.io/nuget/v/dotnet-trx2sonar.svg)](https://www.nuget.org/packages/dotnet-trx2sonar/)

Dotnet tool to convert Visual Studio TRX files to SonarCloud Generic Test Data - Generic Execution. 
More info about Generic Test Data [here](https://docs.sonarqube.org/latest/analysis/generic-test/)

## Installation 
Download and install the .NET Core 2.1, 3.1 or 5 SDK. Once installed, run the following command:
```dotnet tool install --global dotnet-trx2sonar```

If you already have a previous version of dotnet-trx2sonar installed, you can upgrade to the latest version using the following command:
```dotnet tool update --global dotnet-trx2sonar``

## Usage
Once the tool is installed, provide solution directory to scan for `.trx` files and output file.
```dotnet-trx2sonar -d ./your-solution-directory -o ./your-solution-directory/SonarTestResults.xml```

### Parameters
- `--help` `-h` `-?`
  Show the current help.
- `-d` [string]
  Solution directory to parse (recursive).
- `-o` [string]  
  Output filename.
- `-a` `--absolute`  
  Use absolute path for the test file path.

## Development
Launch in Debug by providing values to the required parameters in `.vscode/launch.json` settings.
```
"args": [  
    "-d", "C:/your-solution/",
    "-o", "C:/your-solution/SonarTestResults.xml",
]
```

## Contribution
- If you want to contribute to codes, create pull request
- If you find any bugs or error, create an issue

## License
This project is licensed under the MIT License
