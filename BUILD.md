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

## Android

```bash
dotnet restore S2HD.sln
dotnet build S2HD.csproj -f net10.0-android
dotnet publish S2HD.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk
```

## iOS

Requires macOS with Xcode 14.1+ and the .NET 7 SDK

```bash
dotnet workload install ios
```

iOS device:

```bash
dotnet restore S2HD.csproj -f net7.0-ios -p:BuildingForiOS=true
dotnet build S2HD.csproj -f net7.0-ios -r ios-arm64 -p:BuildingForiOS=true
```