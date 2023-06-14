
# Downloads

There is no installer. There are 2 ways to get the client CLI: download the binary executable or download the dotnet solution and run with the dotnet runtime.

## Table of contents

- [Downloads](#downloads)
  - [Table of contents](#table-of-contents)
  - [Binary executable](#binary-executable)
    - [Latest release download](#latest-release-download)
    - [Github Releases page](#github-releases-page)
      - [Add to PATH](#add-to-path)
        - [MacOS and Linux](#macos-and-linux)
        - [Windows CMD or Powershell](#windows-cmd-or-powershell)
  - [Dotnet solution (.NET source code)](#dotnet-solution-net-source-code)

## Binary executable

You can download a binary from the releases page and run it directly. There are binaries for Windowsx64, Linux64 and MacOSarm64.

### Latest release download

Client:

- [Linux](https://github.com/nicocossiom/IngenieriaProtocolos/releases/download/latest/linux-x64_UdpChat.Client)
- [MacOS](https://github.com/nicocossiom/IngenieriaProtocolos/releases/download/latest/osx-arm64_UdpChat.Client)
- [Windows](https://github.com/nicocossiom/IngenieriaProtocolos/releases/download/latest/win-x64_UdpChat.Client.exe)

Server:

- [Linux](https://github.com/nicocossiom/IngenieriaProtocolos/releases/download/latest/linux-x64_UdpChat.Server)
- [MacOS](https://github.com/nicocossiom/IngenieriaProtocolos/releases/download/latest/osx-arm64_UdpChat.Server)
- [Windows](https://github.com/nicocossiom/IngenieriaProtocolos/releases/download/latest/win-x64_UdpChat.Server.exe)

### Github Releases page

You can laso go to the [releases page on Github](https://github.com/nicocossiom/IngenieriaProtocolos/releases/) to see all available releases and platforms.

![Github Releass webpage](../images/releases.png)

After downloading the client move the file to a folder of your choice and run the executable.

#### Add to PATH

##### MacOS and Linux

```bash
export PATH=$PATH:/path/to/folder
```

##### Windows CMD or Powershell

Windows CMD in Admin mode

```cmd
set PATH=%PATH%;C:\path\to\folder
```

Windows Powershell in Admin mode

```powershell
$env:Path += ";C:\path\to\folder"
```

In Windows you can run the executable from the command line. Or from double clicking it.

## Dotnet solution (.NET source code)

```bash
git clone git@github.com:nicocossiom/IngenieriaProtocolos.git UDPChat
Cloning into 'UDPChat'...
remote: Enumerating objects: 584, done.
remote: Counting objects: 100% (584/584), done.
remote: Compressing objects: 100% (291/291), done.
remote: Total 584 (delta 340), reused 514 (delta 270), pack-reused 0
Receiving objects: 100% (584/584), 6.78 MiB | 1.57 MiB/s, done.
Resolving deltas: 100% (340/340), done.
```

````bash
cd UDPChat
````

```bash
dotnet run --project UdpChat.Client
Welcome to the UDP chat client!

        Default settings:
        Client ports: Recieve 4000 - Send 4001
        Central Server: Adress 127.0.0.1 - Port 5000

Do you want to use the default settings? (y/n) (↩️  after input))
```
