# Introduction 
ProjectIPN is a Instant payment notification platform.
This project implements a micro-service architecture, internally tightly cohesive and externally loosely coupled.

# Getting Started
1. Editor
Use either VS Code or Visual Studio 2019(latest release) to contribute.

2.	Dependencies
 1. Target Framework - .NET 5.0
 2. Library Fx - .NetStandard 2.1
 3. Runtime - .NET Core 5.0.1
 4. SDK - .NET 5 Version 5.0.101
 5. Latest Nuget Packages 

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
ProjectIPN repository has 3 core branches following Git Flow model
  1. main - original up-to-date code base
  2. sandbox - active development and testing code base

Each developer shall clone sandbox and create feature branches which follows the sprints. 
A pull request is expected for code reviews then the principle developer shall merge feature branch to sandbox branch.

**Note:** To avoid conflicts and overwriting work, always fetch sandbox before committing feature branch

# Solution Items
There are 2 files under solution items folder: **ErrorCodes.MD** and **ResetData.MD**
All error codes are enumerated in the ErrorCodes file with a prefix **AN** for this project. 
E.g. *Error Code 1 shall be abbreviated as* ***AN001***

*appsettings.Development.json* is specific to each contributor/developer with configuration matching their private dev box.

Further reads:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [What's new in .NET 5](https://docs.microsoft.com/en-us/dotnet/core/dotnet-five)
- [What's new in C# 9.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9)
- [What's new in EF Core 5.0](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-5.0/whatsnew)