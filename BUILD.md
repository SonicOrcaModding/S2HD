# Building S2HD

The solution targets **.NET 8** (`net8.0`). Use the .NET 8 SDK or newer.

## Windows

Use Visual Studio

## Linux

Use VSCode or...:

```bash
dotnet restore S2HD.sln
dotnet build S2HD.sln -c Debug -p:Platform=x64
dotnet publish S2HD/S2HD.csproj -c Release -r linux-x64 --self-contained true -p:Platform=x64
```