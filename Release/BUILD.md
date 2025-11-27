# Building Releases

This document gives some tips how to build releases of GOATracer.

For all the release builds the starting directory is the GOATracer solution directory.

## Debian

Create a folder for your release:

```bash
mkdir -p ../../Release/Debian/<VERSION-NAME>
```

Create necessary file structure for the .deb within your new folder:

```bash
── DEBIAN
│   └── control (.deb control file, already present)
└── usr
    ├── bin
    │   └── GOATracer
    │       └── GOATracer (Launcher script)
    ├── lib
    │   └── GOATracer
    │       ├── (Project files here)
    └── share
        └── applications
            └── goatracer.desktop (Desktop shortcut)

```

Build for linux:

```bash
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ../../Release/Debian/<VERSION-NAME>/usr/lib/GOATracer/
```

Copy the icon into the build folder:

```bash
cp Assets/icon.ico ../../Release/Debian/<VERSION-NAME>/usr/lib/GOATracer/
```

Open the launcher script and add the following contents:

```bash
#!/bin/bash
# use exec to not have the wrapper script staying as a separate process
# "$@" to pass command line arguments to the app
exec /usr/lib/GOATracer/GOATracer "$@"
```

Open the control file and add the following contents:

```bash
Package: goatracer
Version: <VERSION-NAME>
Section: graphics
Priority: optional
Architecture: amd64
Depends: libx11-6, libice6, libsm6, libfontconfig1, ca-certificates, tzdata, libc6, libgcc1 | libgcc-s1, libgssapi-krb5-2, libstdc++6, zlib1g, libssl1.0.0 | libssl1.0.2 | libssl1.1 | libssl3, libicu | libicu74 | libicu72 | libicu71 | libicu70 | libicu69 | libicu68 | libicu67 | libicu66 | libicu65 | libicu63 | libicu60 | libicu57 | libicu55 | libicu52, libgl1, libxi6, libxcursor1, libxrandr2
Maintainer: Your name <email@example.com>
Description: A 3D scene raytracer.

```

Make sure there is an empty line at the end.

Open the desktop file and add the following contents:

```bash
[Desktop Entry]
Type=Application
Name=GOATracer
Comment=Starts GOATracer
Exec=/usr/lib/GOATracer/GOATracer
Icon=/usr/lib/GOATracer/icon.ico
Terminal=false
Categories=Graphics;
```

Make sure the files have the correct permissions:

```bash
find ../../Release/Debian/<VERSION-NAME> -type d -exec chmod 755 {} \;
find ../../Release/Debian/<VERSION-NAME> -type f -exec chmod 644 {} \;
chmod 755 ../../Release/Debian/<VERSION-NAME>/usr/lib/GOATracer/GOATracer
chmod 755 ../../Release/Debian/<VERSION-NAME>/usr/bin/goatracer
```

Change to the Debian Release directory:

```bash
cd ../../Release/Debian/
```

Package the .deb file:

```bash
dpkg-deb --root-owner-group --build GOATracer-0.0.1-alpha-amd64
```

The .deb file will be in your current directory.

## Windows

As of writing this needs to be done on a Windows machine as the Wix toolkit is broken on Linux.

Install the wix toolkit:

```bash
dotnet tool install --global wix
wix extension add -g WixToolset.UI.wixext
```

Build the avalonia app:

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ../../Release/Windows/bin/
```

Change to the Windows release directory:

```bash
cd ../../Release/Windows
```

Build the .msi package:

```bash
wix build Package.wxs -ext WixToolset.UI.wixext -o ./bin/GOATracer.msi
```
