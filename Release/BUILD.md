# Building Releases

For all the release builds the starting directory is the GOATracer solution directory.

## Debian

## Windows

As of writing this needs to be done on a Windows machine as the Wix toolkit is broken on Linux.

1. Install the wix toolkit:

```bash
dotnet tool install --global wix
wix extension add -g WixToolset.UI.wixext
```

2. Build the avalonia app:

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ../../Release/Windows/Build/
```

3. Change to the Windows release directory:

```bash
cd ../../Release/Windows
```

4. Build the .msi package:

```bash
wix build Package.wxs -ext WixToolset.UI.wixext -o ./Build/GOATracer.msi
```
