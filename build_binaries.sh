#!/bin/bash
# Build binaries

# Client
cd UdpChat.Client || exit
# MacOS arm 64
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
# Windows 64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
# Linux
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true

# Server
cd ../UdpChat.Server || exit
# MacOS arm 64
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
# Windows 64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
# Linux
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true

# Copy binaries to release folder
cd ../
# MacOS arm 64
cp UdpChat.Client/bin/Release/net7.0/osx-arm64/publish/UdpChat.Client release/osx-arm64_UdpChat.Client
cp UdpChat.Server/bin/Release/net7.0/osx-arm64/publish/UdpChat.Server release/osx-arm64_UdpChat.Server

# Windows
cp UdpChat.Client/bin/Release/net7.0/win-x64/publish/UdpChat.Client.exe release/win-x64_UdpChat.Client.exe
cp UdpChat.Server/bin/Release/net7.0/win-x64/publish/UdpChat.Server.exe release/win-x64_UdpChat.Server.exe

# Linux
cp UdpChat.Client/bin/Release/net7.0/linux-x64/publish/UdpChat.Client release/linux-x64_UdpChat.Client
cp UdpChat.Server/bin/Release/net7.0/linux-x64/publish/UdpChat.Server release/linux-x64_UdpChat.Server
