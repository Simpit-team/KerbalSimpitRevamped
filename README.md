# Kerbal Sim Pit

This is a [Kerbal Space Program](https://kerbalspaceprogram.com/) plugin
to enable communication with devices over a serial connection.

It works with an accompanying [Arduino library](https://bitbucket.org/pjhardy/kerbalsimpit-arduino)
to make building hardware devices simpler.

## Usage

Refer to the [wiki](https://bitbucket.org/pjhardy/kerbalsimpit/wiki/Home)
for installation and usage.

## Building

Kerbal Sim Pit is entirely developed and built using command-line utilities.
IDEs such as Visual Studio and MonoDevelop / Xamarin Studio aren't really
supported, and some minor work is required to compile using them.

### Compilation prerequisites

Ensure the following utilites are installed:

* [Mono](http://www.mono-project.com/). The full distribution is required,
  including the .NET 3.5 reference assemblies.
* [GNU Make](https://www.gnu.org/software/make/).
* [GNU M4](https://www.gnu.org/software/m4/m4.html).

On Debian and Ubuntu systems these can be installed by running

    apt install mono-complete mono-reference-assemblies-3.5 build-essential

On MacOS, these are provided by installing the most recent package from the
Mono website, and the XCode command-line tools.

I've never tried building this package under Windows, but suspect it can be
done by installing the full Mono package, and installing a
[Cygwin](https://www.cygwin.com/) environment, including make and m4.

### Compiling

Open a terminal, navigate to the root Kerbal Sim Pit folder, and run

    make
    
If a commandline `zip` tool is installed, `make package` will generate a
release zip file. If KSPDIR is set appropriately in the Makefile,
`make install` will build a new dll and copy it in to your KSP GameData,
which can be useful for testing builds.

### Building with an IDE

The build process includes a `Properties/AssemblyInfo.cs` file, but the
usual build process generates this file from `Properties/AssemblyInfo.cs.m4`.
To build in an IDE this file will have to be created. I suggest copying
`AssemblyInfo.cs.m4` to `AssemblyInfo.cs`, and editing it. The only things
that should need to be set are the `AssemblyVersion`, `AssemblyFileVersion`
and `KSPAssembly` attributes at the bottom of the file.

## TODO

* Begin implementing actual data handlers.
* Flesh out Arduino library, and fix the warnings that break compile on
  some platforms (like teensy).
* Runtime configuration of serial ports.

## Planned features

* Small, lightweight communication. Data items are sent individually, on
an as-needed basis, rather than regular monolithic data packets.
* Dynamic configuration. Serial ports and speeds are configured through an
in-game UI. Supported features are then negotiated between the plugin and
the connected device.
* Multiple devices. The plugin supports an arbitrary number of serial
connections, each with individual input and output configuration.
* Easily extensible. New functionality should be provided to the plugin by
including small classes with a well-defined interface. It should also
support extending capabilities by external add-ons.
